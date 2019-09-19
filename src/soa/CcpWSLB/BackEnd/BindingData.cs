// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.Hpc.BrokerProxy
{
    [DataContract]
    public class BindingData
    {
        [DataContract]
        public class ReaderQuotasData
        {
            [DataMember]
            private int maxArrayLength;
            [DataMember]
            private int maxBytesPerRead;
            [DataMember]
            private int maxDepth;
            [DataMember]
            private int maxNameTableCharCount;
            [DataMember]
            private int maxStringContentLength;

            public int MaxArrayLength
            {
                get { return maxArrayLength; }
                set { maxArrayLength = value; }
            }

            public int MaxBytesPerRead
            {
                get { return maxBytesPerRead; }
                set { maxBytesPerRead = value; }
            }

            public int MaxDepth
            {
                get { return maxDepth; }
                set { maxDepth = value; }
            }

            public int MaxNameTableCharCount
            {
                get { return maxNameTableCharCount; }
                set { maxNameTableCharCount = value; }
            }

            public int MaxStringContentLength
            {
                get { return maxStringContentLength; }
                set { maxStringContentLength = value; }
            }
        }

        [DataMember]
        private long maxBufferPoolSize;
        [DataMember]
        private int maxBufferSize;
        [DataMember]
        private long maxReceivedMessageSize;
        [DataMember]
        private int maxConnections;
        [DataMember]
        private TimeSpan receiveTimeout;
        [DataMember]
        private TimeSpan sendTimeout;
        [DataMember]
        private TimeSpan openTimeout;
        [DataMember]
        private TimeSpan closeTimeout;
        [DataMember]
        private ReaderQuotasData readerQuotas = new ReaderQuotasData();

        public BindingData(Binding binding)
        {
            NetTcpBinding netTcpBinding = binding as NetTcpBinding;
            if (netTcpBinding != null)
            {
                updateBindingData(netTcpBinding);
                return;
            }

            BasicHttpBinding basicHttpBinding = binding as BasicHttpBinding;
            if (basicHttpBinding != null)
            {
                updateBindingData(basicHttpBinding);
                return;
            }

            throw new Exception(string.Format("Unsupported binding: {0}", binding.GetType().FullName));
        }

        private void updateBindingData(NetTcpBinding inputBinding)
        {
            maxBufferPoolSize = inputBinding.MaxBufferPoolSize;
            maxBufferSize = inputBinding.MaxBufferSize;
            maxReceivedMessageSize = inputBinding.MaxReceivedMessageSize;
            maxConnections = inputBinding.MaxConnections;
            receiveTimeout = inputBinding.ReceiveTimeout;
            sendTimeout = inputBinding.SendTimeout;
            openTimeout = inputBinding.OpenTimeout;
            closeTimeout = inputBinding.CloseTimeout;
            readerQuotas.MaxArrayLength = inputBinding.ReaderQuotas.MaxArrayLength;
            readerQuotas.MaxBytesPerRead = inputBinding.ReaderQuotas.MaxBytesPerRead;
            readerQuotas.MaxDepth = inputBinding.ReaderQuotas.MaxDepth;
            readerQuotas.MaxNameTableCharCount = inputBinding.ReaderQuotas.MaxNameTableCharCount;
            readerQuotas.MaxStringContentLength = inputBinding.ReaderQuotas.MaxStringContentLength;
        }

        private void updateBindingData(BasicHttpBinding inputBinding)
        {
            maxBufferPoolSize = inputBinding.MaxBufferPoolSize;
            maxBufferSize = inputBinding.MaxBufferSize;
            maxReceivedMessageSize = inputBinding.MaxReceivedMessageSize;
            receiveTimeout = inputBinding.ReceiveTimeout;
            sendTimeout = inputBinding.SendTimeout;
            openTimeout = inputBinding.OpenTimeout;
            closeTimeout = inputBinding.CloseTimeout;
            readerQuotas.MaxArrayLength = inputBinding.ReaderQuotas.MaxArrayLength;
            readerQuotas.MaxBytesPerRead = inputBinding.ReaderQuotas.MaxBytesPerRead;
            readerQuotas.MaxDepth = inputBinding.ReaderQuotas.MaxDepth;
            readerQuotas.MaxNameTableCharCount = inputBinding.ReaderQuotas.MaxNameTableCharCount;
            readerQuotas.MaxStringContentLength = inputBinding.ReaderQuotas.MaxStringContentLength;
        }

        public NetTcpBinding UpdateBinding(NetTcpBinding binding)
        {
            binding.MaxBufferPoolSize = maxBufferPoolSize;
            binding.MaxBufferSize = maxBufferSize;
            binding.MaxReceivedMessageSize = maxReceivedMessageSize;
            binding.MaxConnections = maxConnections;
            binding.ReceiveTimeout = receiveTimeout;
            binding.SendTimeout = sendTimeout;
            binding.OpenTimeout = openTimeout;
            binding.CloseTimeout = closeTimeout;
            binding.ReaderQuotas.MaxArrayLength = readerQuotas.MaxArrayLength;
            binding.ReaderQuotas.MaxBytesPerRead = readerQuotas.MaxBytesPerRead;
            binding.ReaderQuotas.MaxDepth = readerQuotas.MaxDepth;
            binding.ReaderQuotas.MaxNameTableCharCount = readerQuotas.MaxNameTableCharCount;
            binding.ReaderQuotas.MaxStringContentLength = readerQuotas.MaxStringContentLength;

            return binding;
        }

        public BasicHttpBinding UpdateBinding(BasicHttpBinding binding)
        {
            binding.MaxBufferPoolSize = maxBufferPoolSize;
            binding.MaxBufferSize = maxBufferSize;
            binding.MaxReceivedMessageSize = maxReceivedMessageSize;
            binding.ReceiveTimeout = receiveTimeout;
            binding.SendTimeout = sendTimeout;
            binding.OpenTimeout = openTimeout;
            binding.CloseTimeout = closeTimeout;
            binding.ReaderQuotas.MaxArrayLength = readerQuotas.MaxArrayLength;
            binding.ReaderQuotas.MaxBytesPerRead = readerQuotas.MaxBytesPerRead;
            binding.ReaderQuotas.MaxDepth = readerQuotas.MaxDepth;
            binding.ReaderQuotas.MaxNameTableCharCount = readerQuotas.MaxNameTableCharCount;
            binding.ReaderQuotas.MaxStringContentLength = readerQuotas.MaxStringContentLength;

            return binding;
        }

        public bool Validate()
        {
            return true;
        }

        public long MaxBufferPoolSize
        {
            get { return maxBufferPoolSize; }
            set { maxBufferPoolSize = value; }
        }

        public int MaxBufferSize
        {
            get { return maxBufferSize; }
            set { maxBufferSize = value; }
        }

        public long MaxReceivedMessageSize
        {
            get { return maxReceivedMessageSize; }
            set { maxReceivedMessageSize = value; }
        }

        public int MaxConnections
        {
            get { return maxConnections; }
            set { maxConnections = value; }
        }

        public TimeSpan ReceiveTimeout
        {
            get { return receiveTimeout; }
            set { receiveTimeout = value; }
        }

        public TimeSpan SendTimeout
        {
            get { return sendTimeout; }
            set { sendTimeout = value; }
        }

        public TimeSpan OpenTimeout
        {
            get { return openTimeout; }
            set { openTimeout = value; }
        }

        public TimeSpan CloseTimeout
        {
            get { return closeTimeout; }
            set { closeTimeout = value; }
        }

        public ReaderQuotasData ReaderQuotas
        {
            get { return readerQuotas; }
            set { readerQuotas = value; }
        }
    }
}