namespace BrokerLauncher.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using BrokerLauncher.UnitTest.Mock;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher.QueueAdapter;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Client;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.DTO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BrokerLauncherCloudQueueAdapterUnitTest
    {
        [TestMethod]
        public async Task BasicE2ETest()
        {
            Queue<string> queue = new Queue<string>();
            var serializer = new CloudQueueSerializer(BrokerLauncherCloudQueueCmdTypeBinder.Default);
            var clientListener = new LocalQueueListener<BrokerLauncherCloudQueueResponseDto>(queue, serializer);
            var client = new BrokerLauncherCloudQueueClient(
                clientListener,
                new LocalQueueWriter<BrokerLauncherCloudQueueCmdDto>(queue, serializer));

            var serverListener = new LocalQueueListener<BrokerLauncherCloudQueueCmdDto>(queue, serializer);
            var server = new BrokerLauncherCloudQueueWatcher(
                new MockBrokerLauncher(),
                serverListener,
                new LocalQueueWriter<BrokerLauncherCloudQueueResponseDto>(queue, serializer));

            var tclient = client.CreateAsync(new SessionStartInfoContract(), 10);
            await serverListener.CheckAsync();
            await clientListener.CheckAsync();
            var res = await tclient;
            Assert.AreEqual(MockBrokerLauncher.Result.BrokerUniqueId, res.BrokerUniqueId);
        }
    }
}