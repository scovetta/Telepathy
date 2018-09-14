//-------------------------------------------------------------------------------------------------
// <copyright file="SSLCertBindingHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
//
// <summary>
//     A helper class to set/update/remove the http sslcert binding.
// </summary>
//-------------------------------------------------------------------------------------------------
namespace Microsoft.Hpc
{
    using System.Management.Automation;
    using System.Threading.Tasks;

    public static class SslCertBindingHelper
    {
        public static string PublicSslPort => HpcConstants.DefaultHttpsPort.ToString();
        public static string SslCertMgmtIpPort => "0.0.0.0:" + PublicSslPort;
        public static string SslCertMgmtAppId => "2b519f34-e5b6-4de8-8c38-324634dd76e8";
        public static string SslCertInternalCommunicationAppId => "E3B278C4-502C-4038-801D-E20F853FC906";

        private const string SslCertPshScript = @"
param($ipport, $certHash, $appId)
$orgCertHash = netsh http show sslcert ipport=$ipport | ?{$_ -match '^\s*Certificate\s+Hash\s*:\s*[0-9a-f]+\s*$'} | %{$_ -split ':'} | %{$_.Trim()} | select -Last(1);
if ($orgCertHash -ne $certHash)
{
    if($orgCertHash)
    {
        netsh http del sslcert ipport=$ipport | Out-Null;
    }
    if($certHash)
    {
        $appId = ([guid]$appId).ToString('B');
        netsh http add sslcert ipport=$ipport certhash=$certHash appid=$appId | Out-Null;
    }
}";

        /// <summary>
        /// Add or update the sslcert binding for the specified ipport
        /// </summary>
        /// <param name="ipport">IP address and port for the binding</param>
        /// <param name="appId">GUID to identify the owning application</param>
        /// <param name="thumbprint">The thumbprint of the certificate</param>
        /// <returns></returns>
        public static async Task SetIpPortSslCertBindingAsync(string ipport, string appId, string thumbprint)
        {
            using (PowerShell pshInstance = PowerShell.Create())
            {
                pshInstance.AddScript(SslCertPshScript).AddParameter("ipport", ipport).AddParameter("certHash", thumbprint).AddParameter("appId", appId);
                await Task.Factory.FromAsync(pshInstance.BeginInvoke(), r => pshInstance.EndInvoke(r)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Remove the sslcert binding for the specified ipport
        /// </summary>
        /// <param name="ipport">IP address and port for the binding</param>
        /// <returns></returns>
        public static async Task RemoveIpPortSslCertBindingAsync(string ipport)
        {
            await SetIpPortSslCertBindingAsync(ipport, null, null).ConfigureAwait(false);
        }
    }
}
