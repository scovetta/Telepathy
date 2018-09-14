//------------------------------------------------------------------------------
// <copyright file="Utils.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Utility methods.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement
{
    using System;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// Class for various utils.
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// Define the various possible size postfixes.
        /// </summary>
        private static string[] sizeFormats = 
        {
            Resources.ReadableSizeFormatBytes, 
            Resources.ReadableSizeFormatKiloBytes, 
            Resources.ReadableSizeFormatMegaBytes, 
            Resources.ReadableSizeFormatGigaBytes, 
            Resources.ReadableSizeFormatTeraBytes, 
            Resources.ReadableSizeFormatPetaBytes, 
            Resources.ReadableSizeFormatExaBytes 
        };

        /// <summary>
        /// Translate a size in bytes to human readable form.
        /// </summary>
        /// <param name="size">Size in bytes.</param>
        /// <returns>Human readable form string.</returns>
        public static string BytesToHumanReadableSize(double size)
        {
            int order = 0;

            while (size >= 1024 && order + 1 < sizeFormats.Length)
            {
                ++order;
                size /= 1024;
            }

            return string.Format(CultureInfo.InvariantCulture, sizeFormats[order], size);
        }

        /// <summary>
        /// Append snapshot time to a file name.
        /// </summary>
        /// <param name="fileName">Original file name.</param>
        /// <param name="snapshotTime">Snapshot time to append.</param>
        /// <returns>A file name with appended snapshot time.</returns>
        public static string AppendSnapShotToFileName(string fileName, DateTimeOffset? snapshotTime)
        {
            string resultName = fileName;

            if (snapshotTime.HasValue)
            {
                string pathAndFileNameNoExt = Path.ChangeExtension(fileName, null);
                string extension = Path.GetExtension(fileName);
                string timeStamp = string.Format("{0:yyyy-MM-dd HHmmss fff}", snapshotTime.Value);

                resultName = string.Format(
                    "{0} ({1}){2}",
                    pathAndFileNameNoExt,
                    timeStamp,
                    extension);
            }

            return resultName;
        }
    }
}
