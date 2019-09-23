// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace TestClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Telepathy.Session;

    internal class StatisticData
    {
        private List<KeyValuePair<string, object>> outputLogs = new List<KeyValuePair<string, object>>();

        internal List<KeyValuePair<string, object>> OutputLogs
        {
            get { return outputLogs; }
        }

        private int count;
        internal int Count
        {
            get { return count; }
            set { count = value; }
        }

        private int faultCount;
        internal int FaultCount
        {
            get { return faultCount; }
            set { faultCount = value; }
        }


        private int client = 1;
        internal int Client
        {
            get { return client; }
            set { client = value; }
        }


        private int milliseconds;
        internal int Milliseconds
        {
            get { return milliseconds; }
            set { milliseconds = value; }
        }


        private long inputDataSize;
        internal long InputDataSize
        {
            get { return inputDataSize; }
            set { inputDataSize = value; }
        }


        private long commonDataSize;
        internal long CommonDataSize
        {
            get { return commonDataSize; }
            set { commonDataSize = value; }
        }

        private long outputDataSize;
        internal long OutputDataSize
        {
            get { return outputDataSize; }
            set { outputDataSize = value; }
        }


        private string command = "TestClient.exe";
        internal string Command
        {
            get { return command; }
            set { command = value; }
        }


        private SessionStartInfo startInfo;
        internal SessionStartInfo StartInfo
        {
            get { return startInfo; }
            set { startInfo = value; }
        }


        private DateTime sessionStart = DateTime.MinValue;
        internal DateTime SessionStart
        {
            get { return sessionStart; }
            set { sessionStart = value; }
        }

        private DateTime sessionCreated = DateTime.MinValue;
        internal DateTime SessionCreated
        {
            get { return sessionCreated; }
            set { sessionCreated = value; }
        }


        private DateTime sessionEnd = DateTime.MinValue;
        internal DateTime SessionEnd
        {
            get { return sessionEnd; }
            set { sessionEnd = value; }
        }


        private DateTime sendStart = DateTime.MaxValue;
        internal DateTime SendStart
        {
            get { return sendStart; }
            set { sendStart = value; }
        }


        private DateTime reqEom = DateTime.MinValue;
        internal DateTime ReqEom
        {
            get { return reqEom; }
            set { reqEom = value; }
        }

        private DateTime reqEomDone = DateTime.MinValue;
        internal DateTime ReqEomDone
        {
            get { return reqEomDone; }
            set { reqEomDone = value; }
        }

        private DateTime firstRequest = DateTime.MaxValue;
        public DateTime FirstRequest
        {
            get { return firstRequest; }
            set { firstRequest = value; }
        }

        private DateTime retrieveEnd = DateTime.MinValue;

        internal DateTime RetrieveEnd
        {
            get { return retrieveEnd; }
            set { retrieveEnd = value; }
        }

        private DateTime closeSessionStart = DateTime.MinValue;

        public DateTime CloseSessionStart
        {
            get { return closeSessionStart; }
            set { closeSessionStart = value; }
        }      

        private List<ResultData> resultCollection = new List<ResultData>();

        internal List<ResultData> ResultCollection
        {
            get { return resultCollection; }
            set { resultCollection = value; }
        }

        private int used_cores;

        public int Used_cores
        {
            get { return used_cores; }
            set { used_cores = value; }
        }

        private double totalUsedTime = 0;

        public double TotalUsedTime
        {
            get { return totalUsedTime; }
            set { totalUsedTime = value; }
        }

        private DateTime endRequest = DateTime.MinValue;

        public DateTime EndRequest
        {
            get { return endRequest; }
            set { endRequest = value; }
        }

        private DateTime firstResponseTime = DateTime.MinValue;

        public DateTime FirstResponseTime
        {
            get { return firstResponseTime; }
            set { firstResponseTime = value; }
        }

        private List<int> taskIds = new List<int>();

        public List<int> TaskIds
        {
            get { return taskIds; }
            set { taskIds = value; }
        }

        #region commonDataStatics
        private long commonDataReadTime = 0;

        public long CommonDataReadTime
        {
            get { return commonDataReadTime; }
            set { commonDataReadTime = value; }
        }

        private DateTime createDataClientStart = DateTime.MinValue;

        public DateTime CreateDataClientStart
        {
            get { return createDataClientStart; }
            set { createDataClientStart = value; }
        }

        private DateTime createDataClientEnd = DateTime.MinValue;

        public DateTime CreateDataClientEnd
        {
            get { return createDataClientEnd; }
            set { createDataClientEnd = value; }
        }

        private DateTime writeDataClientStart = DateTime.MinValue;

        public DateTime WriteDataClientStart
        {
            get { return writeDataClientStart; }
            set { writeDataClientStart = value; }
        }

        private DateTime writeDataClientEnd = DateTime.MinValue;

        public DateTime WriteDataClientEnd
        {
            get { return writeDataClientEnd; }
            set { writeDataClientEnd = value; }
        }

        private bool onDemand = false;

        public bool OnDemand
        {
            get { return onDemand; }
            set { onDemand = value; }
        } 

        #endregion

        public void ProcessData()
        {
            double commonDataTotalElapsedMilliSec = 0;
            int validCommonDataReadTime = 0;
            foreach (ResultData resultData in resultCollection.OrderBy(ResultData => ResultData.Start))
            {
                if (firstRequest == DateTime.MaxValue) firstRequest = resultData.Start;
                totalUsedTime += (int)(resultData.End.Subtract(resultData.Start).TotalMilliseconds);
                if (resultData.End > endRequest)
                {
                    endRequest = resultData.End;
                }
                if (!taskIds.Contains(resultData.TaskId)) taskIds.Add(resultData.TaskId);
                commonDataTotalElapsedMilliSec += (resultData.DataAccessStop.Subtract(resultData.DataAccessStart).TotalMilliseconds);
                if (!resultData.DataAccessStart.Equals(resultData.DataAccessStop)) validCommonDataReadTime++;
            }
            foreach (ResultData resultData in resultCollection.OrderBy(ResultData => ResultData.End))
            {
                firstResponseTime = resultData.End;
                break;
            }
            //hack: sometimes req_end on client side is earlier than the last request end time on a distributed system
            if (endRequest > retrieveEnd)
            {
                retrieveEnd = endRequest;
            }
            used_cores = taskIds.Count;
            efficiencyTotal = totalUsedTime / (sessionEnd.Subtract(sessionStart).TotalMilliseconds * (int)startInfo.MaximumUnits);
            efficiencyExcludeSessionCreation = totalUsedTime / (sessionEnd.Subtract(SessionCreated).TotalMilliseconds * (int)startInfo.MaximumUnits);
            efficiencyStartFromFirstRequest = totalUsedTime / (sessionEnd.Subtract(firstRequest).TotalMilliseconds * (int)startInfo.MaximumUnits);
            if (efficiencyStartFromFirstRequest > 1) efficiencyStartFromFirstRequest = 1;
            efficiencyFromFirstRequestExcludeSessionEnd =
                totalUsedTime / (closeSessionStart.Subtract(firstRequest).TotalMilliseconds * (int)startInfo.MaximumUnits);
            if (efficiencyFromFirstRequestExcludeSessionEnd > 1) efficiencyFromFirstRequestExcludeSessionEnd = 1;
            if (endSend > DateTime.MinValue) sendThroughput = (double)count * 1000 / endSend.Subtract(sendStart).TotalMilliseconds;
            else if (reqEomDone > DateTime.MinValue) sendThroughput = (double)count * 1000 / reqEomDone.Subtract(sendStart).TotalMilliseconds;
            throughtputDuration = (double)count * 1000 / endRequest.Subtract(firstRequest).TotalMilliseconds;
            overallThroughput = (double)count * 1000 / retrieveEnd.Subtract(sendStart).TotalMilliseconds;
            calculationElapsedTime = retrieveEnd.Subtract(sendStart).TotalMilliseconds;
            responseTime = calculationElapsedTime / count;

            #region process common data statics
            if (validCommonDataReadTime > 0) commonDataReadTime = (long)(commonDataTotalElapsedMilliSec / validCommonDataReadTime);
            #endregion

            PrepareOutput();
        }

        private void PrepareOutput()
        {
            outputLogs.Add(new KeyValuePair<string, object>("SessionId", this.sessionId));
            outputLogs.Add(new KeyValuePair<string, object>("IsDurable", this.isDurable));
            outputLogs.Add(new KeyValuePair<string, object>("MinUnit", this.startInfo.MinimumUnits.HasValue ? this.startInfo.MinimumUnits.ToString() : string.Empty));
            outputLogs.Add(new KeyValuePair<string, object>("MaxUnit", this.startInfo.MaximumUnits.HasValue ? this.startInfo.MaximumUnits.ToString() : string.Empty));
            outputLogs.Add(new KeyValuePair<string, object>("BatchSize", this.client));
            outputLogs.Add(new KeyValuePair<string, object>("ReqCount", this.count));
            outputLogs.Add(new KeyValuePair<string, object>("ReqTime(millisec)", this.milliseconds));
            outputLogs.Add(new KeyValuePair<string, object>("ReqSize(byte)", sizeof(int) + this.inputDataSize));
            outputLogs.Add(new KeyValuePair<string, object>("UsedCores", this.used_cores));
            outputLogs.Add(new KeyValuePair<string, object>("SessionCreationTime(millisec)", this.sessionCreated.Subtract(this.sessionStart).TotalMilliseconds));
            outputLogs.Add(new KeyValuePair<string, object>("SessionCloseTime(millisec)", this.sessionEnd.Subtract(this.closeSessionStart).TotalMilliseconds));
            outputLogs.Add(new KeyValuePair<string, object>("FirstResponseTime(millisec)", this.firstResponseTime.Subtract(this.sessionStart).TotalMilliseconds));
            outputLogs.Add(new KeyValuePair<string, object>("SendThroughput(msg/sec)", this.sendThroughput));
            outputLogs.Add(new KeyValuePair<string, object>("BrokerThroughputDuration(msg/sec)", this.throughtputDuration));
            outputLogs.Add(new KeyValuePair<string, object>("OverallThroughput(msg/sec)", this.overallThroughput));
            outputLogs.Add(new KeyValuePair<string, object>("CalculationElapsedTime(millisec)", this.calculationElapsedTime));
            outputLogs.Add(new KeyValuePair<string, object>("ResponseTime(millisec)", this.responseTime));
            outputLogs.Add(new KeyValuePair<string, object>("EfficiencyTotal", this.efficiencyTotal));
            outputLogs.Add(new KeyValuePair<string, object>("EfficiencyExcludeSessionCreation", this.efficiencyExcludeSessionCreation));
            outputLogs.Add(new KeyValuePair<string, object>("EfficiencyAfterFirstReqServed", this.efficiencyStartFromFirstRequest));
            outputLogs.Add(new KeyValuePair<string, object>("EfficiencyAfterFirstReqServedExcludeSessionEnd", this.efficiencyFromFirstRequestExcludeSessionEnd));
            outputLogs.Add(new KeyValuePair<string, object>("CommonDataSize(MB)", this.commonDataSize / (1024 * 1024)));            
            outputLogs.Add(new KeyValuePair<string, object>("CommonDataReadTime(millisec)", this.commonDataReadTime));
            outputLogs.Add(new KeyValuePair<string, object>("CommonDataCreateTime(millisec)", this.createDataClientEnd.Subtract(this.createDataClientStart).TotalMilliseconds));
            outputLogs.Add(new KeyValuePair<string, object>("CommonDataWriteTime(millisec)", this.writeDataClientEnd.Subtract(this.writeDataClientStart).TotalMilliseconds));
            outputLogs.Add(new KeyValuePair<string, object>("CommonDataOnDemand", this.onDemand));
        }

        private double efficiencyTotal;

        public double EfficiencyTotal
        {
            get { return efficiencyTotal; }
            set { efficiencyTotal = value; }
        }

        private double efficiencyExcludeSessionCreation;

        public double EfficiencyExcludeSessionCreation
        {
            get { return efficiencyExcludeSessionCreation; }
            set { efficiencyExcludeSessionCreation = value; }
        }

        private double efficiencyStartFromFirstRequest;

        public double EfficiencyStartFromFirstRequest
        {
            get { return efficiencyStartFromFirstRequest; }
            set { efficiencyStartFromFirstRequest = value; }
        }

        private double efficiencyFromFirstRequestExcludeSessionEnd;

        public double EfficiencyFromFirstRequestExcludeSessionEnd
        {
            get { return efficiencyFromFirstRequestExcludeSessionEnd; }
            set { efficiencyFromFirstRequestExcludeSessionEnd = value; }
        }

        private string sessionId;

        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }

        private double sendThroughput = 0;

        public double SendThroughput
        {
            get { return sendThroughput; }
            set { sendThroughput = value; }
        }

        private double throughtputDuration = 0;

        public double ThroughtputDuration
        {
            get { return throughtputDuration; }
            set { throughtputDuration = value; }
        }

        private double overallThroughput = 0;

        public double OverallThroughput
        {
            get { return overallThroughput; }
            set { overallThroughput = value; }
        }

        private double responseTime = 0;

        public double ResponseTime
        {
            get { return responseTime; }
            set { responseTime = value; }
        }

        private double calculationElapsedTime = 0;

        public double CalculationElapsedTime
        {
            get { return calculationElapsedTime; }
            set { calculationElapsedTime = value; }
        }
        
        private DateTime endSend = DateTime.MinValue;

        public DateTime SendEnd
        {
            get { return endSend; }
            set { endSend = value; }
        }

        private bool isDurable = false;

        public bool IsDurable
        {
            get { return isDurable; }
            set { isDurable = value; }
        }
    }


}
