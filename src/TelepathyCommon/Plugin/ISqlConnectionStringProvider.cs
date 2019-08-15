// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISqlConnectionStringProvider.cs" company="Microsoft">
//   2017
// </copyright>
// <summary>
//   Defines the IConnectionString type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace TelepathyCommon.Plugin
{
    /// <summary>
    /// The ConnectionString interface.
    /// </summary>
    public interface ISqlConnectionStringProvider
    {
        /// <summary>
        /// The get connection string async.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<string> GetConnectionStringAsync(string key, CancellationToken token);
    }
}
