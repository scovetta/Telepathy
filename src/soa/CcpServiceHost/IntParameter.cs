// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.CcpServiceHost
{
    using System;
    using System.Globalization;

    /// <summary>
    /// parameter for int type argument
    /// </summary>
    internal class IntParameter : Parameter
    {
        /// <summary>
        /// parameter value in int type
        /// </summary>
        private int value;

        /// <summary>
        /// Initializes a new instance of the IntParameter class.
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="description">parameter description</param>
        public IntParameter(string name, string description)
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
        /// Convert string type value to int type
        /// </summary>
        /// <param name="value">string type value in command line arguments</param>
        protected override void ParseValue(string value)
        {
            try
            {
                this.value = int.Parse(value, CultureInfo.InvariantCulture);
            }
            catch (FormatException e)
            {
                throw new ParameterException(string.Format(CultureInfo.InvariantCulture, "The value '{0}' of parameter '{1}' is not a correct integar.", value, this.Name), e);
            }
            catch (OverflowException e)
            {
                throw new ParameterException(string.Format(CultureInfo.InvariantCulture, "The value '{0}' of parameter '{1}' is too big for an integar.", value, this.Name), e);
            }
        }
    }
}
