// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace TestService
{
    using System;
    using System.ServiceModel;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Telepathy.Session;

    // NOTE: If you change the interface name "IService1" here, you must also update the reference to "IService1" in App.config.
    [ServiceContract]
    public interface IService1
    {
        [OperationContract(Name = "ComputeWithInputData")]
        [FaultContract(typeof(ArgumentException))]
        [ServiceKnownType(typeof(ArgumentException))]
        [FaultContract(typeof(RetryOperationError), Action = RetryOperationError.Action)]
        ReqData GetData(int millisec, byte[] input_data, string commonData_dataClientId, long responseSize, DateTime sendStart);
    }

    public class ReqData
    {
        public string CCP_TASKINSTANCEID = string.Empty;

        public byte[] responseData = null;

        public long commonDataSize = -1;
        public DateTime commonDataAccessStartTime;
        public DateTime commonDataAccessStopTime;
        public DateTime requestStartTime;
        public DateTime requestEndTime;
        public DateTime sendStart;
    }
}
