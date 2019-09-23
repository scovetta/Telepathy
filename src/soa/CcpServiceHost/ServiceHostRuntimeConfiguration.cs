// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.CcpServiceHost
{
    using System;

    internal static class ServiceHostRuntimeConfiguration
    {
        private static bool? standalone;

        public static bool Standalone
        {
            get
            {
                if (!standalone.HasValue)
                {
                    throw new InvalidOperationException("Value hasn't been set.");
                }
                else
                {
                    return standalone.Value;
                }
            }

            set
            {
                if (!standalone.HasValue)
                {
                    standalone = value;
                }
                else if (standalone.Value != value)
                {
                    throw new InvalidOperationException($"Standalone has been set to {standalone}.");
                }
            }
        }

        public static string StorageCredential { get; set; }
    }
}
