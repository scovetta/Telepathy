//------------------------------------------------------------------------------
// <copyright file="Parameter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Abstract parameter
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.CcpServiceHosting
{
    using System;
    using System.Globalization;

    /// <summary>
    /// abstract parameter inherited by parameters in different types
    /// </summary>
    internal abstract class Parameter
    {
        /// <summary>
        /// parameter name
        /// </summary>
        private string name;

        /// <summary>
        /// help description of parameter
        /// </summary>
        private string description;

        /// <summary>
        /// Initializes a new instance of the Parameter class.
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="description">parameter description</param>
        public Parameter(string name, string description)
        {
            this.name = name;
            this.description = description;
        }

        /// <summary>
        /// Gets parameter name
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Gets parameter value
        /// </summary>
        public abstract object Value
        {
            get;
        }

        /// <summary>
        /// Gets parameter description
        /// </summary>
        public string Description
        {
            get { return this.description; }
        }

        /// <summary>
        /// output the help info of parameter
        /// </summary>
        public void PrintHelp()
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "    {0, -25} - {1}", this.name, this.description));
        }

        /// <summary>
        /// parse args to get parameter value
        /// </summary>
        /// <param name="args">command line arguments</param>
        public virtual void Parse(string[] args)
        {
            bool findParameter = false;

            int i = 0;
            for (; i < args.Length; i++)
            {
                if (this.name.Equals(args[i], StringComparison.OrdinalIgnoreCase))
                {
                    findParameter = true;
                    break;
                }
            }

            if (!findParameter)
            {
                throw new ParameterException(string.Format(CultureInfo.InvariantCulture, "Can't find mandatory parameter {0}.", this.name));
            }

            if (i + 1 >= args.Length)
            {
                throw new ParameterException(string.Format(CultureInfo.InvariantCulture, "Can't find the value of parameter {0}.", this.name));
            }

            this.ParseValue(args[i + 1]);
        }

        /// <summary>
        /// convert string type value to specific type
        /// </summary>
        /// <param name="value">string type value in command line arguments</param>
        protected abstract void ParseValue(string value);
    }
}
