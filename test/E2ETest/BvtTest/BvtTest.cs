namespace BvtTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;

    using AITestLib.Helper;

    using Microsoft.ComputeCluster.Test.AppIntegration.EchoService.MessageContract;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using EchoSvcClient = Microsoft.ComputeCluster.Test.AppIntegration.EchoService.EchoSvcClient;

    [TestClass]
    public class BvtTest
    {
        private static string Server = "localhost";

        private static string EchoSvcName = "CcpEchoSvc";

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

        private static T CreateV2WCFTestServiceClient<T, TChannel>(int sessionId, EndpointAddress epr, NetTcpBinding binding)
            where TChannel : class
        {
            Type serviceClientType = typeof(T);
            object client = Activator.CreateInstance(serviceClientType, binding, epr);
            ((ClientBase<TChannel>)client).Endpoint.Behaviors.Add(new V2WCFClientEndpointBehavior(sessionId));

            return (T)client;
        }

        /// <summary>
        /// This case matches with AI_BVT_2 (Interactive Mode Basic Functional (BVT) - non-secure net.tcp)
        /// </summary>
        [TestMethod]
        public void BvtCase1()
        {
            Info("Start BVT");
            List<string> results = new List<string>();
            DateTime? firstresponse = null;
            SessionStartInfo sessionStartInfo = null;

            sessionStartInfo = BuildSessionStartInfo(Server, EchoSvcName, null, null, null, null, SessionUnitType.Node, null, null, null);

            Info("Begin to create session");
            int serviceJobId = -1;
            using (Session session = Session.CreateSession(sessionStartInfo))
            {
                serviceJobId = session.Id;
                var epr = new EndpointAddress(string.Format(NetTcpEndpointPattern, Server, serviceJobId));
                Info("EPR: {0}", epr);
                EchoSvcClient client =
                    CreateV2WCFTestServiceClient<EchoSvcClient, Microsoft.ComputeCluster.Test.AppIntegration.EchoService.IEchoSvc>(
                        serviceJobId,
                        epr,
                        new NetTcpBinding(SecurityMode.None));

                AutoResetEvent evt = new AutoResetEvent(false);
                int count = NumberOfCalls, outbound = NumberOfCalls;
                Info("Begin to send requests");
                DateTime firstrequest = DateTime.Now;
                for (int i = 0; i < count; i++)
                {
                    client.BeginEcho(
                        i.ToString(),
                        delegate(IAsyncResult result)
                            {
                                try
                                {
                                    int idx = (int)result.AsyncState;
                                    if (firstresponse == null)
                                    {
                                        firstresponse = DateTime.Now;
                                    }

                                    string rtn = client.EndEcho(result);
                                    rtn = string.Format("{0}: {1}", idx, rtn);
                                    lock (results)
                                    {
                                        results.Add(rtn);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Error(string.Format("Unexpected error:{0}", e.Message));
                                    throw;
                                }

                                if (Interlocked.Decrement(ref outbound) <= 0)
                                {
                                    evt.Set();
                                }
                            },
                        i);
                }

                evt.WaitOne();

                // step 3.4 print out result
                foreach (string res in results)
                {
                    TraceEvent(res);
                }

                Info(string.Format("Total {0} calls returned.", results.Count));

                if (firstresponse != null)
                {
                    Info(string.Format("First response come back in {0} milliseconds for {1} requests.", ((DateTime)firstresponse - firstrequest).TotalMilliseconds, count));
                }

                Info(string.Format("Total {0} calls returned.", count));
                client.Close();
            }

            // TODO: implement below methods
            // TraceLogger.LogSessionClosed(serviceJobId);
            // VerifyJobStatus(serviceJobId);
        }

        /// <summary>
        /// This case matches with AI_BVT_5 (Interactive Mode Basic Functional (BVT) - non-secure net.tcp - multiple BrokerClient)
        /// </summary>
        [TestMethod]
        public void BvtCase2()
        {
            Info("Start BVT");
            List<string> results = new List<string>();
            SessionStartInfo sessionStartInfo;

            sessionStartInfo = BuildSessionStartInfo(Server, EchoSvcName, null, null, null, null, SessionUnitType.Node, null, null, null);

            sessionStartInfo.Secure = false;
            sessionStartInfo.BrokerSettings.SessionIdleTimeout = 60 * 10 * 1000;
            Info("Begin to create session");
            int serviceJobId = -1;
            int clients = 2;
            AutoResetEvent evt = new AutoResetEvent(false);
            using (Session session = Session.CreateSession(sessionStartInfo))
            {
                serviceJobId = session.Id;
                var epr = new EndpointAddress(string.Format(NetTcpEndpointPattern, Server, serviceJobId));
                Info("EPR: {0}", epr);
                Task[] tasks = new Task[2];
                for (int i = 0; i < 2; i++)
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

                                        if (count == NumberOfCalls) Info("Client {0}: Total {1} calls returned.", guid, count);
                                        else
                                            Error("Client {0}: Total {1} calls returned, but losing {2} results.", guid, count, NumberOfCalls - count);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Error("Unexpected exception of Client {0}", e.ToString());
                                    throw;
                                }
                                finally
                                {
                                    if (Interlocked.Decrement(ref clients) <= 0) evt.Set();
                                }
                            });
                }

                evt.WaitOne();
                Task.WaitAll(tasks);
            }

            // VerifyJobStatus(serviceJobId);
        }

        /// <summary>
        /// Interactive Mode Basic Functional (BVT) - non-secure net.tcp - re-attached broker client
        /// </summary>
        [TestMethod]
        public void BvtCase3()
        {
            Info("Start BVT");
            List<string> results = new List<string>();
            SessionStartInfo sessionStartInfo;

            sessionStartInfo = BuildSessionStartInfo(Server, EchoSvcName, null, null, null, null, SessionUnitType.Node, null, null, null);

            sessionStartInfo.Secure = false;
            sessionStartInfo.BrokerSettings.SessionIdleTimeout = 60 * 10 * 1000;
            Info("Begin to create session");
            int serviceJobId = -1;
            int clients = 2;
            AutoResetEvent evt = new AutoResetEvent(false);
            Session session = Session.CreateSession(sessionStartInfo);
            SessionAttachInfo sessionAttachInfo = new SessionAttachInfo(sessionStartInfo.Headnode, session.Id);
            session = Session.AttachSession(sessionAttachInfo);

            serviceJobId = session.Id;
            var epr = new EndpointAddress(string.Format(NetTcpEndpointPattern, Server, serviceJobId));
            Info("EPR: {0}", epr);
            Task[] tasks = new Task[2];
            for (int i = 0; i < 2; i++)
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

                                    if (count == NumberOfCalls) Info("Client {0}: Total {1} calls returned.", guid, count);
                                    else
                                        Error("Client {0}: Total {1} calls returned, but losing {2} results.", guid, count, NumberOfCalls - count);
                                }
                            }
                            catch (Exception e)
                            {
                                Error("Unexpected exception of Client {0}", e.ToString());
                                throw;
                            }
                            finally
                            {
                                if (Interlocked.Decrement(ref clients) <= 0) evt.Set();
                            }
                        });
            }
            evt.WaitOne();
            Task.WaitAll(tasks);
            session.Close(true);
            session.Dispose();

            // VerifyJobStatus(serviceJobId);
        }
    }
}
