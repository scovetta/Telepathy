namespace TelepathyCommon
{
    /// <summary>
    /// Please only put the constants that need to be accessed by more than one services, setup or client here.
    /// Service specific constants should be moved into service local constants files.
    /// </summary>
    public static class HpcConstants
    {
        internal const int DefaultHttpsPort = 443;

        public const string HpcKeyName = @"SOFTWARE\Microsoft\HPC";

        public const string HpcFullKeyName = @"HKEY_LOCAL_MACHINE\" + HpcKeyName;

        public const string ClusterConnectionStringRegVal = @"ClusterConnectionString";

        // TODO: consider to get real capacity
        public static string NodeCapacity => "1";

        // TODO: investigate why set the value later
        public static string FirstCoreIndex => "3";

        /// <summary>
        /// Certificate for SSL, should be installed in local machine trust root store
        /// </summary>
        public const string SslThumbprint = "SSLThumbprint";

        /// <summary>
        /// Indicate data is stored in reliable registry
        /// </summary>
        public static string RegistrationStoreToken => "CCP_REGISTRATION_STORE";

        public const string SchedulerEnvironmentVariableName = "CCP_SCHEDULER";

        public const string ConnectionStringEnvironmentVariableName = "CCP_CONNECTIONSTRING";
    }
}