using System.Collections.Generic;
using System.Threading.Tasks;

namespace TelepathyCommon.HpcContext.Extensions
{
    public static class HpcContextExtension
    {
        public static EndpointsConnectionString GetConnectionString(this ITelepathyContext context)
        {
            return context.FabricContext.ConnectionString;
        }

        public static async Task<IEnumerable<string>> GetNodesAsync(this ITelepathyContext context)
        {
            return await context.FabricContext.GetNodesAsync(context.CancellationToken).ConfigureAwait(false);
        }
    }
}
