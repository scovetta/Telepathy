//------------------------------------------------------------------------------
// <copyright file="TemplateFormatterProvider.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   Inherit and implement this class to provide instances of
//   the TemplateFormatter class
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics
{
    using System.Collections.Generic;

    /// <summary>
    /// An abstract class that the custom generated template formatter provider
    /// derives from to provide instances of TemplateFormatter class according
    /// to the given event id
    /// </summary>
    public abstract class TemplateFormatterProvider
    {
        /// <summary>
        /// When overridden in a derived class, gets the dictionary
        /// which contains all instances of TemplateFormatter class,
        /// keyed by the event id
        /// </summary>
        protected abstract IDictionary<int, TemplateFormatter> TemplateFormatterDic { get; }

        /// <summary>
        /// Gets the instance of the TemplateFormatter class
        /// </summary>
        /// <param name="eventId">indicating the event id</param>
        /// <returns>returns the instance for the given event id</returns>
        public TemplateFormatter GetTemplateFormatter(int eventId)
        {
            return this.TemplateFormatterDic[eventId];
        }
    }
}
