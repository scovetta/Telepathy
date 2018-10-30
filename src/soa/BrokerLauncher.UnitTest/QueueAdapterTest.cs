namespace BrokerLauncher.UnitTest
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher.QueueAdapter;
    using Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher.QueueAdapter.DTO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Newtonsoft.Json;

    [TestClass]
    public class QueueAdapterTest
    {
        [TestMethod]
        public void BasicRequestSerializeE2E()
        {
            var serializer = new BrokerLauncherCloudQueueSerializer(BrokerLauncherCloudQueueWatcher.TypeBinder, new List<JsonConverter>() { new Int64ToInt32Converter() });
            var cmd = new BrokerLauncherCloudQueueCmdDto("TestId", "TestCmd", new SessionStartInfoContract(), 10);
            var str = serializer.Serialize(cmd);
            var dcmd = serializer.Deserialize<BrokerLauncherCloudQueueCmdDto>(str);
            Assert.AreEqual(cmd.RequestId, dcmd.RequestId);
            Assert.AreEqual(cmd.CmdName, dcmd.CmdName);
            Assert.AreEqual(cmd.Version, dcmd.Version);
            Assert.AreEqual(cmd.Parameters.Length, dcmd.Parameters.Length);
            for (int i = 0; i != dcmd.Parameters.Length; ++i)
            {
                Assert.AreEqual(cmd.Parameters[i].GetType(), dcmd.Parameters[i].GetType());
            }
        }

        [TestMethod]
        public void BasicResultSerializeE2E()
        {
            var serializer = new BrokerLauncherCloudQueueSerializer(BrokerLauncherCloudQueueWatcher.TypeBinder, new List<JsonConverter>() {});
            var res = new BrokerLauncherCloudQueueResponseDto("TestId", "TestCmd", new BrokerInitializationResult());
            var str = serializer.Serialize(res);
            var dres = serializer.Deserialize<BrokerLauncherCloudQueueResponseDto>(str);
            Assert.AreEqual(res.RequestId, dres.RequestId);
            Assert.AreEqual(res.CmdName, dres.CmdName);
            Assert.AreEqual(res.Response.GetType(), dres.Response.GetType());
            
        }
    }
}