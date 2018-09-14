//------------------------------------------------------------------------------
// <copyright file="AzureToFileSystemFileNameResolver.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      File name resolver class for translating Azure file names to Windows file names.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.RecursiveTransferHelpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// File name resolver class for translating Azure file names to Windows file names.
    /// </summary>
    internal class AzureToFileSystemFileNameResolver : IFileNameResolver
    {
        /// <summary>
        /// Default delimiter used as a directory separater in blob name.
        /// </summary>
        private const char DefaultDelimiter = '/';

        /// <summary>
        /// These filenames are reserved on windows, regardless of the file extension.
        /// </summary>
        private static readonly string[] reservedBaseFileNames = new string[]
            {
                "CON", "PRN", "AUX", "NUL", 
                "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", 
                "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
            };

        /// <summary>
        /// These filenames are reserved on windows, only if the full filenamem matches.
        /// </summary>
        private static readonly string[] reservedFileNames = new string[]
            {
                "CLOCK$",
            };

        /// <summary>
        /// Chars invalid for file name.
        /// </summary>
        private static char[] invalidFileNameChars = Path.GetInvalidFileNameChars();

        /// <summary>
        /// Chars invalid for path name.
        /// </summary>
        private static char[] invalidPathChars = AzureToFileSystemFileNameResolver.GetInvalidPathChars();

        /// <summary>
        /// Here lists special characters in regular expression,
        /// those characters need to be escaped in regular expression.
        /// </summary>
        private static HashSet<char> regexSpecialCharacters = new HashSet<char>(
            new char[] { '*', '^', '$', '(', ')', '+', '|', '[', ']', '"', '.', '/', '?', '\\' });

        /// <summary>
        /// Regular expression string format for replacing delimiters 
        /// that we consider as directory separators:
        /// <para>Translate delimiters to '\' if it is:
        /// not the first or the last character in the file name 
        /// and not following another delimiter</para>
        /// <example>/abc//def/ with '/' as delimiter gets translated to: /abc\/def/ </example>.
        /// </summary>
        private static string translateDelimitersRegexFormat = "(?<=[^{0}]+){0}(?=.+)";

        /// <summary>
        /// Regular expression for replacing delimiters that we consider as directory separators:
        /// <para>Translate delimiters to '\' if it is:
        /// not the first or the last character in the file name 
        /// and not following another delimiter</para>
        /// <example>/abc//def/ with '/' as delimiter gets translated to: /abc\/def/ </example>.
        /// </summary>
        private Regex translateDelimitersRegex;

        private Dictionary<string, string> resolvedFilesCache = new Dictionary<string, string>();

        private Func<int> getMaxFileNameLength;

        private char delimiter;

        public AzureToFileSystemFileNameResolver(Func<int> getMaxFileNameLength, char? delimiter)
        {
            this.getMaxFileNameLength = getMaxFileNameLength;
            this.delimiter = null == delimiter ? DefaultDelimiter : delimiter.Value;

            // In azure storage, it will transfer every '\' to '/', so '\' won't be a delimiter.
            if (regexSpecialCharacters.Contains(this.delimiter))
            {
                string delimiterTemp = "\\" + this.delimiter;
                this.translateDelimitersRegex = new Regex(string.Format(translateDelimitersRegexFormat, delimiterTemp), RegexOptions.Compiled);
            }
            else
            { 
                this.translateDelimitersRegex = new Regex(string.Format(translateDelimitersRegexFormat, this.delimiter), RegexOptions.Compiled);
            }
        }

        public string ResolveFileName(FileEntry sourceEntry)
        {
            // 1) Unescape original string, original string is UrlEncoded.
            // 2) Replace Azure directory separator with Windows File System directory separator.
            // 3) Trim spaces at the end of the file name.
            string destinationRelativePath = EscapeInvalidCharacters(this.TranslateDelimiters(sourceEntry.RelativePath).TrimEnd(new char[] { ' ' }), invalidPathChars);

            // Split into path + filename parts.
            int lastSlash = destinationRelativePath.LastIndexOf('\\');

            string destinationFileName;
            string destinationPath;

            if (-1 == lastSlash)
            {
                destinationPath = string.Empty;
                destinationFileName = destinationRelativePath;
            }
            else
            {
                destinationPath = destinationRelativePath.Substring(0, lastSlash + 1);
                destinationFileName = destinationRelativePath.Substring(lastSlash + 1);
            }

            // Append snapshot time to filename.
            destinationFileName = Utils.AppendSnapShotToFileName(destinationFileName, sourceEntry.SnapshotTime);

            // Combine path and filename back together again.
            destinationRelativePath = Path.Combine(destinationPath, destinationFileName);

            // Check if the destination name is 
            // - already used by a previously resolved file.
            // - or represents a reserved filename on the target file system.
            // - or is longer than the allowed path length on the target file system.
            // If this is the case add a numeric prefix to resolve the conflict.
            destinationRelativePath = this.ResolveFileNameConflict(destinationRelativePath);

            // Add the resolved name to the resolved files cache, so additional files
            // will not use the same target name to download to.
            this.resolvedFilesCache.Add(destinationRelativePath.ToLowerInvariant(), destinationRelativePath);

            return destinationRelativePath;
        }

        private static char[] GetInvalidPathChars()
        {
            // Union InvalidFileNameChars and InvalidPathChars together
            // while excluding slash.
            HashSet<char> charSet = new HashSet<char>(Path.GetInvalidPathChars());

            foreach (char c in invalidFileNameChars)
            {
                if ('\\' == c || charSet.Contains(c))
                {
                    continue;
                }

                charSet.Add(c);
            }

            invalidPathChars = new char[charSet.Count];
            charSet.CopyTo(invalidPathChars);

            return invalidPathChars;
        }

        private static string EscapeInvalidCharacters(string fileName, params char[] invalidChars)
        {
            if (null != invalidChars)
            {
                // Replace invalid characters with %HH, with HH being the hexadecimal
                // representation of the invalid character.
                foreach (char c in invalidChars)
                {
                    fileName = fileName.Replace(c.ToString(), string.Format("%{0:X2}", (int)c));
                }
            }

            return fileName;
        }

        private static bool IsReservedFileName(string fileName)
        {
            string fileNameNoExt = Path.GetFileNameWithoutExtension(fileName);
            string fileNameWithExt = Path.GetFileName(fileName);

            if (Array.Exists<string>(reservedBaseFileNames, delegate(string s) { return fileNameNoExt.Equals(s, StringComparison.OrdinalIgnoreCase); }))
            {
                return true;
            }

            if (Array.Exists<string>(reservedFileNames, delegate(string s) { return fileNameWithExt.Equals(s, StringComparison.OrdinalIgnoreCase); }))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return true;
            }

            bool allDotsOrWhiteSpace = true;
            for (int i = 0; i < fileName.Length; ++i)
            {
                if (fileName[i] != '.' && !char.IsWhiteSpace(fileName[i]))
                {
                    allDotsOrWhiteSpace = false;
                    break;
                }
            }

            if (allDotsOrWhiteSpace)
            {
                return true;
            }

            return false;
        }

        private string TranslateDelimiters(string source)
        {
            // Transform delimiters used for directory separators to windows file system directory separator "\".
            return this.translateDelimitersRegex.Replace(source, "\\");
        }

        private string ResolveFileNameConflict(string baseFileName)
        {
            // TODO - MaxFileNameLength could be <= 0.
            int maxFileNameLength = this.getMaxFileNameLength();

            Func<string, bool> conflict = delegate(string fileName)
            {
                return this.resolvedFilesCache.ContainsKey(fileName.ToLowerInvariant()) ||
                       IsReservedFileName(fileName) ||
                       fileName.Length > maxFileNameLength;
            };

            Func<string, string, int, string> construct = delegate(string fileName, string extension, int count)
            {
                string postfixString = string.Format(" ({0})", count);

                // TODO - trimLength could be be larger than pathAndFilename.Length, what do we do in this case?
                int trimLength = (fileName.Length + postfixString.Length + extension.Length) - maxFileNameLength;

                if (trimLength > 0)
                {
                    fileName = fileName.Remove(fileName.Length - trimLength);
                }

                return string.Format("{0}{1}{2}", fileName, postfixString, extension);
            };

            return FileNameResolver.ResolveFileNameConflict(baseFileName, conflict, construct);
        }
    }
}
