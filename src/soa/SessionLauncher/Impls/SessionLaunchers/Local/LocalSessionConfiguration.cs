// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher.Impls.SessionLaunchers.Local
{
    internal static class LocalSessionConfiguration
    {
        private static int currentSessionId = 0;

        public static string BrokerLauncherExePath { get; set; }

        public static string ServiceHostExePath { get; set; }

        public static string ServiceRegistrationPath { get; set; }

        public static string BrokerStorageConnectionString { get; set; }

        public static int GetNextSessionId()
        {
            currentSessionId = currentSessionId + 1;
            return currentSessionId;
        }
    }
}
