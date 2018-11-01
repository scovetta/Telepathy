namespace SessionAPI.UnitTest
{
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.DTO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class QueueAdapterTest
    {
        const string createBrokerCmdStr = @"{""$type"":""BrokerLauncherCloudQueueCmdDto"",""RequestId"":""89fba2c1-3529-4b38-a332-22920e82616d"",""CmdName"":""Create"",""Parameters"":{""$type"":""Object[]"",""$values"":[{""$type"":""SessionStartInfoContract"",""IsNoSession"":true,""AllocationGrowLoadRatioThreshold"":null,""AllocationShrinkLoadRatioThreshold"":null,""ClientIdleTimeout"":null,""ClientConnectionTimeout"":null,""SessionIdleTimeout"":null,""MessagesThrottleStartThreshold"":null,""MessagesThrottleStopThreshold"":null,""ServiceName"":""CcpEchoSvc"",""TransportScheme"":1,""Secure"":false,""CanPreempt"":true,""ShareSession"":false,""JobTemplate"":null,""ResourceUnitType"":0,""MaxUnits"":null,""MinUnits"":null,""Username"":null,""Password"":null,""SavePassword"":null,""Certificate"":null,""PfxPassword"":null,""ServiceJobName"":null,""ServiceJobProject"":null,""NodeGroupsStr"":"""",""RequestedNodesStr"":"""",""Priority"":null,""ExtendedPriority"":2000,""Runtime"":-1,""Environments"":null,""DiagnosticBrokerNode"":null,""AdminJobForHostInDiag"":false,""ServiceVersion"":null,""ClientBrokerHeartbeatInterval"":null,""ClientBrokerHeartbeatRetryCount"":null,""MaxMessageSize"":null,""ServiceOperationTimeout"":null,""UseInprocessBroker"":false,""UseSessionPool"":false,""AutoDisposeBrokerClient"":null,""UseAzureQueue"":true,""UseWindowsClientCredential"":false,""DependFiles"":null,""ClientVersion"":{""$type"":""Version"",""Major"":0,""Minor"":0,""Build"":0,""Revision"":0,""MajorRevision"":0,""MinorRevision"":0},""DispatcherCapacityInGrowShrink"":null,""LocalUser"":false,""ParentJobIds"":null,""ServiceHostIdleTimeout"":null,""ServiceHangTimeout"":null,""UseAad"":false,""DependFilesStorageInfo"":{""$type"":""Dictionary`2""},""RegPath"":""c:\\services\\registration"",""IpAddress"":null},-1]},""Version"":1}";

        private const string createBrokerResStr =
            @"{""$type"":""BrokerLauncherCloudQueueResponseDto"",""RequestId"":""f2d66098-cd56-45ef-9118-30f798f34620"",""CmdName"":""Create"",""Response"":{""$type"":""BrokerInitializationResult"",""BrokerEpr"":{""$type"":""String[]"",""$values"":[""net.tcp://zihao:9091/-1/NetTcp"",null,null,null,null,null]},""ControllerEpr"":{""$type"":""String[]"",""$values"":[""net.tcp://zihao:9091/-1/NetTcp/Controller"",null,null,null,null,null]},""ResponseEpr"":{""$type"":""String[]"",""$values"":[""net.tcp://zihao:9091/-1/NetTcp/GetResponse"",null,null,null,null,null]},""ServiceOperationTimeout"":86400000,""MaxMessageSize"":655360,""ClientBrokerHeartbeatInterval"":20000,""ClientBrokerHeartbeatRetryCount"":3,""AzureRequestQueueUri"":"""",""AzureRequestBlobUri"":"""",""UseAzureQueue"":true,""BrokerUniqueId"":""82ac6e0a-4062-4e63-a5e9-64fa7378ee54"",""SupportsMessageDetails"":true}}";

        [TestMethod]
        public void BasicRequestSerializeE2E()
        {
            var serializer = new CloudQueueSerializer(BrokerLauncherCloudQueueCmdTypeBinder.Default);
            var cmd = new BrokerLauncherCloudQueueCmdDto("TestId", "TestCmd", new SessionStartInfoContract(), 10);
            var str = serializer.Serialize(cmd);
            var dcmd = serializer.Deserialize<BrokerLauncherCloudQueueCmdDto>(str);
            Assert.AreEqual(cmd.RequestId, dcmd.RequestId);
            Assert.AreEqual(cmd.CmdName, dcmd.CmdName);
            Assert.AreEqual(cmd.Version, dcmd.Version);
            Assert.AreEqual(cmd.Parameters.Length, dcmd.Parameters.Length);

            Assert.AreEqual(cmd.Parameters[0].GetType(), dcmd.Parameters[0].GetType());
            Assert.AreEqual(typeof(long), dcmd.Parameters[1].GetType());
        }

        [TestMethod]
        public void BasicResultSerializeE2E()
        {
            var serializer = new CloudQueueSerializer(BrokerLauncherCloudQueueCmdTypeBinder.Default);
            var res = new BrokerLauncherCloudQueueResponseDto("TestId", "TestCmd", new BrokerInitializationResult());
            var str = serializer.Serialize(res);
            var dres = serializer.Deserialize<BrokerLauncherCloudQueueResponseDto>(str);
            Assert.AreEqual(res.RequestId, dres.RequestId);
            Assert.AreEqual(res.CmdName, dres.CmdName);
            Assert.AreEqual(res.Response.GetType(), dres.Response.GetType());
        }

        [TestMethod]
        public void CreateBrokerCmdDeserializeTest()
        {
            var serializer = new CloudQueueSerializer(BrokerLauncherCloudQueueCmdTypeBinder.Default);
            var dcmd = serializer.Deserialize<BrokerLauncherCloudQueueCmdDto>(createBrokerCmdStr);
            Assert.AreEqual("89fba2c1-3529-4b38-a332-22920e82616d", dcmd.RequestId);
        }

        [TestMethod]
        public void CreateBrokerResDeserializeTest()
        {
            var serializer = new CloudQueueSerializer(BrokerLauncherCloudQueueCmdTypeBinder.Default);
            var dcmd = serializer.Deserialize<BrokerLauncherCloudQueueResponseDto>(createBrokerResStr);
            Assert.AreEqual("f2d66098-cd56-45ef-9118-30f798f34620", dcmd.RequestId);
        }
    }
}