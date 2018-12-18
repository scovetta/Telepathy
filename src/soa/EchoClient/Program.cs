//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Main entry for CcpEchoSvc service client
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.EchoClient
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using EchoSvcLib;
    using Microsoft.Hpc.Scheduler.Session;
    using System.ServiceModel;

    public class Program
    {
        static void Main(string[] args)
        {
            CmdParser parser = new CmdParser(args);
            Config config = new Config(parser);

            if (config.HelpInfo)
            {
                config.PrintHelp();
                return;
            }

            if (config.Verbose)
            {
                config.PrintUsedParams(parser);
            }

            if (config.PrintUnusedParams(parser))
            {
                config.PrintHelp();
                return;
            }

            // strategies for the EchoClient
            SessionStartInfo info = null;
            if (config.IsNoSession)
            {
                // Start session without session manager
                if (config.InprocessBroker)
                {
                    info = new SessionStartInfo(config.ServiceName, config.RegPath, null, config.TargetList?.ToArray());
                    info.UseInprocessBroker = true;
                    info.IsNoSession = true;
                }
                else
                {
                    //TODO because registrying a broker in scheduler must have a appropriate session id in HPC pack
                    info = new SessionStartInfo(config.HeadNode, config.ServiceName, config.RegPath, null, config.TargetList?.ToArray());
                    info.UseInprocessBroker = false;
                    info.IsNoSession = true;
                }
            }
            else
            {
                //normal with Hpc
                info = new SessionStartInfo(config.HeadNode, config.ServiceName);
                info.IsNoSession = false;
                info.UseInprocessBroker = config.InprocessBroker;
            }

            if (!string.IsNullOrEmpty(config.Username))
            {
                info.Username = config.Username;
            }
            if (!string.IsNullOrEmpty(config.Password))
            {
                info.Password = config.Password;
            }
            if (!string.IsNullOrEmpty(config.JobName))
            {
                info.ServiceJobName = config.JobName;
            }

            if (!string.IsNullOrEmpty(config.AzureStorageConnectionString))
            {
                info.BrokerLauncherStorageConnectionString = config.AzureStorageConnectionString;
            }

            switch (config.ResourceType.ToLowerInvariant())
            {
                case "core":
                    info.SessionResourceUnitType = SessionUnitType.Core;
                    break;
                case "node":
                    info.SessionResourceUnitType = SessionUnitType.Node;
                    break;
                case "socket":
                    info.SessionResourceUnitType = SessionUnitType.Socket;
                    break;
                case "gpu":
                    info.SessionResourceUnitType = SessionUnitType.Gpu;
                    break;
                default:
                    break;
            }
            if (config.MaxResource > 0)
            {
                info.MaximumUnits = config.MaxResource;
            }
            if (config.MinResource > 0)
            {
                info.MinimumUnits = config.MinResource;
            }
            switch (config.TransportScheme.ToLowerInvariant())
            {
                case "nettcp":
                    info.TransportScheme = TransportScheme.NetTcp;
                    break;
                case "http":
                    info.TransportScheme = TransportScheme.Http;
                    break;
                case "nethttp":
                    info.TransportScheme = TransportScheme.NetHttp;
                    break;
                case "custom":
                    info.TransportScheme = TransportScheme.Custom;
                    break;
                default:
                    break;
            }
            info.JobTemplate = config.JobTemplate;
            info.SessionPriority = config.Priority;
            info.NodeGroupList = new List<string>(config.NodeGroups.Split(new char[] { ',' }));
            info.RequestedNodesList = new List<string>(config.Nodes.Split(new char[] { ',' }));
            info.Secure = !config.Insecure && !config.IsNoSession;
            info.UseAzureQueue = config.AzureQueue;
            info.ShareSession = config.ShareSession;
            info.UseSessionPool = config.SessionPool;
            info.ParentJobIds = StringToIntList(config.ParentIds);
            info.ServiceHostIdleTimeout = config.ServiceIdleSec == -1 ? config.ServiceIdleSec : config.ServiceIdleSec * 1000;
            info.ServiceHangTimeout = config.ServiceHangSec == -1 ? config.ServiceHangSec : config.ServiceHangSec * 1000;
            info.UseWindowsClientCredential = config.UseWCC;
            info.UseAad = config.UseAad;
            
            if (config.Runtime > 0)
            {
                info.Runtime = config.Runtime;
            }
            if (!string.IsNullOrEmpty(config.Environment))
            {
                foreach (string keyValue in config.Environment.Split(new char[] { ',' }))
                {
                    string[] p = keyValue.Split(new char[] { '=' });
                    if (p.Length == 2)
                    {
                        info.Environments.Add(p[0], p[1]);
                    }
                }
            }

            Stopwatch watch = new Stopwatch();
            int timeoutMilliSec = config.MsgTimeoutSec * 1000;

            Dictionary<Guid, Dictionary<Guid, TaskRecord>> brokerClientTaskTimeRecords = new Dictionary<Guid, Dictionary<Guid, TaskRecord>>(config.BrokerClient);
            Dictionary<Guid, DateTime> brokerSendRequestStartTime = new Dictionary<Guid, DateTime>(config.BrokerClient);
            Dictionary<Guid, DateTime> brokerSendRequestEndTime = new Dictionary<Guid, DateTime>(config.BrokerClient);
            Dictionary<Guid, DateTime> brokerGetResponseStartTime = new Dictionary<Guid, DateTime>(config.BrokerClient);
            Dictionary<Guid, DateTime> brokerGetResponseEndTime = new Dictionary<Guid, DateTime>(config.BrokerClient);

            Random rTimeMS = new Random();
            Random rSizeKB = new Random();
            int maxTimeMS = 0;
            int minTimeMS = 0;
            int maxSizeKB = 0;
            int minSizeKB = 0;
            if (!string.IsNullOrEmpty(config.TimeMSRandom))
            {
                string[] values = config.TimeMSRandom.Split(new char[] {'_'}, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length == 2)
                {
                    int.TryParse(values[0], out minTimeMS);
                    int.TryParse(values[1], out maxTimeMS);
                }
            }
            if (!string.IsNullOrEmpty(config.SizeKBRandom))
            {
                string[] values = config.SizeKBRandom.Split(new char[] {'_'}, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length == 2)
                {
                    int.TryParse(values[0], out minSizeKB);
                    int.TryParse(values[1], out maxSizeKB);
                }
            }

            for (int c = 0; c < config.BrokerClient; c++)
            {
                Dictionary<Guid, TaskRecord> taskTimeReconds = new Dictionary<Guid, TaskRecord>(config.NumberOfRequest);
                for (int i = 0; i < config.NumberOfRequest; i++)
                {
                    Guid g = Guid.NewGuid();
                    TaskRecord t = new TaskRecord() { RequestTime = DateTime.MinValue, ResponseTime = DateTime.MinValue };
                    if (maxTimeMS > 0)
                    {
                        t.CallDurationMS = rTimeMS.Next(minTimeMS, maxTimeMS);
                    }
                    else
                    {
                        t.CallDurationMS = config.CallDurationMS;
                    }
                    if (maxSizeKB > 0)
                    {
                        t.MessageSizeByte = rSizeKB.Next(minSizeKB, maxSizeKB) * 1024;
                    }
                    else
                    {
                        t.MessageSizeByte = config.MessageSizeByte;
                    }
                    taskTimeReconds.Add(g, t);
                }
                Guid clientGuid = Guid.NewGuid();
                brokerClientTaskTimeRecords.Add(clientGuid, taskTimeReconds);
                brokerSendRequestStartTime.Add(clientGuid, DateTime.MinValue);
                brokerSendRequestEndTime.Add(clientGuid, DateTime.MinValue);
                brokerGetResponseStartTime.Add(clientGuid, DateTime.MinValue);
                brokerGetResponseEndTime.Add(clientGuid, DateTime.MinValue);
            }

            // Create an interactive or durable session
            Logger.Info("Creating a session for CcpEchoSvc service...");
            SessionBase session = null;

            try
            {
                watch.Start();
                if (config.Durable)
                {
                    session = DurableSession.CreateSession(info);
                }
                else
                {
                    session = info.IsNoSession ? (info.UseInprocessBroker? Session.CreateIPSession(info) : Session.CreateBrkSession(info)) : Session.CreateSession(info);
                }
                watch.Stop();
                Logger.Info("{0, -35} : {1}", "Session ID", session.Id);
                Logger.Info("{0, -35} : {1:F3} sec", "Session creation time", watch.Elapsed.TotalSeconds);

                //session warm up time
                if (config.WarmupTimeSec > 0)
                {
                    Logger.Info("Session warming up in {0} seconds...", config.WarmupTimeSec);
                    Thread.Sleep(config.WarmupTimeSec * 1000);
                }

                int clientNumber = config.BrokerClient;
                int clientCounter = 0;
                AutoResetEvent allDone = new AutoResetEvent(false);

                foreach (Guid g in brokerClientTaskTimeRecords.Keys)
                {
                    ThreadPool.QueueUserWorkItem((o) =>
                        {
                            Guid brokerClientGuid = (Guid)o;
                            AutoResetEvent done = new AutoResetEvent(false);
                            int count = 0;
                            int clientC = Interlocked.Increment(ref clientCounter);

                            Stopwatch watchT = new Stopwatch();
                            try
                            {
                                // Create a BrokerClient proxy
                                using (BrokerClient<IEchoSvc> client = new BrokerClient<IEchoSvc>(brokerClientGuid.ToString(), session))
                                {
                                    if (config.AsyncResponseHandler)
                                    {
                                        //set getresponse handler
                                        Logger.Info("Setting response handler ({0}/{1}) to receive responses async.", clientC, config.BrokerClient);
                                        client.SetResponseHandler<GenerateLoadResponse>((item) =>
                                        {
                                            try
                                            {
                                                Guid gg = item.RequestMessageId;
                                                StatisticInfo si = item.Result.GenerateLoadResult;
                                                if (config.Verbose)
                                                {
                                                    Logger.Info("Response async received ({0}/{1}) {2} : {3}. StartTime-EndTime : {4:HH:mm:ss.fff}-{5:HH:mm:ss.fff}", clientC, config.BrokerClient, item.GetUserData<int>(), gg, si.StartTime, si.EndTime);
                                                }
                                                brokerClientTaskTimeRecords[brokerClientGuid][gg].ResponseTime = DateTime.Now;
                                            }
                                            catch (FaultException ex)
                                            {
                                                Logger.Warning("FaultException while getting responses in callback. \n{0}", ex.ToString());
                                            }
                                            catch (RetryOperationException ex)
                                            {
                                                Logger.Warning("RetryOperationException while getting responses in callback. \n{0}", ex.ToString());
                                            }
                                            catch (SessionException ex)
                                            {
                                                Logger.Warning("SessionException while getting responses in callback. \n{0}", ex.ToString());
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warning("Exception while getting responses in callback. \n{0}", ex.ToString());
                                            }

                                            if (Interlocked.Increment(ref count) == config.NumberOfRequest)
                                            {
                                                done.Set();
                                            }
                                        });
                                    }

                                    Logger.Info("Sending {0} requests for broker client ({1}/{2})...", config.NumberOfRequest, clientC, config.BrokerClient);
                                    brokerSendRequestStartTime[brokerClientGuid] = DateTime.Now;
                                    watchT.Restart();
                                    int i = 0;
                                    foreach (Guid requestGuid in brokerClientTaskTimeRecords[brokerClientGuid].Keys)
                                    {
                                        i++;
                                        GenerateLoadRequest request = new GenerateLoadRequest(brokerClientTaskTimeRecords[brokerClientGuid][requestGuid].CallDurationMS, new byte[brokerClientTaskTimeRecords[brokerClientGuid][requestGuid].MessageSizeByte], null);
                                        client.SendRequest<GenerateLoadRequest>(request, i, timeoutMilliSec, new System.Xml.UniqueId(requestGuid));
                                        if (config.Verbose)
                                        {
                                            Logger.Info("Sent request {0} for ({1}/{2}) : {3} : timeMS - {4} sizeByte - {5}", i, clientC, config.BrokerClient, requestGuid, brokerClientTaskTimeRecords[brokerClientGuid][requestGuid].CallDurationMS, brokerClientTaskTimeRecords[brokerClientGuid][requestGuid].MessageSizeByte);
                                        }
                                        brokerClientTaskTimeRecords[brokerClientGuid][requestGuid].RequestTime = DateTime.Now;
                                        if (config.Flush > 0 && i % config.Flush == 0)
                                        {
                                            client.Flush(timeoutMilliSec);
                                        }
                                    }

                                    // Flush the message
                                    client.EndRequests(timeoutMilliSec);
                                    watchT.Stop();
                                    brokerSendRequestEndTime[brokerClientGuid] = DateTime.Now;
                                    double requestElapsedSeconds = watchT.Elapsed.TotalSeconds;
                                    double requestThroughput = config.NumberOfRequest / requestElapsedSeconds;
                                    Logger.Info("{0, -35} : {1:F3} sec", string.Format("Requests sent time ({0}/{1})", clientC, config.BrokerClient), requestElapsedSeconds);
                                    Logger.Info("{0, -35} : {1:F2} /sec", string.Format("Requests throughput ({0}/{1})", clientC, config.BrokerClient), requestThroughput);

                                    if (!config.AsyncResponseHandler)
                                    {
                                        Logger.Info("Retrieving responses for broker client ({0}/{1})...", clientC, config.BrokerClient);
                                        try
                                        {
                                            brokerGetResponseStartTime[brokerClientGuid] = DateTime.Now;
                                            watchT.Restart();
                                            int responseNumber = 0;
                                            foreach (BrokerResponse<GenerateLoadResponse> response in client.GetResponses<GenerateLoadResponse>(timeoutMilliSec))
                                            {
                                                try
                                                {
                                                    Guid gg = response.RequestMessageId;
                                                    StatisticInfo si = response.Result.GenerateLoadResult;
                                                    if (config.Verbose)
                                                    {
                                                        Logger.Info("Response received ({0}/{1}) {2} : {3}. StartTime-EndTime : {4:HH:mm:ss.fff}-{5:HH:mm:ss.fff}", clientC, config.BrokerClient, response.GetUserData<int>(), gg, si.StartTime, si.EndTime);
                                                    }
                                                    brokerClientTaskTimeRecords[brokerClientGuid][gg].ResponseTime = DateTime.Now;
                                                }
                                                catch (FaultException e)
                                                {
                                                    // Application exceptions
                                                    Logger.Warning("FaultException when getting responses. \n{0}", e.ToString());
                                                }
                                                catch (RetryOperationException e)
                                                {
                                                    // RetryOperationExceptions may or may not be recoverable
                                                    Logger.Warning("RetryOperationException when getting responses. \n{0}", e.ToString());
                                                }
                                                catch (SessionException e)
                                                {
                                                    // Exception
                                                    Logger.Warning("SessionException when getting responses. \n{0}", e.ToString());
                                                }
                                                catch (Exception e)
                                                {
                                                    // Exception
                                                    Logger.Warning("Exception when getting responses. \n{0}", e.ToString());
                                                }
                                                finally
                                                {
                                                    responseNumber++;
                                                }
                                            }
                                            watchT.Stop();
                                            brokerGetResponseEndTime[brokerClientGuid] = DateTime.Now;
                                            double elapsedTimeSec = watchT.Elapsed.TotalSeconds;
                                            Logger.Info("{0, -35} : {1:F3} sec", string.Format("GetResponses time ({0}/{1})", clientC, config.BrokerClient), elapsedTimeSec);
                                            Logger.Info("{0, -35} : {1:F2} /sec", string.Format("GetResponses throughput ({0}/{1})", clientC, config.BrokerClient), responseNumber / elapsedTimeSec);
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.Error("Error occured getting responses.\n{0}", ex.ToString());
                                        }
                                    }
                                    else
                                    {
                                        //wait for receiving responses async.
                                        done.WaitOne();
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                //swallow the exception in the thread.
                                Logger.Error("Error occured in broker client thread.\n{0}", e.ToString());
                            }

                            if (Interlocked.Decrement(ref clientNumber) == 0)
                            {
                                allDone.Set();
                            }

                        }, g);
                } // for t

                Logger.Info("Wait for all broker clients.");
                allDone.WaitOne();


            }
            catch (Exception e)
            {
                Logger.Error("Error occured.\n{0}", e.ToString());
            }
            finally
            {
                if (session != null)
                {
                    //explict close the session to free the resource
                    session.Close(!config.ShareSession);
                    session.Dispose();
                }
            }

            //calc the request/response throughput for all broker client
            double allRequestsElapsedSeconds = (brokerSendRequestEndTime.Values.Max() - brokerSendRequestStartTime.Values.Min()).TotalSeconds;
            double allRequestThroughput = config.NumberOfRequest * config.BrokerClient / allRequestsElapsedSeconds;
            Logger.Info("{0, -35} : {1:F3} sec", "All requests sending time", allRequestsElapsedSeconds);
            Logger.Info("{0, -35} : {1:F2} /sec", "All requests throughput", allRequestThroughput);

            double allResponsesElapsedSeconds = (brokerGetResponseEndTime.Values.Max() - brokerGetResponseStartTime.Values.Min()).TotalSeconds;
            double allResponseThroughput = config.NumberOfRequest * config.BrokerClient / allResponsesElapsedSeconds;
            Logger.Info("{0, -35} : {1:F3} sec", "All resposnes receiving time", allResponsesElapsedSeconds);
            Logger.Info("{0, -35} : {1:F2} /sec", "All responses throughput", allResponseThroughput);


            //calc the min/max/average request e2e time
            double[] times = new double[config.NumberOfRequest * config.BrokerClient];
            DateTime[] dates = new DateTime[config.NumberOfRequest * config.BrokerClient];
            DateTime[] dates_req = new DateTime[config.NumberOfRequest * config.BrokerClient];

            int k = 0;
            foreach (Guid g in brokerClientTaskTimeRecords.Keys)
            {
                foreach (TaskRecord r in brokerClientTaskTimeRecords[g].Values)
                {
                    times[k] = (r.ResponseTime - r.RequestTime).TotalSeconds;
                    dates[k] = r.ResponseTime;
                    dates_req[k] = r.RequestTime;
                    k++;
                }
            }

            Logger.Info("{0, -35} : {1:F3} sec", "Response time Min", times.Min());
            Logger.Info("{0, -35} : {1:F3} sec", "Response time Max", times.Max());
            Logger.Info("{0, -35} : {1:F3} sec", "Response time Ave", times.Average());
            DateTime first = dates.Min();
            DateTime last = dates.Max();
            DateTime first_req = dates_req.Min();
            double elapsedSec = (last - first).TotalSeconds;
            double elapsedSec_req = (last - first_req).TotalSeconds;
            Logger.Info("{0, -35} : {1:HH:mm:ss.fff}", "Response first", first);
            Logger.Info("{0, -35} : {1:HH:mm:ss.fff}", "Response last", last);
            Logger.Info("{0, -35} : {1:F3} sec", "Responses elapsed", elapsedSec);
            Logger.Info("{0, -35} : {1:F2} /sec", "Responses throughput", config.NumberOfRequest * config.BrokerClient / elapsedSec);
            Logger.Info("{0, -35} : {1:F3} sec", "Request E2E elapsed", elapsedSec_req);
            Logger.Info("{0, -35} : {1:F2} /sec", "Request E2E throughput", config.NumberOfRequest * config.BrokerClient / elapsedSec_req);
            Logger.Info("Echo Done.");

        }

        private class TaskRecord
        {
            public DateTime RequestTime;
            public DateTime ResponseTime;
            public int CallDurationMS;
            public long MessageSizeByte;
        }

        static List<int> StringToIntList(string commaSplittedIntString)
        {
            List<int> intList = null;
            if (!string.IsNullOrEmpty(commaSplittedIntString))
            {
                intList = new List<int>();
                foreach (string s in commaSplittedIntString.Split(new char[] { ',' }))
                {
                    int i;
                    if (int.TryParse(s, out i))
                    {
                        intList.Add(i);
                    }
                    else
                    {
                        Logger.Warning("Failed to parse the string \"{0}\" for an int. Ignored it.", s);
                    }
                }
            }
            return intList;
        }
    }
}
