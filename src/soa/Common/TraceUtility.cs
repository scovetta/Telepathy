//------------------------------------------------------------------------------
// <copyright file="TraceUtility.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Utility functions for tracing
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Utility functions for tracing
    /// </summary>
    internal static class TraceUtility
    {
        /// <summary>
        /// Format array into a string
        /// </summary>
        /// <typeparam name="T">indicating the item type</typeparam>
        /// <param name="array">indicating the array to be formated</param>
        /// <returns>returns the formatted string</returns>
        public static string FormatArray<T>(IEnumerable<T> array)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T item in array)
            {
                sb.Append(item.ToString());
                sb.Append(',');
            }

            // Remove the last ','
            return sb.ToString().TrimEnd(',');
        }
    }
}
