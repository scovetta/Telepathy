// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.EchoSvcLib
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.ServiceModel;

    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Exceptions;
    using Microsoft.Telepathy.Session.GenericService;

    /// <summary>
    /// Implementation of the echo service.
    /// </summary>
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public class EchoSvc : IEchoSvc
    {
        /// <summary>
        /// Stores the buffer size.
        /// </summary>
        private const int BufferSize = 64000;

        /// <summary>
        /// Initializes a new instance of the EchoSvc class.
        /// </summary>
        public EchoSvc()
        {
            ServiceContext.OnExiting += new EventHandler<EventArgs>(this.ServiceContext_OnExiting);
        }

        /// <summary>
        /// Echo generic request.
        /// </summary>
        /// <param name="input">service request</param>
        /// <returns>the echo response</returns>
        public GenericServiceResponse EchoGeneric(GenericServiceRequest input)
        {
            Debug.WriteLine(input.Data);
            Console.WriteLine(input.Data);
            ServiceContext.Logger.TraceInformation("EchoGeneric: Generic service request data = {0}", input.Data);

            GenericServiceResponse resp = new GenericServiceResponse();
            resp.Data = Environment.MachineName + " (GenericService): " + input.Data;
            return resp;
        }

        /// <summary>
        /// Echo the input data.
        /// </summary>
        /// <param name="input">input data</param>
        /// <returns>the echo string</returns>
        public string Echo(string input)
        {
            Debug.WriteLine(input);
            Console.WriteLine(input);
            ServiceContext.Logger.TraceInformation("Echo: Input = {0}", input);

            return Environment.MachineName + ":" + input;
        }

        /// <summary>
        /// Returns raw bytes size of the data in the specified DataClient
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <returns>the echo string</returns>
        public int EchoData(string dataClientId)
        {
#if HPCPACK
            Console.WriteLine("DataClient Id: {0}", dataClientId);
            ServiceContext.Logger.TraceInformation("EchoData: Data client Id = {0}", dataClientId);

            DataClient client = ServiceContext.GetDataClient(dataClientId);
            byte[] data = client.ReadRawBytesAll();

            ServiceContext.Logger.TraceInformation("EchoData: Data client Id = {0}, length = {1}", dataClientId, data.Length);
            return data.Length;
#endif
            // TODO: implement EchoData
            throw new NotImplementedException("Data service is not enabled in Telepathy yet.");
        }

        /// <summary>
        /// Generate load on the machine.
        /// </summary>
        /// <param name="runMilliSeconds">the run time span</param>
        /// <param name="dummyData">the dummy data</param>
        /// <param name="fileData">the file path</param>
        /// <returns>statistic info of the load</returns>
        public StatisticInfo GenerateLoad(double runMilliSeconds, byte[] dummyData, string fileData)
        {
            ServiceContext.Logger.TraceInformation("GenerateLoad: File data = {0}", fileData);

            StatisticInfo info = new StatisticInfo();
            info.StartTime = DateTime.Now;
            if (!String.IsNullOrEmpty(fileData))
            {
                byte[] buffer = new byte[BufferSize];
                int readed;
                FileStream file = File.OpenRead(fileData);

                do
                {
                    readed = file.Read(buffer, 0, BufferSize);
                }
                while (readed != BufferSize);

                file.Close();
            }

            DateTime target = DateTime.Now.AddMilliseconds(runMilliSeconds);
            int taskid;
            if (Int32.TryParse(System.Environment.GetEnvironmentVariable("CCP_TASKINSTANCEID"), out taskid))
            {
                info.TaskId = taskid;
            }

            while (DateTime.Now < target)
            {
                // busy wait;
            }

            info.EndTime = DateTime.Now;

            return info;
        }

        /// <summary>
        /// Service exit event handler.
        /// </summary>
        /// <param name="src">the source of the event</param>
        /// <param name="args">the event args</param>
        private void ServiceContext_OnExiting(object src, EventArgs args)
        {
            Console.WriteLine("OnExiting invoked!");
            ServiceContext.Logger.TraceInformation("ServiceContext_OnExiting: OnExiting invoked!");

            SOAEventArgs soaArgs = args as SOAEventArgs;
            if (soaArgs != null)
            {
                if (soaArgs.FaultCode == SOAFaultCode.Service_Preempted)
                {
                    Console.Write("This service is preempted.");
                    ServiceContext.Logger.TraceInformation("ServiceContext_OnExiting: This service is preempted.");
                }
            }
        }
    }
}
