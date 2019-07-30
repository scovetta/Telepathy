namespace BvtTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Threading;

    using AITestLib.Helper;

    using Microsoft.ComputeCluster.Test.AppIntegration.EchoService;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                EchoSvcClient client = CreateV2WCFTestServiceClient<EchoSvcClient, IEchoSvc>(serviceJobId, epr, new NetTcpBinding(SecurityMode.None));

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
    }
}