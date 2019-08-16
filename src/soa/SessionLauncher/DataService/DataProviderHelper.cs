//------------------------------------------------------------------------------
// <copyright file="DataProviderHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      DataProviderHelper
// </summary>
//------------------------------------------------------------------------------
#if HPCPACK

namespace Microsoft.Hpc.Scheduler.Session.Data.DataProvider
{
    using System.Diagnostics;
    using Microsoft.Hpc.Scheduler.Session.Data.Internal;
    using TraceHelper = Microsoft.Hpc.Scheduler.Session.Data.Internal.DataServiceTraceHelper;

    /// <summary>
    /// Data provider helper
    /// </summary>
    internal static class DataProviderHelper
    {
        /// <summary>
        /// Initialize the data server
        /// </summary>
        /// <param name="dsInfo">data server info</param>
        public static void InitializeDataServer(DataServerInfo dsInfo)
        {
            Debug.Assert(dsInfo != null, "dsInfo");

            try
            {
                FileShareDataProvider.InitializeDataServer(dsInfo);
            }
            catch (DataException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataClientInternal] .InitializeDataServer: receives exception {0}", e);
                e.DataServer = dsInfo.AddressInfo;
                throw;
            }
        }

        /// <summary>
        /// Get data provider according to data location and data server info
        /// </summary>
        /// <param name="location">data location</param>
        /// <param name="dsInfo">data server info</param>
        /// <returns>data provider that supports specified data location</returns>
        public static IDataProvider GetDataProvider(DataLocation location, DataServerInfo dsInfo)
        {
            if (location == DataLocation.FileShare)
            {
                return new FileShareDataProvider(dsInfo);
            }
            else 
            {
                // location = FileShareAndAzureBlob
                return new FileShareAndBlobDataProvider(dsInfo, location);
            }
        }
    }
}
#endif