//------------------------------------------------------------------------------
// <copyright file="StringParameter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Parameter for string type value
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.CcpServiceHosting
{
    /// <summary>
    /// parameter for string type argument
    /// </summary>
    internal class StringParameter : Parameter
    {
        /// <summary>
        /// parameter value in string type
        /// </summary>
        private string value;

        /// <summary>
        /// Initializes a new instance of the StringParameter class.
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="description">parameter description</param>
        public StringParameter(string name, string description)
            : base(name, description)
        {
        }

        /// <summary>
        /// Gets parameter value
        /// </summary>
        public override object Value
        {
            get
            {
                return this.value;
            }
        }

        /// <summary>
        /// Parse string type value.
        /// </summary>
        /// <param name="value">string type value in command line arguments</param>
        protected override void ParseValue(string value)
        {
            this.value = value;
        }
    }
}
