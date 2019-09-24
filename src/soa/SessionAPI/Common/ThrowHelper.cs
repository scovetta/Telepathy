// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Common
{
    using System;
    using System.Globalization;
    using System.ServiceModel;

    using Microsoft.Telepathy.Session.Exceptions;

    /// <summary>
    /// Helper class to throw exception
    /// </summary>
    public static class ThrowHelper
    {
        /// <summary>
        /// Throws SessionFault exception
        /// </summary>
        /// <param name="code">indicating the code</param>
        /// <param name="reason">indicating the reason</param>
        /// <param name="context">indicating the context</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This function is shared in multiple projects")]
        public static void ThrowSessionFault(int code, string reason, params string[] context)
        {
            if (context == null)
            {
                throw new FaultException<SessionFault>(new SessionFault(code, reason, context), reason);
            }
            else
            {
                throw new FaultException<SessionFault>(new SessionFault(code, reason, context), String.Format(CultureInfo.CurrentCulture, reason, context));
            }
        }

        /// <summary>
        /// Throw proper fault exception for unknown error
        /// </summary>
        /// <param name="error">indicating the error</param>
        public static void ThrowUnknownError(string error)
        {
            ThrowSessionFault(SOAFaultCode.UnknownError, "{0}", error);
        }
    }
}
