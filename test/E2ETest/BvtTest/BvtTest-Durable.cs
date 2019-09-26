// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Test.E2E.Bvt
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.ComputeCluster.Test.AppIntegration.EchoService.MessageContract;
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Internal;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BvtTestDurable
    {
        private static string Server;

        private static string EchoSvcName = "CcpEchoSvc";

        private static string HNEnvName = "HNMachine";

        private static string DefaultServer = "localhost";

        private static bool InProc = false;

        private static int NumberOfCalls = 500;

        private static string NetTcpEndpointPattern = "net.tcp://{0}:9091/{1}/NetTcp";

        private static void Info(string msg, params object[] args)
        {
            Trace.TraceInformation(msg, args);
        }

        private static void TraceEvent(string msg, params object[] args)
        {
            Trace.TraceInformation(msg, args);
        }

        private static void Error(string msg, params object[] args)
        {
            Trace.TraceError(msg, args);
        }

        public static void Assert(bool condition, string msg, params object[] obj)
        {
            if (!condition)
            {
                Error(msg, obj);
            }
        }

        private static SessionStartInfo BuildSessionStartInfo(
            string server,
            string serviceName,
            string BindingScheme,
            string TraceDir,
            string username,
            string password,
            SessionUnitType unitType,
            int? minUnit,
            int? maxUnit,
            Version serviceVersion)
        {
            SessionStartInfo startInfo;
            if (serviceVersion == null)
            {
                startInfo = new SessionStartInfo(server, serviceName);
            }
            else
            {
                startInfo = new SessionStartInfo(server, serviceName, serviceVersion);
            }

            if (BindingScheme != null)
            {
                // If BindingScheme is assigned, using the scheme
                switch (BindingScheme.ToLower())
                {
                    case "nettcp":
                    case "http":
                    case "webapi":
                        startInfo.TransportScheme = (TransportScheme)Enum.Parse(typeof(TransportScheme), BindingScheme, true);
                        break;
                    default: break;
                }
            }

            if (InProc)
            {
                startInfo.UseInprocessBroker = true;
            }

            if (unitType != SessionUnitType.Core)
            {
                startInfo.SessionResourceUnitType = unitType;
            }

            startInfo.MaximumUnits = maxUnit;
            startInfo.MinimumUnits = minUnit;

            return startInfo;
        }

        [TestInitialize]
        public void TestInit()
        {
            var HNMachine = Environment.GetEnvironmentVariable(HNEnvName, EnvironmentVariableTarget.User);
            Server = string.IsNullOrEmpty(HNMachine) ? DefaultServer : HNMachine;
        }

        /// <summary>
        /// This case matches with V3_AI_BVT_2 (Simple Fire & Recollect scenario - insecure net.tcp)
        /// </summary>
        [TestMethod]
        public void BvtDurableCase1()
        {
            Info("Start BVT");
            SessionStartInfo sessionStartInfo;

            sessionStartInfo = BuildSessionStartInfo(Server, EchoSvcName, null, null, null, null, SessionUnitType.Node, null, null, null);
            sessionStartInfo.Secure = false;
            string serviceJobId;
            Info("Begin to create Durable Session.");
            string guid = Guid.NewGuid().ToString();
            using (DurableSession session = DurableSession.CreateSession(sessionStartInfo))
            {
                serviceJobId = session.Id;
                var epr = new EndpointAddress(string.Format(NetTcpEndpointPattern, Server, serviceJobId));
                Info("EPR: {0}", epr);
                try
                {
                    Info("Client {0}: Begin to send requests.", guid);
                    using (BrokerClient<IEchoSvc> client = new BrokerClient<IEchoSvc>(guid, session))
                    {
                        for (int i = 0; i < NumberOfCalls; i++)
                        {
                            client.SendRequest<EchoRequest>(new EchoRequest(i.ToString()), i + ":" + guid);
                        }

                        Info("Client {0}: Begin to call EndOfMessage.", guid);
                        client.EndRequests();
                    }
                }
                catch (Exception e)
                {
                    Error("Unexpected exception of Client {0}", e.ToString());
                    throw;
                }
            }

            // sleep 10 seconds
            Info("Client disconnects and sleep 10 seconds");
            Thread.Sleep(10000);

            SessionAttachInfo sessionAttachInfo = new SessionAttachInfo(Server, serviceJobId);
            int count = 0;
            Info("Begin to attach Durable Session.");
            try
            {
                using (DurableSession session = DurableSession.AttachSession(sessionAttachInfo))
                {
                    Info("Begin to retrieve results.");
                    using (BrokerClient<IEchoSvc> client = new BrokerClient<IEchoSvc>(guid, session))
                    {
                        foreach (BrokerResponse<EchoResponse> response in client.GetResponses<EchoResponse>())
                        {
                            Info(response.Result.EchoResult);
                            string[] rtn = response.Result.EchoResult.Split(new[] { ':' });
                            Assert(
                                rtn[rtn.Length - 1] == response.GetUserData<string>().Split(new[] { ':' })[0] && response.GetUserData<string>().Split(new[] { ':' })[1] == guid,
                                "Result is corrupt: expected:computername:{0}, actual:{1}",
                                response.GetUserData<string>().Split(new[] { ':' })[0],
                                response.Result.EchoResult);
                            count++;
                        }
                    }

                    session.Close();
                }

                if (NumberOfCalls == count)
                {
                    Info("Total {0} calls returned.", count);
                }
                else
                {
                    Error("Total {0} calls returned, but losing {1} results.\n", count, NumberOfCalls - count);
                }
            }
            catch (Exception e)
            {
                Error("Unexpected exception during attaching and getting response {0}", e.ToString());
                throw;
            }
        }

        /// <summary>
        /// This case matches with V3_AI_BVT_6 (non-secure net.tcp Durable Session - multiple BrokerClient)
        /// </summary>
        [TestMethod]
        public void BvtDurableCase2()
        {
            Info("Start BVT");
            SessionStartInfo sessionStartInfo;

            sessionStartInfo = BuildSessionStartInfo(Server, EchoSvcName, null, null, null, null, SessionUnitType.Node, null, null, null);
            sessionStartInfo.Secure = false;
            Info("Begin to create session");
            string serviceJobId;
            int clientNum = 2;
            AutoResetEvent anotherClient = new AutoResetEvent(false);

            Task[] tasks = new Task[clientNum];
            DurableSession session = DurableSession.CreateSession(sessionStartInfo);
            serviceJobId = session.Id;

            SessionAttachInfo sessionAttachInfo = new SessionAttachInfo(Server, serviceJobId);
            for (int i = 0; i < clientNum; i++)
            {
                var idx = i;
                tasks[i] = Task.Run(
                    () =>
                    {
                        string guid = Guid.NewGuid().ToString();
                        using (DurableSession attachSession = DurableSession.AttachSession(sessionAttachInfo))
                        {
                            try
                            {
                                Info("Client {0}: Begin to send requests.", guid);
                                using (BrokerClient<IEchoSvc> client = new BrokerClient<IEchoSvc>(guid, attachSession))
                                {
                                    for (int j = 0; j < NumberOfCalls; j++)
                                    {
                                        client.SendRequest<EchoRequest>(new EchoRequest(j.ToString()), j + ":" + guid);
                                    }

                                    Info("Client {0}: Begin to call EndOfMessage.", guid);
                                    client.EndRequests();
                                    Info("Client {0}: Begin to get responses.", guid);
                                    int count = 0;
                                    if (idx == 0)
                                    {
                                        foreach (BrokerResponse<EchoResponse> response in client.GetResponses<EchoResponse>())
                                        {
                                            count++;
                                            Info(response.Result.EchoResult);
                                            string[] rtn = response.Result.EchoResult.Split(new[] { ':' });
                                            Assert(
                                                rtn[rtn.Length - 1] == response.GetUserData<string>().Split(new[] { ':' })[0] && response.GetUserData<string>().Split(new[] { ':' })[1] == guid,
                                                "Result is corrupt: expected:computername:{0}, actual:{1}",
                                                response.GetUserData<string>().Split(new[] { ':' })[0],
                                                response.Result.EchoResult);
                                        }
                                    }
                                    else
                                    {
                                        foreach (var response in client.GetResponses())
                                        {
                                            count++;
                                            EchoResponse result = (EchoResponse)response.Result;
                                            Info(result.EchoResult);
                                            string[] rtn = result.EchoResult.Split(new[] { ':' });
                                            Assert(
                                                rtn[rtn.Length - 1] == response.GetUserData<string>().Split(new[] { ':' })[0] && response.GetUserData<string>().Split(new[] { ':' })[1] == guid,
                                                "Result is corrupt: expected:computername:{0}, actual:{1}",
                                                response.GetUserData<string>(),
                                                result.EchoResult);
                                        }
                                    }

                                    if (count == NumberOfCalls)
                                    {
                                        Info("Client {0}: Total {1} calls returned.", guid, count);
                                    }
                                    else
                                    {
                                        Error("Client {0}: Total {1} calls returned, but losing {2} results.", guid, count, NumberOfCalls - count);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Error("Unexpected exception of Client {0}", e.ToString());
                                throw;
                            }
                            finally
                            {
                                if (Interlocked.Decrement(ref clientNum) <= 0)
                                {
                                    anotherClient.Set();
                                }
                            }
                        }
                    });
            }

            anotherClient.WaitOne();
            Task.WaitAll(tasks);
            session.Close(true);
            session.Dispose();
        }

        /// <summary>
        /// This case matches with V3_AI_BVT_8 (non-secure net.tcp shared Durable Session - BrokerClient)
        /// </summary>
        [TestMethod]
        public void BvtDurableCase3()
        {
            Info("Start BVT");
            SessionStartInfo sessionStartInfo;

            sessionStartInfo = BuildSessionStartInfo(Server, EchoSvcName, null, null, null, null, SessionUnitType.Node, null, null, null);
            sessionStartInfo.Secure = false;
            sessionStartInfo.ShareSession = true;

            Info("Begin to create session");
            string serviceJobId;
            int clientNum = 2;
            AutoResetEvent anotherClient = new AutoResetEvent(false);

            using (DurableSession session = DurableSession.CreateSession(sessionStartInfo))
            {
                serviceJobId = session.Id;
                var epr = new EndpointAddress(string.Format(NetTcpEndpointPattern, Server, serviceJobId));
                Info("EPR: {0}", epr);
                Task[] tasks = new Task[clientNum];
                for (int i = 0; i < clientNum; i++)
                {
                    var idx = i;
                    tasks[i] = Task.Run(
                        () =>
                        {
                            string guid = Guid.NewGuid().ToString();
                            try
                            {
                                Info("Client {0}: Begin to send requests.", guid);
                                using (BrokerClient<IEchoSvc> client = new BrokerClient<IEchoSvc>(guid, session))
                                {
                                    for (int j = 0; j < NumberOfCalls; j++)
                                    {
                                        client.SendRequest<EchoRequest>(new EchoRequest(j.ToString()), j + ":" + guid);
                                    }

                                    Info("Client {0}: Begin to call EndOfMessage.", guid);
                                    client.EndRequests();
                                    Info("Client {0}: Begin to get responses.", guid);
                                    int count = 0;
                                    if (idx == 0)
                                    {
                                        foreach (BrokerResponse<EchoResponse> response in client.GetResponses<EchoResponse>())
                                        {
                                            count++;
                                            Info(response.Result.EchoResult);
                                            string[] rtn = response.Result.EchoResult.Split(new[] { ':' });
                                            Assert(
                                                rtn[rtn.Length - 1] == response.GetUserData<string>().Split(new[] { ':' })[0] && response.GetUserData<string>().Split(new[] { ':' })[1] == guid,
                                                "Result is corrupt: expected:computername:{0}, actual:{1}",
                                                response.GetUserData<string>().Split(new[] { ':' })[0],
                                                response.Result.EchoResult);
                                        }
                                    }
                                    else
                                    {
                                        foreach (var response in client.GetResponses())
                                        {
                                            count++;
                                            EchoResponse result = (EchoResponse)response.Result;
                                            Info(result.EchoResult);
                                            string[] rtn = result.EchoResult.Split(new[] { ':' });
                                            Assert(
                                                rtn[rtn.Length - 1] == response.GetUserData<string>().Split(new[] { ':' })[0] && response.GetUserData<string>().Split(new[] { ':' })[1] == guid,
                                                "Result is corrupt: expected:computername:{0}, actual:{1}",
                                                response.GetUserData<string>(),
                                                result.EchoResult);
                                        }
                                    }

                                    if (count == NumberOfCalls)
                                    {
                                        Info("Client {0}: Total {1} calls returned.", guid, count);
                                    }
                                    else
                                    {
                                        Error("Client {0}: Total {1} calls returned, but losing {2} results.", guid, count, NumberOfCalls - count);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Error("Unexpected exception of Client {0}", e.ToString());
                                throw;
                            }
                            finally
                            {
                                if (Interlocked.Decrement(ref clientNum) <= 0)
                                {
                                    anotherClient.Set();
                                }
                            }
                        });
                }

                anotherClient.WaitOne();
                Task.WaitAll(tasks);
                session.Close(true);
                session.Dispose();
            }
        }
    }
}
