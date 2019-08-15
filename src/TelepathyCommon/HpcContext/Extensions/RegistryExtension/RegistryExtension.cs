using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TelepathyCommon.Plugin;
using TelepathyCommon.Registry;

namespace TelepathyCommon.HpcContext.Extensions.RegistryExtension
{
    public static class RegistryExtension
    {
        #region RegistryExtensions

        private static string clusterNameCache;
        private static Guid clusterIdCache;
        private static string thumbPrintCache;
        
        public static async Task<Guid> GetClusterIdAsync(this ITelepathyContext context)
        {
            if (clusterIdCache == Guid.Empty)
            {
                clusterIdCache = await context.Registry.GetValueAsync<Guid>(HpcConstants.HpcFullKeyName, HpcConstants.ClusterIdRegVal, context.CancellationToken).ConfigureAwait(false);
            }

            return clusterIdCache;
        }

        public static async Task SetClusterIdAsync(this ITelepathyContext context, Guid clusterId)
        {
            await context.Registry.SetValueAsync<Guid>(HpcConstants.HpcFullKeyName, HpcConstants.ClusterIdRegVal, clusterId, context.CancellationToken).ConfigureAwait(false);
            clusterIdCache = clusterId;
        }

        public static async Task<string> GetClusterNameAsync(this ITelepathyContext context)
        {
            if (string.IsNullOrEmpty(clusterNameCache))
            {
                clusterNameCache = await context.Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.ClusterNameRegVal, context.CancellationToken).ConfigureAwait(false);
            }

            return clusterNameCache;
        }

        public static async Task SetClusterNameAsync(this ITelepathyContext context, string clusterName)
        {
            await context.Registry.SetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.ClusterNameRegVal, clusterName, context.CancellationToken).ConfigureAwait(false);
            clusterNameCache = clusterName;
        }

        public static async Task<string> GetDatabaseConnectionStringAsync(this ITelepathyContext context, string stringKey)
        {
            if (context.FabricContext.IsHpcService() && SqlConnectionStringProvider.Provider != null)
            {
                // server reliable registry, alter the database string logic.
                try
                {
                    Trace.TraceInformation("Calling SqlConnectionStringProvider.GetConnectionStringAsync with key = {0}", stringKey);
                    var connectionString = await SqlConnectionStringProvider.Provider.GetConnectionStringAsync(
                        stringKey,
                        context.CancellationToken).ConfigureAwait(false);

                    if (null == connectionString)
                    {
                        throw new InvalidDataException(
                            $"Unable to load connection string {stringKey}.");
                    }

                    return connectionString;
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning(
                        "Exception happened when loading connection string for {0}, ex {1}.",
                        stringKey,
                        ex);
                    throw;
                }
            }
            else
            {
                return await context.Registry.GetValueAsync<string>(
                    HpcConstants.HpcSecurityRegKey,
                    stringKey,
                    context.CancellationToken,
                    null).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets the scheduler connection string
        /// </summary>
        public static async Task<string> GetSchedulerDatabaseConnectionStringAsync(this ITelepathyContext context)
        {
            return await context.GetDatabaseConnectionStringAsync(HpcConstants.SchedulerDbStringRegVal).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the monitoring connection string
        /// </summary>
        public static async Task<string> GetMonitoringDatabaseConnectionStringAsync(this ITelepathyContext context)
        {
            return await context.GetDatabaseConnectionStringAsync(HpcConstants.MonitoringDbStringRegVal).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the reporting connection string
        /// </summary>
        public static async Task<string> GetReportingDatabaseConnectionStringAsync(this ITelepathyContext context)
        {
            return await context.GetDatabaseConnectionStringAsync(HpcConstants.ReportingDbStringRegVal).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the diagnostics connection string
        /// </summary>
        public static async Task<string> GetDiagnosticsDatabaseConnectionStringAsync(this ITelepathyContext context)
        {
            return await context.GetDatabaseConnectionStringAsync(HpcConstants.DiagnosticsDbStringRegVal).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the management connection string
        /// </summary>
        public static async Task<string> GetManagementDatabaseConnectionStringAsync(this ITelepathyContext context)
        {
            return await context.GetDatabaseConnectionStringAsync(HpcConstants.ManagementDbStringRegVal).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the is linux https setting
        /// </summary>
        public static async Task<int> GetIsLinuxHttpsAsync(this ITelepathyContext context)
        {
            return await context.Registry.GetValueAsync<int>(HpcConstants.HpcFullKeyName, HpcConstants.LinuxHttpsRegVal, context.CancellationToken, 0).ConfigureAwait(false);
        }

        public static async Task<string> GetAzureStorageConnectionStringAsync(this ITelepathyContext context)
        {
            return await context.Registry.GetValueAsync<string>(HpcConstants.HpcSecurityRegKey, HpcConstants.AzureStorageConnectionString, context.CancellationToken, null).ConfigureAwait(false);
        }

        public static async Task<string> GetSSLThumbprint(this ITelepathyContext context)
        {
            return await context.Registry.GetSSLThumbprint(context.CancellationToken).ConfigureAwait(false);
        }

        public static async Task<string> GetSSLThumbprint(this IRegistry registry, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(thumbPrintCache))
            {
                thumbPrintCache = await registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.SslThumbprint, cancellationToken, null).ConfigureAwait(false);
            }

            return thumbPrintCache;
        }

        public static async Task<CertificateValidationType> GetCertificateValidationTypeAsync(this NonHARegistry registry)
        {
            return (CertificateValidationType)await registry.GetValueAsync<int>(
                                                      HpcConstants.HpcFullKeyName,
                                                      HpcConstants.CertificateValidationType,
                                                      CancellationToken.None,
                                                      0) // Default value is CertificateValidationType.None
                                                  .ConfigureAwait(false);
        }

        public static async Task<bool> CheckIfNonDomain(this IRegistry registry)
        {
            return await registry.GetValueAsync(HpcConstants.HpcFullKeyName, HpcConstants.NonDomainRole, CancellationToken.None, 0) > 0;
        }

        #region network share
        public static async Task<string> GetClusterRuntimeDataShareAsync(this ITelepathyContext context)
        {
            return await context.Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.RuntimeDataSharePropertyName, context.CancellationToken, null).ConfigureAwait(false);
        }

        public static async Task<string> GetClusterSpoolDirShareAsync(this ITelepathyContext context)
        {
            return await context.Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.SpoolDirSharePropertyName, context.CancellationToken, null).ConfigureAwait(false);
        }

        public static async Task<string> GetClusterServiceRegistrationShareAsync(this ITelepathyContext context)
        {
            return await context.Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.ServiceRegistrationSharePropertyName, context.CancellationToken, null).ConfigureAwait(false);
        }

        public static async Task<string> GetClusterInstallShareAsync(this ITelepathyContext context)
        {
            return await context.Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.InstallSharePropertyName, context.CancellationToken, null).ConfigureAwait(false);
        }

        public static async Task<string> GetClusterDiagnosticsShareAsync(this ITelepathyContext context)
        {
            return await context.Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.DiagnosticsSharePropertyName, context.CancellationToken, null).ConfigureAwait(false);
        }
        #endregion
        #endregion
    }
}
