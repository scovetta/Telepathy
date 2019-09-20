// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.CcpServiceHost
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Containe all the parameters of HpcServiceHost.
    /// </summary>
    internal class ParameterContainer
    {
        /// <summary>
        /// store all the parameters in a list
        /// </summary>
        private List<Parameter> parameters = new List<Parameter>();

        /// <summary>
        /// job id parameter
        /// </summary>
        private IntParameter jobIdParameter = new IntParameter("/JobId", "the job id");

        /// <summary>
        /// task id parameter
        /// </summary>
        private IntParameter taskIdParameter = new IntParameter("/TaskId", "the task id");

        /// <summary>
        /// code id parameter
        /// </summary>
        private IntParameter coreIdParameter = new IntParameter("/CoreId", "the core id");

        /// <summary>
        /// service name parameter
        /// </summary>
        private StringParameter fileNameParameter = new StringParameter("/RegistrationFileName", "the name of the service registration file");

        /// <summary>
        /// service registration path parameter
        /// </summary>
        private StringParameter pathParameter = new StringParameter("/RegistrationPath", "the full path of the service registration folder");

        /// <summary>
        /// command line arguments
        /// </summary>
        private string[] args;

        /// <summary>
        /// Initializes a new instance of the ParameterContainer class.
        /// </summary>
        /// <param name="args">
        /// command line arguments
        /// </param>
        public ParameterContainer(string[] args)
        {
            this.args = args;
            this.parameters.Add(this.jobIdParameter);
            this.parameters.Add(this.taskIdParameter);
            this.parameters.Add(this.coreIdParameter);
            this.parameters.Add(this.fileNameParameter);
            this.parameters.Add(this.pathParameter);
        }

        /// <summary>
        /// Gets job id.
        /// </summary>
        public string JobId
        {
            get
            {
                return this.jobIdParameter.Value.ToString();
            }
        }

        /// <summary>
        /// Gets task id.
        /// </summary>
        public string TaskId
        {
            get
            {
                return this.taskIdParameter.Value.ToString();
            }
        }

        /// <summary>
        /// Gets core id.
        /// </summary>
        public string CoreId
        {
            get
            {
                return this.coreIdParameter.Value.ToString();
            }
        }

        /// <summary>
        /// Gets soa service name.
        /// </summary>
        public string FileName
        {
            get
            {
                return this.fileNameParameter.Value.ToString();
            }
        }

        /// <summary>
        /// Gets full path of service registration folder.
        /// </summary>
        public string RegistrationPath
        {
            get
            {
                return this.pathParameter.Value.ToString();
            }
        }

        /// <summary>
        /// Parse the parameters
        /// </summary>
        public void Parse()
        {
            foreach (Parameter p in this.parameters)
            {
                p.Parse(this.args);
            }
        }

        /// <summary>
        /// Print the help information of command
        /// </summary>
        /// <returns>
        /// print help or not
        /// </returns>
        public bool PrintHelp()
        {
            string[] helpParameters = { "/h", "-h", "/help", "-help", "/?", "-?" };

            bool exist = false;
            foreach (string arg in this.args)
            {
                exist = Array.Exists<string>(
                    helpParameters, 
                    delegate(string value) { return arg.Equals(value, StringComparison.OrdinalIgnoreCase); });
                if (exist)
                {
                    break;
                }
            }

            if (exist)
            {
                Console.WriteLine("Syntax:");
                Console.WriteLine("    HpcServiceHost.exe /JobId <jid> /TaskId <tid> /CoreId <cid> /RegistrationFileName <filename> /RegistrationPath <path>");

                Console.WriteLine();
                Console.WriteLine("Parameters:");
                foreach (Parameter p in this.parameters)
                {
                    p.PrintHelp();
                }

                Console.WriteLine();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
