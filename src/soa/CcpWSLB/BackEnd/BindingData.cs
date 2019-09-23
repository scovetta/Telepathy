// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

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
                get { return this.maxArrayLength; }
                set { this.maxArrayLength = value; }
            }

            public int MaxBytesPerRead
            {
                get { return this.maxBytesPerRead; }
                set { this.maxBytesPerRead = value; }
            }

            public int MaxDepth
            {
                get { return this.maxDepth; }
                set { this.maxDepth = value; }
            }

            public int MaxNameTableCharCount
            {
                get { return this.maxNameTableCharCount; }
                set { this.maxNameTableCharCount = value; }
            }

            public int MaxStringContentLength
            {
                get { return this.maxStringContentLength; }
                set { this.maxStringContentLength = value; }
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
                this.updateBindingData(netTcpBinding);
                return;
            }

            BasicHttpBinding basicHttpBinding = binding as BasicHttpBinding;
            if (basicHttpBinding != null)
            {
                this.updateBindingData(basicHttpBinding);
                return;
            }

            throw new Exception(string.Format("Unsupported binding: {0}", binding.GetType().FullName));
        }

        private void updateBindingData(NetTcpBinding inputBinding)
        {
            this.maxBufferPoolSize = inputBinding.MaxBufferPoolSize;
            this.maxBufferSize = inputBinding.MaxBufferSize;
            this.maxReceivedMessageSize = inputBinding.MaxReceivedMessageSize;
            this.maxConnections = inputBinding.MaxConnections;
            this.receiveTimeout = inputBinding.ReceiveTimeout;
            this.sendTimeout = inputBinding.SendTimeout;
            this.openTimeout = inputBinding.OpenTimeout;
            this.closeTimeout = inputBinding.CloseTimeout;
            this.readerQuotas.MaxArrayLength = inputBinding.ReaderQuotas.MaxArrayLength;
            this.readerQuotas.MaxBytesPerRead = inputBinding.ReaderQuotas.MaxBytesPerRead;
            this.readerQuotas.MaxDepth = inputBinding.ReaderQuotas.MaxDepth;
            this.readerQuotas.MaxNameTableCharCount = inputBinding.ReaderQuotas.MaxNameTableCharCount;
            this.readerQuotas.MaxStringContentLength = inputBinding.ReaderQuotas.MaxStringContentLength;
        }

        private void updateBindingData(BasicHttpBinding inputBinding)
        {
            this.maxBufferPoolSize = inputBinding.MaxBufferPoolSize;
            this.maxBufferSize = inputBinding.MaxBufferSize;
            this.maxReceivedMessageSize = inputBinding.MaxReceivedMessageSize;
            this.receiveTimeout = inputBinding.ReceiveTimeout;
            this.sendTimeout = inputBinding.SendTimeout;
            this.openTimeout = inputBinding.OpenTimeout;
            this.closeTimeout = inputBinding.CloseTimeout;
            this.readerQuotas.MaxArrayLength = inputBinding.ReaderQuotas.MaxArrayLength;
            this.readerQuotas.MaxBytesPerRead = inputBinding.ReaderQuotas.MaxBytesPerRead;
            this.readerQuotas.MaxDepth = inputBinding.ReaderQuotas.MaxDepth;
            this.readerQuotas.MaxNameTableCharCount = inputBinding.ReaderQuotas.MaxNameTableCharCount;
            this.readerQuotas.MaxStringContentLength = inputBinding.ReaderQuotas.MaxStringContentLength;
        }

        public NetTcpBinding UpdateBinding(NetTcpBinding binding)
        {
            binding.MaxBufferPoolSize = this.maxBufferPoolSize;
            binding.MaxBufferSize = this.maxBufferSize;
            binding.MaxReceivedMessageSize = this.maxReceivedMessageSize;
            binding.MaxConnections = this.maxConnections;
            binding.ReceiveTimeout = this.receiveTimeout;
            binding.SendTimeout = this.sendTimeout;
            binding.OpenTimeout = this.openTimeout;
            binding.CloseTimeout = this.closeTimeout;
            binding.ReaderQuotas.MaxArrayLength = this.readerQuotas.MaxArrayLength;
            binding.ReaderQuotas.MaxBytesPerRead = this.readerQuotas.MaxBytesPerRead;
            binding.ReaderQuotas.MaxDepth = this.readerQuotas.MaxDepth;
            binding.ReaderQuotas.MaxNameTableCharCount = this.readerQuotas.MaxNameTableCharCount;
            binding.ReaderQuotas.MaxStringContentLength = this.readerQuotas.MaxStringContentLength;

            return binding;
        }

        public BasicHttpBinding UpdateBinding(BasicHttpBinding binding)
        {
            binding.MaxBufferPoolSize = this.maxBufferPoolSize;
            binding.MaxBufferSize = this.maxBufferSize;
            binding.MaxReceivedMessageSize = this.maxReceivedMessageSize;
            binding.ReceiveTimeout = this.receiveTimeout;
            binding.SendTimeout = this.sendTimeout;
            binding.OpenTimeout = this.openTimeout;
            binding.CloseTimeout = this.closeTimeout;
            binding.ReaderQuotas.MaxArrayLength = this.readerQuotas.MaxArrayLength;
            binding.ReaderQuotas.MaxBytesPerRead = this.readerQuotas.MaxBytesPerRead;
            binding.ReaderQuotas.MaxDepth = this.readerQuotas.MaxDepth;
            binding.ReaderQuotas.MaxNameTableCharCount = this.readerQuotas.MaxNameTableCharCount;
            binding.ReaderQuotas.MaxStringContentLength = this.readerQuotas.MaxStringContentLength;

            return binding;
        }

        public bool Validate()
        {
            return true;
        }

        public long MaxBufferPoolSize
        {
            get { return this.maxBufferPoolSize; }
            set { this.maxBufferPoolSize = value; }
        }

        public int MaxBufferSize
        {
            get { return this.maxBufferSize; }
            set { this.maxBufferSize = value; }
        }

        public long MaxReceivedMessageSize
        {
            get { return this.maxReceivedMessageSize; }
            set { this.maxReceivedMessageSize = value; }
        }

        public int MaxConnections
        {
            get { return this.maxConnections; }
            set { this.maxConnections = value; }
        }

        public TimeSpan ReceiveTimeout
        {
            get { return this.receiveTimeout; }
            set { this.receiveTimeout = value; }
        }

        public TimeSpan SendTimeout
        {
            get { return this.sendTimeout; }
            set { this.sendTimeout = value; }
        }

        public TimeSpan OpenTimeout
        {
            get { return this.openTimeout; }
            set { this.openTimeout = value; }
        }

        public TimeSpan CloseTimeout
        {
            get { return this.closeTimeout; }
            set { this.closeTimeout = value; }
        }

        public ReaderQuotasData ReaderQuotas
        {
            get { return this.readerQuotas; }
            set { this.readerQuotas = value; }
        }
    }
}