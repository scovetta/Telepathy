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
        private Queue<string> queue = new Queue<string>();

        private BrokerLauncherCloudQueueClient client;

        private BrokerLauncherCloudQueueWatcher server;

        private LocalQueueListener<CloudQueueResponseDto> clientListener;

        private LocalQueueListener<CloudQueueCmdDto> serverListener;

        [TestInitialize]
        public void TestInit()
        {
            var serializer = new CloudQueueSerializer(CloudQueueCmdTypeBinder.BrokerLauncherBinder);
            this.clientListener = new LocalQueueListener<CloudQueueResponseDto>(queue, serializer);
            this.client = new BrokerLauncherCloudQueueClient(this.clientListener, new LocalQueueWriter<CloudQueueCmdDto>(queue, serializer));

            this.serverListener = new LocalQueueListener<CloudQueueCmdDto>(queue, serializer);
            this.server = new BrokerLauncherCloudQueueWatcher(new MockBrokerLauncher(), this.serverListener, new LocalQueueWriter<CloudQueueResponseDto>(queue, serializer));
        }

        [TestMethod]
        public async Task CreateE2ETest()
        {
            var tclient = this.client.CreateAsync(new SessionStartInfoContract(), 10);
            await this.serverListener.CheckAsync();
            await this.clientListener.CheckAsync();
            var res = await tclient;
            Assert.AreEqual(MockBrokerLauncher.Result.BrokerUniqueId, res.BrokerUniqueId);
        }

        [TestMethod]
        public async Task CreateDurableE2ETest()
        {
            var tclient = this.client.CreateDurableAsync(new SessionStartInfoContract(), 10);
            await this.serverListener.CheckAsync();
            await this.clientListener.CheckAsync();
            var res = await tclient;
            Assert.AreEqual(MockBrokerLauncher.Result.BrokerUniqueId, res.BrokerUniqueId);
        }

        [TestMethod]
        public async Task AttachE2ETest()
        {
            var tclient = this.client.AttachAsync(10);
            await this.serverListener.CheckAsync();
            await this.clientListener.CheckAsync();
            var res = await tclient;
            Assert.AreEqual(MockBrokerLauncher.Result.BrokerUniqueId, res.BrokerUniqueId);
        }

        [TestMethod]
        public async Task CloseE2ETest()
        {
            var tclient = this.client.CloseAsync(10);
            await this.serverListener.CheckAsync();
            await this.clientListener.CheckAsync();
            await tclient;
        }

        [TestMethod]
        public async Task PingBrokerE2ETest()
        {
            var tclient = this.client.PingBrokerAsync(10);
            await this.serverListener.CheckAsync();
            await this.clientListener.CheckAsync();
            var res = await tclient;
            Assert.AreEqual(true, res);
        }

        [TestMethod]
        public async Task PingBroker2E2ETest()
        {
            var tclient = this.client.PingBroker2Async(10);
            await this.serverListener.CheckAsync();
            await this.clientListener.CheckAsync();
            var res = await tclient;
            Assert.AreEqual("Yes", res);
        }

        [TestMethod]
        public async Task GetActiveBrokerIdListE2ETest()
        {
            var tclient = this.client.GetActiveBrokerIdListAsync();
            await this.serverListener.CheckAsync();
            await this.clientListener.CheckAsync();
            var res = await tclient;
            Assert.AreEqual(1, res.Length);
            Assert.AreEqual(1, res[0]);
        }
    }
}