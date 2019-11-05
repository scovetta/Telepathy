// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Common
{
    /// <summary>
    ///     Please only put the constants that need to be accessed by more than one services, setup or client here.
    ///     Service specific constants should be moved into service local constants files.
    /// </summary>
    public static class TelepathyConstants
    {
        public const string ClusterConnectionStringRegVal = @"ClusterConnectionString";

        public const string ConnectionStringEnvironmentVariableName = "CCP_CONNECTIONSTRING";

        public const string HpcFullKeyName = @"HKEY_LOCAL_MACHINE\" + HpcKeyName;

        public const string HpcKeyName = @"SOFTWARE\Microsoft\HPC";

        public const string SchedulerEnvironmentVariableName = "CCP_SCHEDULER";

        /// <summary>
        ///     Certificate for SSL, should be installed in local machine trust root store
        /// </summary>
        public const string SslThumbprint = "SSLThumbprint";

        internal const int DefaultHttpsPort = 443;

        public static string AzureTableBindingSchemePrefix => @"az.table://";

        // TODO: investigate why set the value later
        public static string FirstCoreIndex => "3";

        // TODO: consider to get real capacity
        public static string NodeCapacity => "1";

        /// <summary>
        ///     Indicate data is stored in reliable registry
        /// </summary>
        public static string RegistrationStoreToken => "CCP_REGISTRATION_STORE";

        public static string SessionLauncherAzureTableBindingAddress => $"{AzureTableBindingSchemePrefix}SessionLauncher";

        public static string SessionSchedulerDelegationAzureTableBindingAddress => $"{AzureTableBindingSchemePrefix}SchedulerDelegation";

        public static string StandaloneSessionId => "0";

        public static string ServiceWorkingDirEnvVar => "TELEPATHY_SERVICE_WORKING_DIR";
    }
}