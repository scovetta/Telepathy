namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.DataService.REST
{
#if HPCPACK
    using Microsoft.Hpc.Scheduler.Session.Data.Internal;

    using TelepathyCommon;

    internal class DataServiceRestServer : InternalRestServer
    {
        public static DataService DataServiceInstance { get; private set; }

        public DataServiceRestServer(DataService serviceInstance) : base(HpcConstants.HpcDataServiceAppRoot, HpcConstants.HpcDataServicePort)
        {
            DataServiceInstance = serviceInstance;
        }
    }
#endif
}
