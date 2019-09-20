// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace SessionAPI.UnitTest
{
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Interface;
    using Microsoft.Telepathy.Session.QueueAdapter;
    using Microsoft.Telepathy.Session.QueueAdapter.DTO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class QueueAdapterTest
    {
        const string createBrokerCmdStr = @"{""$type"":""CloudQueueCmdDto"",""RequestId"":""89fba2c1-3529-4b38-a332-22920e82616d"",""CmdName"":""Create"",""Parameters"":{""$type"":""Object[]"",""$values"":[{""$type"":""SessionStartInfoContract"",""IsNoSession"":true,""AllocationGrowLoadRatioThreshold"":null,""AllocationShrinkLoadRatioThreshold"":null,""ClientIdleTimeout"":null,""ClientConnectionTimeout"":null,""SessionIdleTimeout"":null,""MessagesThrottleStartThreshold"":null,""MessagesThrottleStopThreshold"":null,""ServiceName"":""CcpEchoSvc"",""TransportScheme"":1,""Secure"":false,""CanPreempt"":true,""ShareSession"":false,""JobTemplate"":null,""ResourceUnitType"":0,""MaxUnits"":null,""MinUnits"":null,""Username"":null,""Password"":null,""SavePassword"":null,""Certificate"":null,""PfxPassword"":null,""ServiceJobName"":null,""ServiceJobProject"":null,""NodeGroupsStr"":"""",""RequestedNodesStr"":"""",""Priority"":null,""ExtendedPriority"":2000,""Runtime"":-1,""Environments"":null,""DiagnosticBrokerNode"":null,""AdminJobForHostInDiag"":false,""ServiceVersion"":null,""ClientBrokerHeartbeatInterval"":null,""ClientBrokerHeartbeatRetryCount"":null,""MaxMessageSize"":null,""ServiceOperationTimeout"":null,""UseInprocessBroker"":false,""UseSessionPool"":false,""AutoDisposeBrokerClient"":null,""UseAzureQueue"":true,""UseWindowsClientCredential"":false,""DependFiles"":null,""ClientVersion"":{""$type"":""Version"",""Major"":0,""Minor"":0,""Build"":0,""Revision"":0,""MajorRevision"":0,""MinorRevision"":0},""DispatcherCapacityInGrowShrink"":null,""LocalUser"":false,""ParentJobIds"":null,""ServiceHostIdleTimeout"":null,""ServiceHangTimeout"":null,""UseAad"":false,""DependFilesStorageInfo"":{""$type"":""Dictionary`2""},""RegPath"":""c:\\services\\registration"",""IpAddress"":null},-1]},""Version"":1}";

        private const string createBrokerResStr =
            @"{""$type"":""CloudQueueResponseDto"",""RequestId"":""f2d66098-cd56-45ef-9118-30f798f34620"",""CmdName"":""Create"",""Response"":{""$type"":""BrokerInitializationResult"",""BrokerEpr"":{""$type"":""String[]"",""$values"":[""net.tcp://zihao:9091/-1/NetTcp"",null,null,null,null,null]},""ControllerEpr"":{""$type"":""String[]"",""$values"":[""net.tcp://zihao:9091/-1/NetTcp/Controller"",null,null,null,null,null]},""ResponseEpr"":{""$type"":""String[]"",""$values"":[""net.tcp://zihao:9091/-1/NetTcp/GetResponse"",null,null,null,null,null]},""ServiceOperationTimeout"":86400000,""MaxMessageSize"":655360,""ClientBrokerHeartbeatInterval"":20000,""ClientBrokerHeartbeatRetryCount"":3,""AzureRequestQueueUri"":"""",""AzureRequestBlobUri"":"""",""UseAzureQueue"":true,""BrokerUniqueId"":""82ac6e0a-4062-4e63-a5e9-64fa7378ee54"",""SupportsMessageDetails"":true}}";

        private const string endrequestsCmdStr =
            @"{""$type"":""CloudQueueCmdDto"",""RequestId"":""b0d08e3e-dbc3-41f6-8c28-bec49145a396"",""CmdName"":""EndRequests"",""Parameters"":{""$type"":""Object[]"",""$values"":[10,""4882731e-d40b-4938-840f-28a0e1a3e9d3"",0,3600000,3600000]},""Version"":1}";

        private const string getResponseAQResStr =
            @"{""$type"":""CloudQueueResponseDto"",""RequestId"":""722ae6d9-0072-4831-9031-5d0ccb414616"",""CmdName"":""GetResponsesAQ"",""Response"":{""$type"":""ValueTuple`2"",""Item1"":""https://soaservicestorage.queue.core.windows.net/hpcsoa-234000264-0-response-4125719597?sv=2017-04-17&sig=mGnbXbdccsGyQJWu%2BbkodUGP4tqG4LxSNbM5AW4gKeA%3D&se=2018-11-09T08%3A56%3A03Z&sp=p"",""Item2"":""https://soaservicestorage.blob.core.windows.net/hpcsoa-234000264-0-response-4125719597?sv=2017-04-17&sr=c&sig=lwKvDqTigzctKMfDSuCSEMpFqUv1%2FzuUP3QbnHJzOpo%3D&se=2018-11-09T08%3A56%3A03Z&sp=rd""}}";

        [TestMethod]
        public void BasicRequestSerializeE2E()
        {
            var serializer = new CloudQueueSerializer(CloudQueueCmdTypeBinder.BrokerLauncherBinder);
            var cmd = new CloudQueueCmdDto("TestId", "TestCmd", new SessionStartInfoContract(), 10);
            var str = serializer.Serialize(cmd);
            var dcmd = serializer.Deserialize<CloudQueueCmdDto>(str);
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
            var serializer = new CloudQueueSerializer(CloudQueueCmdTypeBinder.BrokerLauncherBinder);
            var res = new CloudQueueResponseDto("TestId", "TestCmd", new BrokerInitializationResult());
            var str = serializer.Serialize(res);
            var dres = serializer.Deserialize<CloudQueueResponseDto>(str);
            Assert.AreEqual(res.RequestId, dres.RequestId);
            Assert.AreEqual(res.CmdName, dres.CmdName);
            Assert.AreEqual(res.Response.GetType(), dres.Response.GetType());
        }

        [TestMethod]
        public void CreateBrokerCmdDeserializeTest()
        {
            var serializer = new CloudQueueSerializer(CloudQueueCmdTypeBinder.BrokerLauncherBinder);
            var dcmd = serializer.Deserialize<CloudQueueCmdDto>(createBrokerCmdStr);
            Assert.AreEqual("89fba2c1-3529-4b38-a332-22920e82616d", dcmd.RequestId);
        }

        [TestMethod]
        public void CreateBrokerResDeserializeTest()
        {
            var serializer = new CloudQueueSerializer(CloudQueueCmdTypeBinder.BrokerLauncherBinder);
            var dcmd = serializer.Deserialize<CloudQueueResponseDto>(createBrokerResStr);
            Assert.AreEqual("f2d66098-cd56-45ef-9118-30f798f34620", dcmd.RequestId);
        }

        [TestMethod]
        public void EndRequestCmdDeserializeTest()
        {
            var serializer = new CloudQueueSerializer(CloudQueueCmdTypeBinder.BrokerLauncherBinder);
            var dcmd = serializer.Deserialize<CloudQueueCmdDto>(endrequestsCmdStr);
            Assert.AreEqual("b0d08e3e-dbc3-41f6-8c28-bec49145a396", dcmd.RequestId);
        }

        [TestMethod]
        public void GetResponsesAQResDeserializeTest()
        {
            var serializer = new CloudQueueSerializer(CloudQueueCmdTypeBinder.BrokerLauncherBinder);
            var dcmd = serializer.Deserialize<CloudQueueResponseDto>(getResponseAQResStr);
            Assert.AreEqual("722ae6d9-0072-4831-9031-5d0ccb414616", dcmd.RequestId);
        }
    }
}