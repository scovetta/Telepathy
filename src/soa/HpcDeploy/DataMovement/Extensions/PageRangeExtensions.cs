//------------------------------------------------------------------------------
// <copyright file="PageRangeExtensions.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Extensions methods for PageRange class for use with BlobTransfer.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.Extensions
{
    using System;
    using System.Collections.Generic;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Extensions methods for PageRange class for use with BlobTransfer.
    /// </summary>
    internal static class PageRangeExtensions
    {
        /// <summary>
        /// Split a PageRange into multiple PageRange objects, each at most maxPageRangeSize long.
        /// </summary>
        /// <param name="pageRange">PageRange object to split.</param>
        /// <param name="maxPageRangeSize">Maximum length for each piece.</param>
        /// <returns>List of PageRange objects.</returns>
        public static IEnumerable<PageRange> SplitRanges(this PageRange pageRange, long maxPageRangeSize)
        {
            long startOffset = pageRange.StartOffset;
            long rangeSize = pageRange.EndOffset - pageRange.StartOffset + 1;

            do
            {
                PageRange subRange = new PageRange(startOffset, startOffset + Math.Min(rangeSize, maxPageRangeSize) - 1);

                startOffset += maxPageRangeSize;
                rangeSize -= maxPageRangeSize;

                yield return subRange;
            }
            while (rangeSize > 0);
        }

        /// <summary>
        /// Splits each of the PageRange objects in the supplied list into one or more PageRange objects, each at most maxPageRangeSize long.
        /// </summary>
        /// <param name="pageRanges">PageRange objects to split.</param>
        /// <param name="maxPageRangeSize">Maximum length for each piece.</param>
        /// <returns>List of PageRange objects.</returns>
        public static IEnumerable<PageRange> SplitRanges(this IEnumerable<PageRange> pageRanges, long maxPageRangeSize)
        {
            foreach (PageRange pageRange in pageRanges)
            {
                foreach (PageRange subRange in pageRange.SplitRanges(maxPageRangeSize))
                {
                    yield return subRange;
                }
            }
        }        
    }
}
