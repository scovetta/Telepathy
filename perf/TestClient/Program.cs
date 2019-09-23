// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace TestClient
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;

    using Microsoft.Telepathy.Session;

    /// <summary>
    /// Usage:
    /// Interactive Session's Usage: TestClient.exe [-h headnode] [-m max_cores] [-min min_cores] [-n req_count] [-r millisec_fo
    /// r_each_req] [-i bytes_for_each_req] [-o bytes_for_each_response] [-c common_data_for_each_req] [-sleep sleeptime_before_sending] [-cp common_data_pat
    /// h] [-batch batchCounts] [-save filename] -v2Client -onDemand
    /// Interactive Session's Usage with BrokerClient: TestClient.exe [-h headnode] [-m max_cores] [-min min_cores] [-n req_coun
    /// t] [-r millisec_for_each_req] [-i bytes_for_each_req] [-o bytes_for_each_response] [-c common_data_for_each_req] [-sleep sleeptime_before_sending] [-
    /// cp common_data_path] [-batch batchCounts] [-save filename] -onDemand
    /// Durable Session's Usage: TestClient.exe [-h headnode] [-m max_cores] [-min min_cores] [-n req_count] [-r millisec_for_ea
    /// ch_req] [-i bytes_for_each_req] [-o bytes_for_each_response] [-c common_data_for_each_req] [-sleep sleeptime_before_sending] [-cp common_data_path] [
    /// -batch batchCounts] [-save filename] -durable -onDemand
    ///
    /// Example:
    /// TestClient.exe -h qingzhi-win7 -m 4 -min 2 -n 100 -r 1000 -i 100 -c 100 -batch 10
    /// </summary>
    public class Program
    {
        private static string headnode = Environment.MachineName;
        private static string filename = string.Empty;

        private static long common_data_size = 0;
        private static bool common_data_compress = false;
        private static string commonData_dataClientId = string.Empty;

        private static int max_cores = 1;
        private static int min_cores = 1;
        private static int req_count = 1;
        private static int batchCount = 1;
        private static int millisec_for_each_req = 1000;

        internal static int sleep_before_sending;

        private static int flush_per_req = -1;

        private static long input_data_size;
        private static long output_data_size = 0;

        private static bool showhelp;
        private static bool durable;
        private static bool v2Client;
        private static bool http;
        private static bool responseHandler;
        private static bool no_log;
        private static bool no_chart;

        private static bool detail;

        private static bool multiThreads = false;

        private static bool createSessionOnly = false;
        private static string sessionId = "-1";

        private static bool inproc = false;
        private static bool standalone = false;

        private static string commaSeparatedTargetList = "127.0.0.1";

        private static string regPath = null;

        private static string username = string.Empty;
        private static string password = string.Empty;

        private static bool rest = false;
        private static bool onDemand = false; 
        private static bool custom = false; // Used for diagnosing customer issues

        // let request throws RetryOperationError and ignore it from client side
        private static bool retryRequestAndIgnoreRetryOperationError = false;
        private static int userTraceCount = 0;

        private static Object lockobject = new Object();



        private static StatisticData data = new StatisticData();


        private static string ServiceName = "TestService";

        private delegate void Logger(string msg, params object[] arg);
        private static Logger Log = new Logger(Utils.Log);

        static int Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback((a, b, c, d) => { return true; });

            ParseArgument(args);
            if (showhelp)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("Interactive Session's Usage: TestClient.exe [-h headnode] [-m max_cores] [-min min_cores] [-n req_count] [-r millisec_for_each_req] [-i bytes_for_each_req] [-c common_data_for_each_req] [-o bytes_for_each_response] [-sleep sleeptime_before_sending] [-cp common_data_path] [-client clientCounts] [-save filename] [-rest]");
                Console.WriteLine("Interactive Session's Usage with BrokerClient: TestClient.exe [-h headnode] [-m max_cores] [-min min_cores] [-n req_count] [-r millisec_for_each_req] [-i bytes_for_each_req] [-c common_data_for_each_req] [-o bytes_for_each_response] [-sleep sleeptime_before_sending] [-cp common_data_path] [-client clientCounts] [-save filename] [-rest] -interactiveNew");
                Console.WriteLine("Durable Session's Usage: TestClient.exe [-h headnode] [-m max_cores] [-min min_cores] [-n req_count] [-r millisec_for_each_req] [-i bytes_for_each_req] [-c common_data_for_each_req] [-o bytes_for_each_response] [-sleep sleeptime_before_sending] [-cp common_data_path] [-client clientCounts] [-save filename] [-rest] -durable");
                Console.WriteLine("The return code of the executable is the session Id which the program creates/attaches");
                return -1;
            }


            SessionStartInfo startInfo = new SessionStartInfo(headnode, ServiceName);
            startInfo.UseInprocessBroker = inproc;
            startInfo.IsNoSession = standalone;
            startInfo.RegPath = regPath;
            startInfo.Secure = false;
            startInfo.IpAddress = commaSeparatedTargetList.Split(',');
            

            if (http) startInfo.TransportScheme = TransportScheme.Http;
            startInfo.SessionResourceUnitType = SessionUnitType.Core;
            startInfo.MaximumUnits = max_cores;
            startInfo.MinimumUnits = min_cores;
            startInfo.BrokerSettings.SessionIdleTimeout = 60 * 60 * 1000;
            startInfo.BrokerSettings.MaxMessageSize = int.MaxValue;
            if (rest) startInfo.TransportScheme = TransportScheme.WebAPI;
            if (sleep_before_sending > 60 * 1000)
            {
                startInfo.BrokerSettings.SessionIdleTimeout = sleep_before_sending * 2;
            }
            if (!String.IsNullOrEmpty(username))
            {
                startInfo.Username = username;
            }
            if (!String.IsNullOrEmpty(password))
            {
                startInfo.Password = password;
            }
            if (retryRequestAndIgnoreRetryOperationError)
            {
                startInfo.Environments.Add("RetryRequest", bool.TrueString);
            }
            if (userTraceCount > 0)
            {
                startInfo.Environments.Add("UserTraceCount", userTraceCount.ToString());
            }

            //if create session only, create session first and then return exit code as session id
            if (createSessionOnly)
            {
                CreateSession(startInfo, durable);
            }

            data.StartInfo = startInfo;
            data.Count = req_count;
            data.Milliseconds = millisec_for_each_req;
            data.InputDataSize = input_data_size;
            data.CommonDataSize = common_data_size;
            data.OutputDataSize = output_data_size;
            data.Client = batchCount;
            data.IsDurable = durable;
            data.OnDemand = onDemand; 
            foreach (string arg in args)
            {
                data.Command += (" " + arg);
            }

            Log("****** Test begin: {0} ******", data.Command);
            data.SessionStart = DateTime.Now;
            Log("Begin to create session.");
            SessionBase session;
#if False
            if (durable)
            {
                if (sessionId == -1)
                {
                    session = DurableSession.CreateSession(startInfo);
                }
                else
                {
                    session = DurableSession.AttachSession(new SessionAttachInfo(headnode, sessionId));
                }
            }
            else
            {

                if (sessionId == -1)
                {
                    session = Session.CreateSession(startInfo);
                }
                else
                {
                    session = Session.AttachSession(new SessionAttachInfo(headnode, sessionId));
                }
            }
#endif

            session = Session.CreateSession(startInfo);


            Log("Session created: {0}.", session.Id);
            data.SessionCreated = DateTime.Now;
            data.SessionId = session.Id;

            RunTest(session, !v2Client);

            Log("Begin to close session.");
            data.CloseSessionStart = DateTime.Now;
            // if sessionId is set by user, it's mostly used by multi-client-one-session, so do not close it
            if (sessionId.Equals(-1))
            {
                try
                {
                    session.Close(true, 10 * 60 * 1000);
                }
                catch (Exception e)
                {
                    Log("Close session failed: {0}", e.ToString());
                }
                finally
                {
                    Log("Session closed.");
                }
            }
            data.SessionEnd = DateTime.Now;

            data.ProcessData();

            foreach (string str in Utils.GetOuputString(data))
            {
                Console.WriteLine(str);
            }
            if (string.IsNullOrEmpty(filename))
            {
                filename = "result" + DateTime.Now.ToString("yyyyMdHms");
            }

            if (detail)
            {
                Utils.SaveDetail(data, filename);
            }
            if (!no_log)
            {
                Utils.LogOutput(data, filename);
            }
            if (!no_chart)
            {
                Utils.DrawChart(data, filename);
            }
            //return data.SessionId;
            return 0;
        }


        private static void ParseArgument(string[] args)
        {
            ArgumentsParser parser = new ArgumentsParser(args);
            ArgumentsParser.SetIfExist(parser["servicename"], ref ServiceName);
            ArgumentsParser.SetIfExist(parser["h"], ref headnode);
            ArgumentsParser.SetIfExist(parser["m"], ref max_cores);
            ArgumentsParser.SetIfExist(parser["min"], ref min_cores);
            ArgumentsParser.SetIfExist(parser["n"], ref req_count);
            ArgumentsParser.SetIfExist(parser["r"], ref millisec_for_each_req);

            ArgumentsParser.SetIfExist(parser["i"], ref input_data_size);
            input_data_size -= 4;
            if (input_data_size < 0)
            {
                input_data_size = 0;
            }

            ArgumentsParser.SetIfExist(parser["o"], ref output_data_size);

            ArgumentsParser.SetIfExist(parser["c"], ref common_data_size);
            ArgumentsParser.SetIfExist(parser["c_compress"], ref common_data_compress);
            ArgumentsParser.SetIfExist(parser["sleep"], ref sleep_before_sending);

            ArgumentsParser.SetIfExist(parser["durable"], ref durable);
            ArgumentsParser.SetIfExist(parser["http"], ref http);
            ArgumentsParser.SetIfExist(parser["v2client"], ref v2Client);
            ArgumentsParser.SetIfExist(parser["responseHandler"], ref responseHandler);
            ArgumentsParser.SetIfExist(parser["batch"], ref batchCount);
            ArgumentsParser.SetIfExist(parser["nolog"], ref no_log);
            ArgumentsParser.SetIfExist(parser["detail"], ref detail);
            ArgumentsParser.SetIfExist(parser["nochart"], ref no_chart);
            multiThreads = !durable;
            ArgumentsParser.SetIfExist(parser["mt"], ref multiThreads);


            ArgumentsParser.SetIfExist(parser["save"], ref filename);

            ArgumentsParser.SetIfExist(parser["createSessionOnly"], ref createSessionOnly);
            ArgumentsParser.SetIfExist(parser["sessionId"], ref sessionId);

            ArgumentsParser.SetIfExist(parser["inproc"], ref inproc);
            ArgumentsParser.SetIfExist(parser["standalone"], ref standalone);
            ArgumentsParser.SetIfExist(parser["targetList"], ref commaSeparatedTargetList);
            ArgumentsParser.SetIfExist(parser["regPath"], ref regPath);

            ArgumentsParser.SetIfExist(parser["flushPerReq"], ref flush_per_req);
            if (flush_per_req == -1) flush_per_req = req_count;

            ArgumentsParser.SetIfExist(parser["u"], ref username);
            ArgumentsParser.SetIfExist(parser["p"], ref password);

            ArgumentsParser.SetIfExist(parser["help"], ref showhelp);
            ArgumentsParser.SetIfExist(parser["?"], ref showhelp);

            ArgumentsParser.SetIfExist(parser["rest"], ref rest);
            ArgumentsParser.SetIfExist(parser["custom"], ref custom);
            ArgumentsParser.SetIfExist(parser["onDemand"], ref onDemand);

            ArgumentsParser.SetIfExist(parser["retryReq"], ref retryRequestAndIgnoreRetryOperationError);
            ArgumentsParser.SetIfExist(parser["userTraceCount"], ref userTraceCount);
        }

        private static string CreateSession(SessionStartInfo startInfo, bool isDurable)
        {
            string sessionId;
#if False
            if (isDurable)
            {
                DurableSession session = DurableSession.CreateSession(startInfo);
                sessionId = session.Id;
            }
            else
            {
                Session session = Session.CreateSession(startInfo);
                session.AutoClose = false;
                sessionId = session.Id;
            }
#endif
            Session session = Session.CreateSession(startInfo);
            sessionId = session.Id;

            Log("Session created.");
            //Environment.ExitCode = sessionId;
            return sessionId;
        }

        private static void PrepareCommonData(string sessionId)
        {
            if (common_data_size > 0)
            {
#if HPCPACK
                byte[] common_data = new byte[common_data_size];
                Random r = new Random();
                r.NextBytes(common_data);
                commonData_dataClientId = sessionId.ToString();
                Log("Begin to write common data.");
                data.CreateDataClientStart = DateTime.Now; 
                using (DataClient dataClient = DataClient.Create(headnode, commonData_dataClientId, null, !onDemand))
                {
                    data.CreateDataClientEnd = DateTime.Now; 
                    dataClient.SetDataLifeCycle(new DataLifeCycle(sessionId));
                    data.WriteDataClientStart = DateTime.Now; 
                    dataClient.WriteRawBytesAll(common_data, common_data_compress);
                    data.WriteDataClientEnd = DateTime.Now; 
                    dataClient.Close();
                }
                Log("Common data written done.");
#else
                throw new NotSupportedException("No common data support yet.");
#endif
            }
            else
            {
                Log("Common data size is 0, skip creating common data.");
            }
        }

        private static void RunTest(SessionBase session, bool v3)
        {
            PrepareCommonData(session.Id);
            if (custom)
                CustomizedResponseHandlerTest(session);
            else if (v3)
                InternalTest(session, responseHandler, multiThreads);
            else
                InternalTestAsync(session);
        }


        private static void ClientSendRequest(BrokerClient<IService1> client, string clientId, int msg_count, byte[] task_data)
        {
            Log("Client {0}: Begin to send requests.", clientId);
            try
            {
                for (int j = 0; j < msg_count; j++)
                {
                    client.SendRequest<ComputeWithInputDataRequest>(new ComputeWithInputDataRequest(millisec_for_each_req, task_data, commonData_dataClientId, output_data_size, DateTime.Now));
                    if (flush_per_req != req_count && (j + 1) % flush_per_req == 0) client.Flush(10 * 60 * 1000);
                }

                Log("Client {0}: Begin to call EndOfMessage.", clientId);

                if (batchCount == 1)
                {
                    data.ReqEom = DateTime.Now;
                }

                client.EndRequests(10 * 60 * 1000);
                if (batchCount == 1)
                {
                    data.ReqEomDone = DateTime.Now;
                }
                Log("Client {0}: EndOfMessage done.", clientId);
            }
            catch (WebException e)
            {
                if (e.Response is HttpWebResponse)
                {
                    HttpWebResponse response = e.Response as HttpWebResponse;
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        Log("Unexpected WebException when client {0} sending requests: {1}", clientId, response.StatusCode, reader.ReadToEnd());
                    }
                    // Do not continue anymore
                    Environment.Exit(int.MinValue);
                }
                else throw e;
            }
            catch (Exception e)
            {
                Log("Unexpected exception when client {0} sending requests: {1}", clientId, e.ToString());
                // Do not continue anymore
                Environment.Exit(int.MinValue);
            }
        }

        private static List<ResultData> ClientGetResponse(BrokerClient<IService1> client, string clientId)
        {
            int faultCalls = 0;
            List<ResultData> results = new List<ResultData>();
            try
            {
                foreach (var response in client.GetResponses<ComputeWithInputDataResponse>())
                {
                    try
                    {
                        if (common_data_size > 0 && response.Result.ComputeWithInputDataResult.commonDataSize != common_data_size)
                        {
                            throw new Exception(string.Format("Common data is corrupted: expected: {0}, actual: {1}", common_data_size, response.Result.ComputeWithInputDataResult.commonDataSize));
                        }
                        results.Add(Utils.CreateResultData(response.Result));
                    }
                    catch (WebException e)
                    {
                        if (e.Response is HttpWebResponse)
                        {
                            HttpWebResponse httpresponse = e.Response as HttpWebResponse;
                            using (StreamReader reader = new StreamReader(httpresponse.GetResponseStream()))
                            {
                                Log("Unexpected WebException when client {0} sending requests: {1}", clientId, httpresponse.StatusCode, reader.ReadToEnd());
                            }
                            results.Add(Utils.CreateDummyResultData());
                            Interlocked.Increment(ref faultCalls);
                        }
                        else throw e;
                    }
                    catch (RetryOperationException)
                    {
                        if (retryRequestAndIgnoreRetryOperationError)
                        {
                            results.Add(Utils.CreateDummyResultData());
                        }
                        else throw;
                    }
                    catch (Exception e)
                    {
                        Log(e.ToString());
                        results.Add(Utils.CreateDummyResultData());
                        Interlocked.Increment(ref faultCalls);
                    }
                }
            }
            catch (Exception e)
            {
                Log("Client {0}: Unexpected Exception thrown from ClientGetResponse(): {1}",clientId,e.ToString());
            }

            Log("Client {0}: All requests returned.", clientId);

            if (DateTime.Now > data.RetrieveEnd)
            {
                data.RetrieveEnd = DateTime.Now;
            }

            return results;

        }

        private static void InternalGetResponseTest(SessionBase session, bool multiThreads)
        {

            if (sleep_before_sending > 0)
            {
                Thread.Sleep(sleep_before_sending);
            }

            if (multiThreads) InternalGetResponseTestMultiThreads(session);
            else InternalGetResponseTestSingleThread(session);
        }

        private static void InternalGetResponseTestMultiThreads(SessionBase session)
        {
            NetTcpBinding binding = Utils.CreateNetTcpBinding();

            int faultCalls = 0;
            byte[] task_data = new byte[input_data_size];
            (new Random()).NextBytes(task_data);

            AutoResetEvent allClientSendEvt = new AutoResetEvent(false);
            AutoResetEvent allClientDoneEvt = new AutoResetEvent(false);
            int clientSendCounting = batchCount;
            int clientDoneCounting = batchCount;
            data.SendStart = DateTime.Now;

            for (int i = 0; i < batchCount; i++)
            {
                int msg_count = req_count / batchCount;
                if (i == batchCount - 1)
                {
                    msg_count += req_count % batchCount;
                }

                string clientId = Environment.MachineName + "-" + i.ToString() + "-" + Process.GetCurrentProcess().Id.ToString();
                //Console.WriteLine("--Client Id = {0}", clientId);
                var client = new BrokerClient<IService1>(clientId, session, binding);

                Thread t = new Thread(new ThreadStart(() =>
                {
                    ClientSendRequest(client, clientId, msg_count, task_data);
                    if (Interlocked.Decrement(ref clientSendCounting) <= 0) allClientSendEvt.Set();
                }));

                Thread t2 = new Thread(new ThreadStart(() =>
                {
                    List<ResultData> results = ClientGetResponse(client, clientId);
                    lock (lockobject)
                    {
                        data.ResultCollection.AddRange(results);
                    }
                    try
                    {
                        client.Close(durable, 10 * 60 * 1000);
                    }
                    catch (Exception e)
                    {
                        Log("Purge client {0} failed: {1}", clientId, e.ToString());
                    }
                    finally
                    {
                        client.Close();
                        client.Dispose();
                        if (Interlocked.Decrement(ref clientDoneCounting) <= 0)
                        {
                            allClientDoneEvt.Set();
                        }
                    }
                }));
                t.Start();
                t2.Start();
            }

            allClientSendEvt.WaitOne();
            data.SendEnd = DateTime.Now;
            allClientDoneEvt.WaitOne();
            data.RetrieveEnd = DateTime.Now;
            data.FaultCount = faultCalls;
        }

        private static void InternalGetResponseTestSingleThread(SessionBase session)
        {
            NetTcpBinding binding = Utils.CreateNetTcpBinding();

            int faultCalls = 0;
            byte[] task_data = new byte[input_data_size];
            (new Random()).NextBytes(task_data);

            data.SendStart = DateTime.Now;

            for (int i = 0; i < batchCount; i++)
            {
                int msg_count = req_count / batchCount;
                if (i == batchCount - 1)
                {
                    msg_count += req_count % batchCount;
                }

                string clientId = Environment.MachineName + "-" + i.ToString() + "-" + Process.GetCurrentProcess().Id.ToString();

                using (var client = new BrokerClient<IService1>(clientId, session, binding))
                {
                    ClientSendRequest(client, clientId, msg_count, task_data);
                }
            }
            //Log("The max interval of SendRequest opertaion is {0} milliseconds.", sendInterval);
            data.SendEnd = DateTime.Now;

            for (int i = 0; i < batchCount; i++)
            {
                string clientId = Environment.MachineName + "-" + i.ToString() + "-" + Process.GetCurrentProcess().Id.ToString();
                using (var client = new BrokerClient<IService1>(clientId, session))
                {
                    List<ResultData> results = ClientGetResponse(client, clientId);                    
                    data.ResultCollection.AddRange(results);

                    try
                    {
                        client.Close(durable, 10 * 60 * 1000);
                    }
                    catch (Exception e)
                    {
                        Log("Purge client {0} failed: {1}", clientId, e.ToString());
                    }
                }
            }

            data.RetrieveEnd = DateTime.Now;
            data.FaultCount = faultCalls;
        }

        private static void InternalResponseHandlerTest(SessionBase session, bool multiThreads)
        {


            if (sleep_before_sending > 0)
            {
                Thread.Sleep(sleep_before_sending);
            }

            //double sendInterval = 0;
            if (multiThreads) InternalResponseHandlerTestMultiThreads(session);
            else InternalResponseHandlerTestSingleThread(session);
        }

        private static void InternalResponseHandlerTestMultiThreads(SessionBase session)
        {
            NetTcpBinding binding = Utils.CreateNetTcpBinding();
            byte[] task_data = new byte[input_data_size];
            (new Random()).NextBytes(task_data);

            AutoResetEvent allClientSendEvt = new AutoResetEvent(false);
            AutoResetEvent allClientDoneEvt = new AutoResetEvent(false);
            int clientSendCounting = batchCount;
            int clientDoneCounting = batchCount;
            data.SendStart = DateTime.Now;

            for (int i = 0; i < batchCount; i++)
            {
                int msg_count = req_count / batchCount;
                if (i == batchCount - 1)
                {
                    msg_count += req_count % batchCount;
                }

                string clientId = Environment.MachineName + "-" + i.ToString() + "-" + Process.GetCurrentProcess().Id.ToString();
                Thread t = new Thread(new ThreadStart(() =>
                {
                    AutoResetEvent batchDone = new AutoResetEvent(false);

                    using (var client = new BrokerClient<IService1>(clientId, session, binding))
                    {
                        ResponseHandlerBase handler;
                        handler = new ComputeWithInputDataResponseHandler(client, clientId, common_data_size, retryRequestAndIgnoreRetryOperationError);
                        client.SetResponseHandler<ComputeWithInputDataResponse>(((ComputeWithInputDataResponseHandler)handler).ResponseHandler<ComputeWithInputDataResponse>);

                        ClientSendRequest(client, clientId, msg_count, task_data);

                        if (Interlocked.Decrement(ref clientSendCounting) <= 0)
                        {
                            allClientSendEvt.Set();
                        }

                        handler.WaitOne();
                        Log("Client {0}: All requests returned.", clientId);

                        lock (lockobject)
                        {
                            data.ResultCollection.AddRange(handler.results);
                            data.FaultCount = handler.faultCalls;
                        }
                        try
                        {
                            client.Close(durable, 10 * 60 * 1000);
                        }
                        catch (Exception e)
                        {
                            Log("Purge client {0} failed: {1}", clientId, e.ToString());
                        }
                        if (Interlocked.Decrement(ref clientDoneCounting) <= 0)
                        {
                            allClientDoneEvt.Set();
                        }
                    }
                }));
                t.Start();
            }

            allClientSendEvt.WaitOne();
            data.SendEnd = DateTime.Now;
            allClientDoneEvt.WaitOne();
            data.RetrieveEnd = DateTime.Now;
        }

        private static void InternalResponseHandlerTestSingleThread(SessionBase session)
        {
            NetTcpBinding binding = Utils.CreateNetTcpBinding();
            byte[] task_data = new byte[input_data_size];
            (new Random()).NextBytes(task_data);

            data.SendStart = DateTime.Now;

            var responseHandlers = new List<ResponseHandlerBase>();
            for (int i = 0; i < batchCount; i++)
            {

                int msg_count = req_count / batchCount;
                if (i == batchCount - 1)
                {
                    msg_count += req_count % batchCount;
                }

                string clientId = Environment.MachineName + "-" + i.ToString() + "-" + Process.GetCurrentProcess().Id.ToString();
                var client = new BrokerClient<IService1>(clientId, session, binding);

                ResponseHandlerBase handler;
                handler = new ComputeWithInputDataResponseHandler(client, clientId, common_data_size, retryRequestAndIgnoreRetryOperationError);
                client.SetResponseHandler<ComputeWithInputDataResponse>(((ComputeWithInputDataResponseHandler)handler).ResponseHandler<ComputeWithInputDataResponse>);
                responseHandlers.Add(handler);
                ClientSendRequest(client, clientId, msg_count, task_data);

            }
            //Log("The max interval of SendRequest opertaion is {0} milliseconds.", sendInterval);
            data.SendEnd = DateTime.Now;

            foreach (ResponseHandlerBase handler in responseHandlers)
            {
                handler.WaitOne();
                data.ResultCollection.AddRange(handler.results);
                data.FaultCount += handler.faultCalls;
                try
                {
                    handler.client.Close(durable, 10 * 60 * 1000);
                }
                catch (Exception e)
                {
                    Log("Purge client {0} failed: {1}", handler.clientId.ToString(), e.ToString());
                }
            }

            data.RetrieveEnd = DateTime.Now;
        }

        private static void CustomizedResponseHandlerTest(SessionBase session)
        {
            NetTcpBinding binding = Utils.CreateNetTcpBinding();
            byte[] task_data = new byte[input_data_size];
            (new Random()).NextBytes(task_data);

            object mylock = default(object);

            for (int i = 0; i < req_count; i++)
            {
                string clientId = Guid.NewGuid().ToString();
                BrokerClient<IService1> client = new BrokerClient<IService1>(clientId, session, binding);
                ResponseHandlerBase handler;
                handler = new ComputeWithInputDataResponseHandler(client, clientId, common_data_size, retryRequestAndIgnoreRetryOperationError);

                // Measure the set response handler time
                Stopwatch watch = Stopwatch.StartNew();
                client.SetResponseHandler<ComputeWithInputDataResponse>(
                    response =>
                    {
                        lock (mylock)
                        {
                            response.Dispose();
                            client.Close();
                        }
                    }
                    );
                watch.Stop();
                Log("Elapsed time for SetResponseHandler is {0} milliseconds", watch.ElapsedMilliseconds);

                // Send requests
                client.SendRequest<ComputeWithInputDataRequest>(new ComputeWithInputDataRequest(millisec_for_each_req, task_data, commonData_dataClientId, output_data_size, DateTime.Now));
                data.ReqEom = DateTime.Now;
                client.EndRequests(10 * 60 * 1000);
                data.ReqEomDone = DateTime.Now;

                // Send every 0-4 sec
                //Thread.Sleep((new Random()).Next(4000));
            }
        }

        private static void InternalTest(SessionBase session, bool responseHandler, bool mutliThreads)
        {
            if (responseHandler) InternalResponseHandlerTest(session, mutliThreads);
            else InternalGetResponseTest(session, mutliThreads);
        }


        private static void InternalTestAsync(SessionBase session)
        {
            int faultCalls = 0;
            byte[] task_data = new byte[input_data_size];
            (new Random()).NextBytes(task_data);

            if (sleep_before_sending > 0)
            {
                Thread.Sleep(sleep_before_sending);
            }

            Binding binding;
            if (http) binding = Utils.CreateHttpBinding();
            else binding = Utils.CreateNetTcpBinding();

            int counting = batchCount;
            AutoResetEvent evt = new AutoResetEvent(false);
            data.SendStart = DateTime.Now;

            for (int i = 0; i < batchCount; i++)
            {
                int msg_count = req_count / batchCount;
                if (i == batchCount - 1)
                {
                    msg_count += req_count % batchCount;
                }

                string clientId = Environment.MachineName + "-" + i.ToString() + "-" + Process.GetCurrentProcess().Id.ToString();
                Thread t = new Thread(new ThreadStart(() =>
                {
                    int AsyncResultCount = msg_count;
                    AutoResetEvent AsyncResultsDone = new AutoResetEvent(false);

                    Log("Client {0}: Begin to send requests", clientId);
                    if (req_count == 0)
                    {
                        AsyncResultsDone.Set();
                    }

                    List<ResultData> results = new List<ResultData>();
                    Service1Client client = new Service1Client(binding, session.EndpointReference);

                    for (int k = 0; k < msg_count; k++)
                    {
                        ResultData result = new ResultData();
                        ComputeWithInputDataRequest request = new ComputeWithInputDataRequest
                        {
                            millisec = millisec_for_each_req,
                            input_data = task_data,
                            commonData_dataClientId = commonData_dataClientId,
                            responseSize = output_data_size,
                        };
                        client.BeginComputeWithInputData(request,
                            (AsyncCallback)delegate(IAsyncResult r)
                            {
                                try
                                {
                                    ResultData rtn = r.AsyncState as ResultData;
                                    ComputeWithInputDataResponse response = client.EndComputeWithInputData(r);
                                    rtn.TaskId = int.Parse(response.ComputeWithInputDataResult.CCP_TASKINSTANCEID);
                                    rtn.Start = response.ComputeWithInputDataResult.requestStartTime;
                                    rtn.End = response.ComputeWithInputDataResult.requestEndTime;

                                    lock (results)
                                    {
                                        results.Add(rtn);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log(e.ToString());
                                    lock (results)
                                    {
                                        results.Add(new ResultData(DateTime.Now,
                                                                   DateTime.Now,
                                                                   -1,
                                                                   DateTime.Now, DateTime.Now));
                                    }
                                    Interlocked.Increment(ref faultCalls);
                                }

                                if (Interlocked.Decrement(ref AsyncResultCount) <= 0)
                                {
                                    AsyncResultsDone.Set();
                                }
                            },
                            result);
                    }


                    AsyncResultsDone.WaitOne();

                    Log("Client {0}: All requests returned.", clientId);

                    if (DateTime.Now > data.RetrieveEnd)
                    {
                        data.RetrieveEnd = DateTime.Now;
                    }

                    client.Close();

                    lock (data)
                    {
                        data.ResultCollection.AddRange(results);
                    }



                    if (Interlocked.Decrement(ref counting) <= 0)
                    {
                        evt.Set();
                    }
                }));

                t.Start();
            }

            evt.WaitOne();

            data.FaultCount = faultCalls;
        }
    }
}
