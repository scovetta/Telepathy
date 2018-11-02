using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CcpWSLB.UnitTest
{
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Client;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using CcpWSLB.UnitTest.Mock;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.DTO;
    using Microsoft.Hpc.ServiceBroker.FrontEnd.AzureQueue;

    [TestClass]
    public class BrokerWorkerControllerCloudAdapterUnitTest
    {
        private Queue<string> queue = new Queue<string>();

        private BrokerControllerCloudQueueClient client;

        private BrokerWorkerControllerQueueWatcher server;

        private LocalQueueListener<CloudQueueResponseDto> clientListener;

        private LocalQueueListener<CloudQueueCmdDto> serverListener;


        [TestInitialize]
        public void TestInit()
        {
            var serializer = new CloudQueueSerializer();
            this.clientListener = new LocalQueueListener<CloudQueueResponseDto>(queue, serializer);
            this.client = new BrokerControllerCloudQueueClient(this.clientListener, new LocalQueueWriter<CloudQueueCmdDto>(queue, serializer));

            this.serverListener = new LocalQueueListener<CloudQueueCmdDto>(queue, serializer);
            this.server = new BrokerWorkerControllerQueueWatcher(new MockController(), this.serverListener, new LocalQueueWriter<CloudQueueResponseDto>(queue, serializer));
        }

        [TestMethod]
        public async Task EndRequestsE2ETest()
        {
            var tclient = this.client.EndRequestsAsync(1, string.Empty, 2, 3, 4);
            await this.serverListener.CheckAsync();
            await this.clientListener.CheckAsync();
            await tclient;
        }

        [TestMethod]
        public async Task FlushE2ETest()
        {
            var tclient = this.client.FlushAsync(1, string.Empty, 2, 3, 4);
            await this.serverListener.CheckAsync();
            await this.clientListener.CheckAsync();
            await tclient;
        }

        [TestMethod]
        public async Task PurgeE2ETest()
        {
            var tclient = this.client.PurgeAsync(string.Empty);
            await this.serverListener.CheckAsync();
            await this.clientListener.CheckAsync();
            await tclient;
        }

        [TestMethod]
        public async Task GetBrokerClientStatusE2ETest()
        {
            var tclient = this.client.GetBrokerClientStatusAsync(string.Empty);
            await this.serverListener.CheckAsync();
            await this.clientListener.CheckAsync();
            var res = await tclient;
            Assert.AreEqual(BrokerClientStatus.Processing, res);
        }

        [TestMethod]
        public async Task GetRequestsCountE2ETest()
        {
            var tclient = this.client.GetRequestsCountAsync(string.Empty);
            await this.serverListener.CheckAsync();
            await this.clientListener.CheckAsync();
            var res = await tclient;
            Assert.AreEqual(1, res);
        }

        [TestMethod]
        public async Task PullResponsesE2ETest()
        {
            var tclient = this.client.PullResponsesAsync(string.Empty, GetResponsePosition.Current, 1, string.Empty);
            await this.serverListener.CheckAsync();
            await this.clientListener.CheckAsync();
            var res = await tclient;
            Assert.AreEqual(true, res.EOM);
        }

        [TestMethod]
        public async Task GetResponsesAQE2ETest()
        {
            var tclient = this.client.GetResponsesAQAsync(string.Empty,string.Empty,  GetResponsePosition.Current, 1, string.Empty, 1);
            await this.serverListener.CheckAsync();
            await this.clientListener.CheckAsync();
            var res = await tclient;
            Assert.AreEqual("queue", res.azureResponseQueueUri);
            Assert.AreEqual("blob", res.azureResponseBlobUr);
        }

        [TestMethod]
        public async Task PingE2ETest()
        {
            var tclient = this.client.PingAsync();
            await this.serverListener.CheckAsync();
            await this.clientListener.CheckAsync();
            await tclient;
        }
    }
}
