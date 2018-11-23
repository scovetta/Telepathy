using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.ServiceModel;
using Microsoft.Hpc.Scheduler.Session.Data;
using Microsoft.Hpc.Scheduler.Session;
using System.Diagnostics;
using System.Text;

namespace TestService
{
    // NOTE: If you change the class name "Service1" here, you must also update the reference to "Service1" in App.config.
    public class Service1 : IService1
    {
        static byte[] data = null;
        static object syncObj = new object();

        public ReqData GetData(int millisec, byte[] input_data, string commonData_dataClientId, long responseSize, DateTime sendStart)
        {
            DateTime start = DateTime.Now;
            ReqData rtn = new ReqData();

            if (!string.IsNullOrEmpty(commonData_dataClientId))
            {
                try
                {
                    if (data == null)
                    {
                        lock (syncObj)
                        {
                            if (data == null)
                            {
                                DateTime openStart = DateTime.UtcNow;
                                rtn.commonDataAccessStartTime = openStart;
                                Console.WriteLine("{0}: Start open", openStart.ToLongTimeString());
                                using (DataClient dataClient = ServiceContext.GetDataClient(commonData_dataClientId))
                                {
                                    DateTime openEnd = DateTime.UtcNow;
                                    Console.WriteLine("{0}: Open finished", openEnd.ToLongTimeString());
                                    Console.WriteLine("{0}: Open time: {1} millisec.", DateTime.UtcNow.ToLongTimeString(), openEnd.Subtract(openStart).TotalMilliseconds);

                                    DateTime readStart = DateTime.UtcNow;
                                    Console.WriteLine("{0}: Start read", openStart.ToLongTimeString());
                                    data = dataClient.ReadRawBytesAll();
                                    DateTime readEnd = DateTime.UtcNow;
                                    Console.WriteLine("{0}: Read finshed", readEnd.ToLongTimeString());
                                    Console.WriteLine("{0}: Read time: {1} millisec.", DateTime.UtcNow.ToLongTimeString(), readEnd.Subtract(readStart).TotalMilliseconds);

                                    rtn.commonDataAccessStopTime = readEnd;
                                    Console.WriteLine("{0}: Common data access time: {1} millisec.", DateTime.UtcNow.ToLongTimeString(), rtn.commonDataAccessStopTime.Subtract(rtn.commonDataAccessStartTime).TotalMilliseconds);
                                }
                            }
                        }
                    }
                    if (data != null)
                    {
                        rtn.commonDataSize = data.Length;
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("dataclient exception: {0}", e.ToString());
                }
            }

            int userTraceCount = 0;
            int.TryParse(Environment.GetEnvironmentVariable("UserTraceCount"), out userTraceCount);
            StringBuilder usertracedata = new StringBuilder();
            // Per PM, we test each user trace with 64 chars
            for (int i = 0; i < 64; i++)
            {
                usertracedata.Append('0');
            }
            for (int i = 0; i < userTraceCount; i++)
            {
                ServiceContext.Logger.TraceInformation(usertracedata.ToString());
            }
            
            Thread.Sleep(millisec);

            bool throwRetryOperationError = false;
            bool.TryParse(Environment.GetEnvironmentVariable("RetryRequest"), out throwRetryOperationError);
            if (throwRetryOperationError)
            {
                throw new FaultException<RetryOperationError>(new RetryOperationError("RetryRequest"));
            }

            DateTime end = DateTime.Now;

            rtn.CCP_TASKINSTANCEID = System.Environment.GetEnvironmentVariable("CCP_TASKINSTANCEID");
            if (responseSize < 0) throw new FaultException<ArgumentException>(new ArgumentException("responseSize should not be less than zero."));
            else if (responseSize > 0)
            {
                rtn.responseData = new byte[responseSize];
                (new Random()).NextBytes(rtn.responseData);
            }
            rtn.requestStartTime = start;
            rtn.requestEndTime = end;
            rtn.sendStart = sendStart;
            return rtn;
        }


    }
}
