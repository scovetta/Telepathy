using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Hpc.Scheduler.Session
{
    /// <summary>
    /// HPC implementation of <see cref="XmlWriterTraceListener"/>
    /// </summary>
    public class HpcTraceListener : XmlWriterTraceListener
    {
        /// <summary>
        /// Constructor of <see cref="HpcTraceListener"/>
        /// </summary>
        /// <param name="filename">
        ///   <para />
        /// </param>
        public HpcTraceListener(string filename)
            : base(ExtendName(filename))
        {

        }

        /// <summary>
        /// Constructor of <see cref="HpcTraceListener"/>
        /// </summary>
        /// <param name="filename">
        ///   <para />
        /// </param>
        /// <param name="name">
        ///   <para />
        /// </param>
        public HpcTraceListener(string filename, string name)
            : base(ExtendName(filename), name)
        {
        }

        /// <summary>
        /// Append the jobid_taskid_ in front of the filename
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static string ExtendName(string param)
        {
            string name = System.Environment.ExpandEnvironmentVariables(param);
            string jobid = System.Environment.GetEnvironmentVariable("CCP_JOBID");
            string instanceid = System.Environment.GetEnvironmentVariable("CCP_TASKINSTANCEID");
            string taskid = System.Environment.GetEnvironmentVariable("CCP_TASKID");

            if (String.IsNullOrEmpty(jobid))
            {
                jobid = "0";
            }

            if (String.IsNullOrEmpty(instanceid))
            {
                if (String.IsNullOrEmpty(taskid))
                {
                    // for broker
                    return Path.Combine(Path.GetDirectoryName(name), jobid + "_" + Path.GetFileName(name));
                }
                else
                {
                    return Path.Combine(Path.GetDirectoryName(name), jobid + "_" + taskid + "_" + Path.GetFileName(name));
                }
            }
            else
            {
                if (String.IsNullOrEmpty(taskid))
                {
                    taskid = "0";
                }

                return Path.Combine(Path.GetDirectoryName(name), jobid + "_" + taskid + "." +
                    instanceid + "_" + Path.GetFileName(name));
            }
        }
    }

}
