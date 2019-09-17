namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.AzureBatch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal static class AzureBatchSessionIdGenerator
    {
        public static string GenerateSessionId()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        }
    }
}
