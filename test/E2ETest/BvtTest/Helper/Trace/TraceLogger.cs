using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Globalization;
using System.Threading;

namespace AITestLib.Helper.Trace
{
   public class TraceLogger
    {
        private static string _traceLogFile = null;
        private static StreamWriter writer = null;
        private static int writing = 0;

        // format: DateTime \t SessionId \t EventId \t Latency \t OtherArgs"
        const string logFormatString = "{0}\t{1}\t{2}\t{3}{4}";

        // time latency for each test event (ms)
        const int SessionCreatingLatency = 10000;
        const int SessionCreatedLatency = 10000;
        const int FrontEndRequestReceivedLatency = 10000;
        const int BackendRequestSentLatency = 10000;
        const int BackendResponseReceivedLatency = 10000;
        const int FrontEndResponseSentLatency = 10000;
        const int SessionFinishedLatency = 10000;
        const int SessionFinishedBecauseOfTimeoutLatency = 10000;

        private TraceLogger()
        {
        }

       public static void InitTraceLogger(string traceLogFile)
       {
           _traceLogFile = traceLogFile;
       }

       public static void CloseTraceLogger()
       {
           if (writer != null)
           {
               writer.Close();
               writer = null;
           }
       }

        #region Logger

        private static string FormatString(int sessionId, TraceTestEvents testEvent, int latency, params string[] args)
        {
            return FormatString(DateTime.Now, sessionId, testEvent, latency, args);
        }

        private static string FormatString(DateTime dataTime, int sessionId, TraceTestEvents testEvent, int latency, params string[] args)
        {
            StringBuilder sb = new StringBuilder();
            if (args != null)
            {
                foreach (string arg in args)
                {
                    sb.Append("\t" + arg);
                }
            }
            return string.Format(logFormatString, dataTime.ToString("o", CultureInfo.InvariantCulture), sessionId, (int)testEvent, latency, sb.ToString());
        }

        private static void LogInfo(string info)
        {
            while (1 == Interlocked.CompareExchange(ref writing, 1, 0))
            {
                Thread.Sleep(0);
            }
            if (writer == null && !string.IsNullOrEmpty(_traceLogFile))
            {
                if (!File.Exists(_traceLogFile))
                {
                    FileStream fs = File.Create(_traceLogFile);
                    fs.Close();
                }
                writer = File.AppendText(_traceLogFile);
                writer.AutoFlush = true;
            }
            if (writer != null)
            {
                writer.WriteLine(info);
            }
            Interlocked.Exchange(ref writing, 0);
        }

        public static void LogTestCaseStart(string testCaseName)
        {
            LogInfo("StartTest:" + testCaseName);
        }

        public static void LogSessionCreating(int sessionId)
        {
            LogInfo(FormatString(sessionId,TraceTestEvents.SessionCreating, SessionCreatingLatency));
        }

        public static void LogSessionCreated(int sessionId)
        {
            LogInfo(FormatString(sessionId, TraceTestEvents.SessionCreated, SessionCreatedLatency));
        }

        public static void LogSendRequest(int sessionId, UniqueId messageId)
        {
            Guid id;
            messageId.TryGetGuid(out id);
            LogSendRequest(sessionId, id.ToString());
        }

        public static void LogSendRequest(int sessionId, string messageId)
        {
            LogInfo(FormatString(sessionId, TraceTestEvents.FrontEndRequestReceived, FrontEndRequestReceivedLatency, messageId));
        }

        public static void LogSendRequest(int sessionId, UniqueId messageId, string clientId)
        {
            Guid id;
            messageId.TryGetGuid(out id);
            LogSendRequest(sessionId, id.ToString(), clientId);
        }

        public static void LogSendRequest(int sessionId, string messageId, string clientId)
        {
            LogInfo(FormatString(sessionId, TraceTestEvents.FrontEndRequestReceived, FrontEndRequestReceivedLatency, messageId, clientId));
        }

        public static void LogRequestDispatching(int sessionId, string messageId, DateTime dateTime)
        {
            LogInfo(FormatString(dateTime, sessionId, TraceTestEvents.BackendRequestSent, BackendRequestSentLatency, messageId));
        }

        public static void LogBrokerResponseReceived(int sessionId, string messageId, DateTime dateTime)
        {
            LogInfo(FormatString(dateTime, sessionId, TraceTestEvents.BackendResponseReceived, BackendResponseReceivedLatency, messageId));
        }

        public static void LogResponseRecived(int sessionId, string messageId)
        {
            LogInfo(FormatString(sessionId, TraceTestEvents.FrontEndResponseSent, FrontEndResponseSentLatency, messageId));
        }

        public static void LogResponseRecived(int sessionId, string messageId, string clientId)
        {
            LogInfo(FormatString(sessionId, TraceTestEvents.FrontEndResponseSent, FrontEndResponseSentLatency, messageId, clientId));
        }

        public static void LogSessionClosed(int sessionId)
        {
            LogInfo(FormatString(sessionId, TraceTestEvents.SessionFinished, SessionFinishedLatency));
        }

        public static void LogSessionFinishedBecauseOfTimeout(int sessionId)
        {
            LogInfo(FormatString(sessionId, TraceTestEvents.SessionFinishedBecauseOfTimeout, SessionFinishedBecauseOfTimeoutLatency));
        }

        #endregion
    }
}
