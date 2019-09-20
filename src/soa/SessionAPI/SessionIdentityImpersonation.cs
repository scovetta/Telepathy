// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session
{
    using System;
    using System.Diagnostics;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.Threading;

    /// <summary>
    /// Save information of impersonation in session API
    /// </summary>
    public class SessionIdentityImpersonation : IDisposable
    {
        private WindowsImpersonationContext impersonationContext;

        /// <summary>
        /// Constructor of <see cref="SessionIdentityImpersonation"/>
        /// </summary>
        /// <param name="useAad"></param>
        public SessionIdentityImpersonation(bool useAad)
        {
            if (useAad)
            {
                Trace.TraceInformation($"[{nameof(SessionIdentityImpersonation)}] Try to impersonate as AAD user. CurrentPrincipal={Thread.CurrentPrincipal?.Identity?.Name}.");
            }
            else if (OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.AuthByKerberosOrNtlmOrBasic())
            {
                this.impersonationContext = ServiceSecurityContext.Current.WindowsIdentity.Impersonate();
                Trace.TraceInformation($"[{nameof(SessionIdentityImpersonation)}] Try to impersonate as Windows user {ServiceSecurityContext.Current.WindowsIdentity.Name}.");
            }
            else
            {
                // X509identity goes here
                if (!this.ValidX509Identity())
                {
                    Trace.TraceError($"[{nameof(SessionIdentityImpersonation)}] X509 Validation failed.");
                    throw new InvalidOperationException($"{nameof(SessionIdentityImpersonation)}: X509 Validation failed.");
                }

                Trace.TraceInformation($"[{nameof(SessionIdentityImpersonation)}] Current identity is X509 identity. Not doing impersonation.");
            }
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public void Dispose()
        {
            this.impersonationContext?.Dispose();
            this.impersonationContext = null;

            // Suppress finalization of this disposed instance.
            GC.SuppressFinalize(this);
        }

        private bool ValidX509Identity()
        {
            // Not implemented
            return true;
        }
    }
}
