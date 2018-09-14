using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Hpc
{
    public static class HpcContextExtension
    {
        public static EndpointsConnectionString GetConnectionString(this IHpcContext context)
        {
            return context.FabricContext.ConnectionString;
        }

        public static async Task<IEnumerable<string>> GetNodesAsync(this IHpcContext context)
        {
            return await context.FabricContext.GetNodesAsync(context.CancellationToken).ConfigureAwait(false);
        }
    }
}
