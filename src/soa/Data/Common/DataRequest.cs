//------------------------------------------------------------------------------
// <copyright file="DataRequest.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The DataRequest class defines the format of requests from DataProxy to DataService.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This class defines the format of requests from DataProxy to DataService
    /// </summary>
    [DataContract(Namespace = "http://hpc.microsoft.com/session/data")]
    [Serializable]
    public sealed class DataRequest
    {
        /// <summary>
        /// Initializes a new instance of the DataRequest class
        /// </summary>
        public DataRequest()
        {
            this.Id = Guid.NewGuid().ToString();
            this.CreateTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the unique id of the request
        /// </summary>
        [DataMember]
        public string Id
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the time when the request is created
        /// </summary>
        [DataMember]
        public DateTime CreateTime
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets name of the queue where the response shoud go
        /// </summary>
        [DataMember]
        public string ResponseQueue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets id of the job from which the request comes from
        /// </summary>
        [DataMember]
        public int JobId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets job's secret
        /// </summary>
        [DataMember]
        public string JobSecret
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
        /// Gets or sets target DataClient id
        /// </summary>
        [DataMember]
        public string DataClientId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets additional params. All params should be serializable
        /// </summary>
        [DataMember(IsRequired = false)]
        public object[] Params
        {
            get;
            set;
        }
    }
}
