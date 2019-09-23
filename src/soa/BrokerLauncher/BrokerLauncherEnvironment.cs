// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.BrokerLauncher
{
    internal static class BrokerLauncherEnvironment
    {
        internal static bool Standalone => string.IsNullOrEmpty(BrokerLauncherSettings.Default.SessionAddress);
    }
}
