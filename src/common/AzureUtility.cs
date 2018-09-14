namespace Microsoft.Hpc.Azure.Common
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Management;
    using System.Text;
    using Microsoft.WindowsAzure.ServiceRuntime;

    /// <summary>
    /// Generic Azure Utility methods
    /// </summary>
    public class AzureUtility
    {
        public const string HpcNodeNameEnvStr = "HPC_NODE_NAME";

        public static void ExecuteCommand(string execution, string parameters)
        {
            Trace.TraceInformation("[General] Executing command: {0} {1}", execution, parameters);
            Process process = new Process();
            ProcessStartInfo psi = new ProcessStartInfo();

            psi.FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe");
            psi.Arguments = string.Format("/C \"{0}\" {1}", execution, parameters);

            StringBuilder sbOut = new StringBuilder();

            StringBuilder sbErr = new StringBuilder();

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            process.StartInfo = psi;
            process.EnableRaisingEvents = true;

            process.OutputDataReceived +=
                delegate (object sender, DataReceivedEventArgs evt)
                {
                    if (evt.Data != null)
                    {
                        sbOut.AppendLine(evt.Data);
                    }
                };

            process.ErrorDataReceived +=
                delegate (object sender, DataReceivedEventArgs evt)
                {
                    if (evt.Data != null)
                    {
                        sbErr.AppendLine(evt.Data);
                    }
                };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            // TODO: Change this to configurable
            process.WaitForExit(1000 * 60); // 1 minutes 

            string info = string.Format("[Result: {0}] {1}", process.HasExited ? process.ExitCode.ToString() :
                "Ongoing", sbOut.ToString());
            Trace.TraceInformation("Executing command result {0}", info);

            if (!string.IsNullOrEmpty(sbErr.ToString()))
            {
                Trace.TraceError("Executing command error {0}", sbErr.ToString());
            }

            if (!process.HasExited)
            {
                Trace.TraceInformation("Command did not finish. Killing it and continuing");
                process.Kill();
            }

            process.Close();
        }

        /// <summary>
        /// Make a best-effort to execute a script (take up to 30 minutes for execution -- throw on Timeout)
        /// </summary>
        /// <param name="scriptName"></param>
        /// <param name="packageRoot"></param>
        /// <param name="stdOut"></param>
        /// <param name="stdErr"></param>
        /// <returns></returns>
        public static int ExecuteStartupScript(string scriptName, string packageRoot, out string error)
        {
            error = null;

            string workingDir = Path.Combine(packageRoot, scriptName); // Package sub-folder

            int azureStartupTaskTimeoutSec = 30 * 60; // by default 30 * 60 seconds
            int.TryParse(RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.AzureStartupTaskTimeoutSec), out azureStartupTaskTimeoutSec);

            Process process = new Process();
            ProcessStartInfo psi = new ProcessStartInfo();

            // If the package directory exists, set the process working directory to it
            DirectoryInfo wDir = new DirectoryInfo(workingDir);
            if (!wDir.Exists)
            {
                // If the directory doesn't exist try stripping the extension
                // in-case the admin omitted the package on upload
                wDir = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(workingDir),
                           Path.GetFileNameWithoutExtension(workingDir)));
            }

            if (wDir.Exists)
            {
                DateTime subtime = DateTime.MinValue;
                DirectoryInfo subdir = null;
                foreach (DirectoryInfo dir in wDir.GetDirectories())
                {
                    if (dir.CreationTimeUtc > subtime)
                    {
                        subtime = dir.CreationTimeUtc;
                        subdir = dir;
                    }
                }

                if (subdir != null)
                {
                    workingDir = subdir.FullName;
                    psi.WorkingDirectory = workingDir;
                }
            }

            string scriptPath = Path.Combine(workingDir, scriptName); // Full script path
            scriptPath = scriptPath.TrimEnd('\\');

            psi.FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe");
            psi.Arguments = string.Format("/C \"{0}\"", (File.Exists(scriptPath) ? scriptPath : scriptName));

            StringBuilder sbOut = new StringBuilder();

            StringBuilder sbErr = new StringBuilder();

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            process.StartInfo = psi;
            process.EnableRaisingEvents = true;

            process.OutputDataReceived +=
                delegate (object sender, DataReceivedEventArgs evt)
                {
                    if (evt.Data != null)
                    {
                        sbOut.AppendLine(evt.Data);
                    }
                };

            process.ErrorDataReceived +=
                delegate (object sender, DataReceivedEventArgs evt)
                {
                    if (evt.Data != null)
                    {
                        sbErr.AppendLine(evt.Data);
                    }
                };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit(1000 * azureStartupTaskTimeoutSec);

            string info = string.Format("[Result: {0}] {1}", process.HasExited ? process.ExitCode.ToString() :
                "Ongoing", sbOut.ToString());
            Trace.TraceInformation("StartupScript {0}", info);

            File.AppendAllText(StartupLogFile, info);
            if (!string.IsNullOrEmpty(sbErr.ToString()))
            {
                error = sbErr.ToString();
                File.AppendAllText(StartupErrFile, error);
                Trace.TraceError("StartupScript {0}", sbErr);
            }

            return process.ExitCode;
        }

        //
        // Populate environment variables
        //
        public static void PopulateEnvironmentVariablesBlocking()
        {
            try
            {
                string serializedData = RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.SerializedNodeData);
                AzureNodeData nodeData = AzureNodeData.GetAzureNodeData(serializedData);
                string logicalname = null;
                for (int i = 0; i < 360; i++) // Try to bind logical name for up to an hour
                {
                    logicalname = Environment.GetEnvironmentVariable("LogicalName", EnvironmentVariableTarget.Process);
                    if (logicalname != null) break;
                    System.Threading.Thread.Sleep(10 * 1000);
                }

                string groups = nodeData.GetNodeGroupList(logicalname);
                Environment.SetEnvironmentVariable("HPC_NODE_GROUPS", groups, EnvironmentVariableTarget.Machine);
                Environment.SetEnvironmentVariable("HPC_NODE_GROUPS", groups, EnvironmentVariableTarget.Process);

                if (logicalname != null)
                {
                    Environment.SetEnvironmentVariable("HPC_NODE_NAME", logicalname, EnvironmentVariableTarget.Machine);
                    Environment.SetEnvironmentVariable("HPC_NODE_NAME", logicalname, EnvironmentVariableTarget.Process);
                }
                else
                {
                    logicalname = String.Empty;
                }

                Trace.TraceInformation("Environment Variables blocking populated, HPC_NODE_NAME={0}, HPC_NODE_GROUPS={1}", logicalname, groups);
            }
            catch (Exception e)
            {
                File.AppendAllText(Path.Combine(Environment.GetEnvironmentVariable("WINDIR"), "HpcEnv.err"), e.ToString());
                Trace.TraceError("EnvironmentVariables {0}", e.Message);
            }
        }

        /// <summary>
        /// Disables NetBios on a specific node
        /// </summary>
        public static void DisableNetBios()
        {
            string query = "SELECT * FROM Win32_NetworkAdapterConfiguration";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject nic in searcher.Get())
            {
                try
                {
                    bool ipEnabled = (bool)nic["IPEnabled"];
                    if (ipEnabled)
                    {
                        // Per documentation 2 disables netbios on the NIC
                        Trace.TraceInformation("Disabling NetBios on a NIC.");
                        uint err = (uint)nic.InvokeMethod("SetTcpipNetbios", new object[] { 2 });
                        if (err > 0)
                        {
                            Trace.TraceError("DisableNetBios: Failed to disable NetBios on a NIC. Error code: {0}", err);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError("DisableNetBios: Failed to Disable NetBios on a NIC: {0}", ex);
                }
            }

        }

        //
        // Get path of Startup log file
        //
        public static string StartupLogFile
        {
            get
            {
                string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                path = Path.GetDirectoryName(path);
                path = Path.Combine(path, "HpcStartupCommand.log");
                return path;
            }
        }

        //
        // Get path of Startup err file
        //
        public static string StartupErrFile
        {
            get
            {
                string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                path = Path.GetDirectoryName(path);
                path = Path.Combine(path, "HpcStartupCommand.err");
                return path;
            }
        }

        //
        // Run the startup task
        //
        public static int RunStartupTask(string packageRootDir, out string error)
        {
            try
            {
                string startupScriptPackage = RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.StartupScript);
                if (!string.IsNullOrEmpty(startupScriptPackage))
                {
                    return AzureUtility.ExecuteStartupScript(startupScriptPackage, packageRootDir, out error);
                }
                else
                {
                    error = null;
                    return 0;
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("StartupScript Exception: {0}", e.Message);
                error = e.Message;
                return -1;
            }
        }

    }
}