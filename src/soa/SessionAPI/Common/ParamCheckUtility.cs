// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Helper class to throw exceptions
    /// </summary>
    public static class ParamCheckUtility
    {
        /// <summary>
        /// Stores the regex checking client id
        /// </summary>
        private static Regex clientIdValid = new Regex(@"^[0-9a-zA-Z_\-\{\}\s]*$");

        /// <summary>
        /// Stores the regex checking DataClient ID
        /// </summary>
        private static Regex dataClientIdValid = new Regex(@"^[0-9a-zA-Z_\-\{\}\s]*[0-9a-zA-Z_\-\{\}]$");

        /// <summary>
        /// Gets the regex checking client id
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file.")]
        public static Regex ClientIdValid
        {
            get { return clientIdValid; }
        }

        /// <summary>
        /// Gets the regex checking DataClient ID
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file.")]
        public static Regex DataClientIdValid
        {
            // DataClient ID shares the same valid character set with broker client id
            get { return dataClientIdValid; }
        }

        /// <summary>
        /// Throws ArgumentNullException if arg string is empty
        /// </summary>
        /// <param name="arg">string argument to check</param>
        /// <param name="name">Name of argument to check</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file.")]
        public static void ThrowIfEmpty(string arg, string name)
        {
            if (arg != null && arg.Equals(string.Empty))
            {
                throw new ArgumentException(name);
            }
        }

        /// <summary>
        /// Throws ArgumentNullException if arg is null
        /// </summary>
        /// <param name="arg">Argument to check</param>
        /// <param name="name">Name of argument to check</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file.")]
        public static void ThrowIfNull(object arg, string name)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// Throws ArgumentNullException if arg is null or empty
        /// </summary>
        /// <param name="arg">Argument to check</param>
        /// <param name="name">Name of argument to check</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file.")]
        public static void ThrowIfNullOrEmpty(string arg, string name)
        {
            if (String.IsNullOrEmpty(arg))
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// Throws ArgumentOutOfRangeException if out of range
        /// </summary>
        /// <param name="outOfRange">indicating whether it's out of range</param>
        /// <param name="name">Name of argument to check</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file.")]
        public static void ThrowIfOutofRange(bool outOfRange, string name)
        {
            if (outOfRange)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        /// <summary>
        /// Throws ArgumentException if specified len is too large
        /// </summary>
        /// <param name="len">Length to check</param>
        /// <param name="name">Name of argument</param>
        /// <param name="maxLen">Maxium length</param>
        /// <param name="message">Error message</param>
        /// <param name="args">Error message arguments</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file.")]
        public static void ThrowIfTooLong(int len, string name, int maxLen, string message, params object[] args)
        {
            if (len > maxLen)
            {
                string errorMessage = args == null || args.Length == 0 ? message : String.Format(CultureInfo.CurrentCulture, message, args);
                throw new ArgumentException(errorMessage, name);
            }
        }

        /// <summary>
        /// Throws AugumentException if client id is not valid
        /// </summary>
        /// <param name="reg">indicating the regular expression</param>
        /// <param name="str">indicating the string</param>
        /// <param name="name">indicating the param name</param>
        /// <param name="message">indicating the error message</param>
        /// <param name="args">indicating the error message arguments</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file.")]
        public static void ThrowIfNotMatchRegex(Regex reg, string str, string name, string message, params object[] args)
        {
            if (!reg.IsMatch(str))
            {
                string errorMessage = args == null || args.Length == 0 ? message : String.Format(CultureInfo.CurrentCulture, message, args);
                throw new ArgumentException(errorMessage, name);
            }
        }

        /// <summary>
        /// Ensures service version is not 0.0 for Major.Minor
        /// </summary>
        /// <param name="version">indicating the version</param>
        /// <returns>returns a value indicating whether the service version is Major.Minor<.Build><.Revision></returns>
        public static bool IsServiceVersionValid(Version version)
        {
            // 0.0 is not allowed
            return !(version.Major == 0 && version.Minor == 0);
        }
    }
}
