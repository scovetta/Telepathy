//------------------------------------------------------------------------------
// <copyright file="DispatcherManagerTest.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Unit Test for DispatcherManager
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.SvcBroker.UnitTest.BackEnd
{
    using System;
    using System.Collections.Generic;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using Microsoft.Hpc.ServiceBroker;
    using Microsoft.Hpc.ServiceBroker.BackEnd;
    using Microsoft.Hpc.ServiceBroker.BrokerStorage;
    using Microsoft.Hpc.ServiceBroker.FrontEnd;
    using Microsoft.Hpc.SvcBroker.UnitTest.Mock;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit test for DispatcherManager
    /// </summary>
    [TestClass]
    public class DispatcherManagerTest
    {
        /// <summary>
        /// Store the service host epr format
        /// </summary>
        private static readonly string serviceHostEprFormat = "net.tcp://{0}:{1}/{2}/{3}/_defaultEndpoint";

        /// <summary>
        /// Store the service host port
        /// </summary>
        private static readonly int serviceHostPort = 9088;

        /// <summary>
        /// Unit test to create a new dispatcher
        /// </summary>
        [TestMethod]
        public void NewDispatcherTest()
        {
            int jobId = 1;
            int taskId = 1;
            int niceId = 1;
            string machineName = Environment.MachineName;
            EndpointAddress epr = this.GetEndpointAddress(jobId, taskId, machineName);
            MockBrokerQueue queue = new MockBrokerQueue();

            ServiceHost host = StartMockServiceHost(epr);

            Global.SetBrokerQueue(new MockBrokerQueue());
            DispatcherManager manager = new DispatcherManager(1, String.Empty);
            manager.NewDispatcher(jobId, taskId, niceId, machineName);

            Message request = Message.CreateMessage(MessageVersion.Default, "NewDispatcherTest", "Test");
            request.Headers.MessageId = new UniqueId();
            queue.TriggerGetRequestCallback(new BrokerQueueItem(new DummyRequestContext(MessageVersion.Default), request, null));

            Thread.Sleep(500);
            int tryCount = 0;
            while (tryCount < 3)
            {
                if (queue.ReplyMessageQueue.Count > 0)
                {
                    Message reply = queue.ReplyMessageQueue.Dequeue();
                    string replyMessage = reply.GetBody<string>();
                    Assert.AreEqual("TestReply", replyMessage);
                    break;
                }

                Thread.Sleep(3000);
                tryCount++;
            }

            if (tryCount >= 3)
            {
                Assert.Fail("Timeout");
            }

            host.Close();
        }

        /// <summary>
        /// Starts a mock service host
        /// </summary>
        /// <param name="epr">indicate the epr</param>
        /// <returns>the service host</returns>
        private ServiceHost StartMockServiceHost(EndpointAddress epr)
        {
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.Transport);
            binding.Security.Transport.ProtectionLevel = ProtectionLevel.None;
            ServiceHost host = new ServiceHost(typeof(MockServiceHost), epr.Uri);
            host.AddServiceEndpoint(typeof(IMockServiceContract), binding, String.Empty);
            host.Open();
            return host;
        }

        /// <summary>
        /// Gets the enpoint address for a service host
        /// </summary>
        /// <param name="jobId">indicate the job id</param>
        /// <param name="taskId">indicate the task id</param>
        /// <param name="machineName">indicate the machine name</param>
        /// <returns>endpoint address</returns>
        private EndpointAddress GetEndpointAddress(int jobId, int taskId, string machineName)
        {
            return new EndpointAddress(String.Format(serviceHostEprFormat, machineName, serviceHostPort, jobId, taskId));
        }
    }
}
