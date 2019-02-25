//------------------------------------------------------------------------------
// <copyright file="AbstractSessionFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Factory class to create session instance
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;
    /// <summary>
    /// Factory class to create session instance
    /// </summary>
    public abstract class AbstractSessionFactory
    {
        /// <summary>
        /// Default library directory path on on-premise installation and Azure VM role
        /// </summary>
        private const string HpcAssemblyDir = @"%CCP_HOME%bin";

        /// <summary>
        /// Default library directory path on Azure worker role
        /// </summary>
        private const string HpcAssemblyDir2 = @"%CCP_HOME%";

        /// <summary>
        /// Storage client library name
        /// </summary>
        private const string StorageClientAssemblyName = "Microsoft.WindowsAzure.Storage.dll";

        static AbstractSessionFactory()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveHandler;
        }

        /// <summary>
        /// Override to implement real logic to create session
        /// </summary>
        /// <param name="startInfo">indicating session start information</param>
        /// <param name="durable">indicating a value whether a durable session is to be created</param>
        /// <param name="timeoutMilliseconds">indicating the timeout</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>returns session instance</returns>
        public abstract Task<SessionBase> CreateSession(SessionStartInfo startInfo, bool durable, int timeoutMilliseconds, Binding binding);

        /// <summary>
        /// Override to implement real logic to attach to an existing session
        /// </summary>
        /// <param name="attachInfo">indicating the session attach information</param>
        /// <param name="durable">indicating a value whether a durable session is to be created</param>
        /// <param name="timeoutMilliseconds">indicating the timeout</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>returns session instance</returns>
        public abstract Task<SessionBase> AttachSession(SessionAttachInfo attachInfo, bool durable, int timeoutMilliseconds, Binding binding);

        /// <summary>
        /// Attach the broker
        /// </summary>
        /// <param name="startInfo">session start info</param>
        /// <param name="sessionInfo">session info</param>
        /// <param name="durable">whether durable session</param>
        /// <param name="timeoutMilliseconds">attach timeout</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns></returns>
        public abstract Task<SessionBase> AttachBroker(SessionStartInfo startInfo, SessionInfoContract sessionInfo, bool durable, int timeoutMilliseconds, Binding binding);

        /// <summary>
        /// Load the assembly from some customized path, if it cannot be found automatically.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">A System.ResolveEventArgs that contains the event data.</param>
        /// <returns>targeted assembly</returns>
        private static Assembly ResolveHandler(object sender, ResolveEventArgs args)
        {
            if (string.IsNullOrEmpty(args.Name))
            {
                return null;
            }

            // Session API assembly may be installed in GAC, or %CCP_HOME%bin,
            // or "%CCP_HOME%"; while Microsoft.WindowsAzure.Storage.dll
            // may be installed in %CCP_HOME%bin, or "%CCP_HOME%".  If they are
            // located at different places, we need load it from target folder
            // explicitly
            AssemblyName targetAssemblyName = new AssemblyName(args.Name);
            if (targetAssemblyName.Name.Equals(Path.GetFileNameWithoutExtension(StorageClientAssemblyName), StringComparison.OrdinalIgnoreCase))
            {
                string assemblyPath = Path.Combine(Environment.ExpandEnvironmentVariables(HpcAssemblyDir), StorageClientAssemblyName);
                if (!File.Exists(assemblyPath))
                {
                    assemblyPath = Path.Combine(Environment.ExpandEnvironmentVariables(HpcAssemblyDir2), StorageClientAssemblyName);
                }
                if (!File.Exists(assemblyPath))
                {
                    return null;
                }

                try
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
                catch (Exception ex)
                {
                    SessionBase.TraceSource.TraceInformation(
                        "[BlobDataContainer] .ResolveHandler: failed to load assembly {0}: {1}",
                        assemblyPath, ex);
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Build broker factory
        /// </summary>
        protected static IBrokerFactory BuildBrokerFactory(SessionStartInfo startInfo, bool durable)
        {
            if (startInfo.UseInprocessBroker)
            {
                return new InprocessBrokerFactory(startInfo.Headnode, durable);
            }
            else
            {
                return new V3BrokerFactory(durable);
            }
        }

        /// <summary>
        /// Build broker factory
        /// </summary>
        protected static IBrokerFactory BuildBrokerFactory(SessionAttachInfo attachInfo, bool durable)
        {
            if (attachInfo.UseInprocessBroker)
            {
                return new InprocessBrokerFactory(attachInfo.Headnode, durable);
            }
            else
            {
                return new V3BrokerFactory(durable);
            }
        }
    }
}
