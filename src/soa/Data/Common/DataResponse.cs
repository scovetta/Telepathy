//------------------------------------------------------------------------------
// <copyright file="DataResponse.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      DataResponse class defines the data contract from DataService to DataProxy
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    /// <summary>
    /// This class defines the format of responses from DataService to DataProxy
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://hpc.microsoft.com/session/data")]
    [KnownType(typeof(DataFault))]
    public sealed class DataResponse
    {
        /// <summary>
        /// Initializes a new instance of the DataResponse class
        /// </summary>
        public DataResponse()
        {
        }

        /// <summary>
        /// Initializes a new instance of the DataResponse class
        /// </summary>
        /// <param name="request">DataRequest instance to which this DataResponse instance corresponds to</param>
        public DataResponse(DataRequest request)
        {
            this.RequestId = request.Id;
            this.Type = request.Type;
        }

        /// <summary>
        /// Gets or sets id of the corresponding request
        /// </summary>
        [DataMember]
        public string RequestId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the request type
        /// </summary>
        [DataMember]
        public DataRequestType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the error code
        /// </summary>
        [DataMember]
        public int ErrorCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the fault exception
        /// </summary>
        [DataMember(IsRequired = false)]
        public FaultException<DataFault> Exception
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets response results. All results should be serializable
        /// </summary>
        [DataMember(IsRequired = false)]
        public object[] Results
        {
            get;
            set;
        }
    }
}
