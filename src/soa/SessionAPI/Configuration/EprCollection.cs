// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Configuration
{
    using System.Configuration;
    using System.Diagnostics;

    /// <summary>
    /// Represents the collection of EPRs
    /// </summary>
    public class EprCollection : ConfigurationElementCollection
    {
        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new EprElement();
        }

        /// <summary>
        /// Gets element key for epr element
        /// </summary>
        /// <param name="element">indicating the epr element</param>
        /// <returns>returns the epr as element key</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            Debug.Assert(element is EprElement, "[EprCollection] Element must be an instance of type EprCollection.");
            return ((EprElement)element).Epr;
        }
    }
}
