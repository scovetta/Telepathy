//------------------------------------------------------------------------------
// <copyright file="DataServiceAgent.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//       Data service agent
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.Internal
{
    using System;
    using System.Security;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.ServiceBroker;

    /// <summary>
    /// Data service agent
    /// </summary>
    internal class DataServiceAgent : ClientBase<IDataServiceAsync>, IDataService
    {
        private TransportScheme scheme;

        /// <summary>
        /// Initializes a new instance of the DataServiceAgent class
        /// </summary>
        /// <param name="headNode">cluster head node name</param>
        public DataServiceAgent(string headNode)
            : this(new Uri(SoaHelper.GetDataServiceAddress(headNode, TransportScheme.NetTcp)))
        {
            this.scheme = TransportScheme.NetTcp;
        }

        /// <summary>
        /// Initializes a new instance of the DataServiceAgent class
        /// </summary>
        /// <param name="headNode">cluster head node name</param>
        public DataServiceAgent(string headNode, TransportScheme scheme)
            : this(new Uri(SoaHelper.GetDataServiceAddress(headNode, scheme)))
        {
            this.scheme = scheme;
        }

        /// <summary>
        /// Initializes a new instance of the DataServiceAgent class
        /// </summary>
        /// <param name="uri">data service uri</param>
        public DataServiceAgent(Uri uri) :
            base(GetBinding(uri), GetEndpoint(uri))
        {
            // For common data on Azure, none secure binding is used
            if (!SoaHelper.IsOnAzure())
            {
                this.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
            }
        }

        public void SetCredential(string userName, string password)
        {
            if (this.scheme == TransportScheme.Http)
            {
                this.ClientCredentials.UserName.UserName = userName;
                this.ClientCredentials.UserName.Password = password;
            }
            else if(this.scheme == TransportScheme.NetTcp)
            {
                string domainPart;
                string namePart;
                SoaHelper.ParseDomainUser(userName, out domainPart, out namePart);
                this.ClientCredentials.Windows.ClientCredential.Domain = domainPart;
                this.ClientCredentials.Windows.ClientCredential.UserName = namePart;
                this.ClientCredentials.Windows.ClientCredential.Password = password;
            }
        }

        /// <summary>
        /// Create a DataClient with the specified data client id
        /// </summary>
        /// <param name="dataClientId">id that uniquely identifies a data client</param>
        /// <param name="allowedUsers">privileged users of the data client</param>
        /// <returns>data store path of the data client</returns>
        public string CreateDataClient(string dataClientId, string[] allowedUsers)
        {
            IAsyncResult result = this.Channel.BeginCreateDataClient(dataClientId, allowedUsers, null, null);
            return this.Channel.EndCreateDataClient(result);
        }

        /// <summary>
        /// Create a DataClient with the specified data client id
        /// </summary>
        /// <param name="dataClientId">id that uniquely identifies a data client</param>
        /// <param name="allowedUsers">privileged users of the data client</param>
        /// <param name="location">data location</param>
        /// <returns>information for accessing the data client</returns>
        public DataClientInfo CreateDataClientV4(string dataClientId, string[] allowedUsers, DataLocation location)
        {
            IAsyncResult result = this.Channel.BeginCreateDataClientV4(dataClientId, allowedUsers, location, null, null);
            return this.Channel.EndCreateDataClientV4(result);
        }

        /// <summary>
        /// Open a DataClient with the specified data client id
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <returns>information for accessing the data client</returns>
        public string OpenDataClient(string dataClientId)
        {
            IAsyncResult result = this.Channel.BeginOpenDataClient(dataClientId, null, null);
            return this.Channel.EndOpenDataClient(result);
        }

        /// <summary>
        /// Open a DataClient with the specified data client id
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <returns>information for accessing the data client</returns>
        public DataClientInfo OpenDataClientV4(string dataClientId, DataLocation location)
        {
            IAsyncResult result = this.Channel.BeginOpenDataClientV4(dataClientId, location, null, null);
            return this.Channel.EndOpenDataClientV4(result);
        }

        public DataClientInfo OpenDataClientBySecret(string dataClientId, int jobId, string jobSecret)
        {
            IAsyncResult result = this.Channel.BeginOpenDataClientBySecret(dataClientId, jobId, jobSecret, null, null);
            return this.Channel.EndOpenDataClientBySecret(result);
        }

        /// <summary>
        /// Delete a data client with the specified data client id
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        public void DeleteDataClient(string dataClientId)
        {
            IAsyncResult result = this.Channel.BeginDeleteDataClient(dataClientId, null, null);
            this.Channel.EndDeleteDataClient(result);
        }

        /// <summary>
        /// Associate lifecycle of a DataClient with lifecycle of a session
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <param name="sessionId">session id</param>
        public void AssociateDataClientWithSession(string dataClientId, int sessionId)
        {
            IAsyncResult result = this.Channel.BeginAssociateDataClientWithSession(dataClientId, sessionId, null, null);
            this.Channel.EndAssociateDataClientWithSession(result);
        }

        /// <summary>
        /// Mark a DataClient as write done
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        public void WriteDone(string dataClientId)
        {
            IAsyncResult result = this.Channel.BeginWriteDone(dataClientId, null, null);
            this.Channel.EndWriteDone(result);
        }


        /// <summary>
        /// Get data service binding
        /// </summary>
        /// <returns>data service binding</returns>
        private static Binding GetBinding(string headNode)
        {
            Binding binding;
            if (SoaHelper.IsOnAzure())
            {
                // Azure CN doesn't join domain. so secure binding cannot be used to authenticate non-system users
                // TODO: use secure binding and enhance security mechanism
                binding = BindingHelper.HardCodedUnsecureDataServiceNetTcpBinding;
                // At client side, SendTimeout is used to initialize OperationTimeout. It governs
                // the whole interaction for sending a message (including receiving a reply message).
                // ReceiveTimeout is actually not used.
                binding.SendTimeout = TimeSpan.FromMinutes(Constant.DataProxyOperationTimeoutInMinutes);
                binding.ReceiveTimeout = TimeSpan.FromMinutes(Constant.DataProxyOperationTimeoutInMinutes);
            }
            else if (SoaHelper.IsSchedulerOnIaaS(headNode))
            {
                binding = BindingHelper.HardCodedDataServiceHttpsBinding;
            }
            else
            {
                binding = BindingHelper.HardCodedDataServiceNetTcpBinding;
            }

            return binding;
        }

        /// <summary>
        /// Get data service binding
        /// </summary>
        /// <returns>data service binding</returns>
        private static Binding GetBinding(Uri uri)
        {
            Binding binding;
            if (uri.Scheme.Equals(BindingHelper.HttpsScheme, StringComparison.OrdinalIgnoreCase))
            {
                binding = BindingHelper.HardCodedDataServiceHttpsBinding;
            }
            else if (uri.Scheme.Equals(BindingHelper.NetTcpScheme))
            {
                if (SoaHelper.IsOnAzure())
                {
                    // Azure CN doesn't join domain. so secure binding cannot be used to authenticate non-system users
                    // TODO: use secure binding and enhance security mechanism
                    binding = BindingHelper.HardCodedUnsecureDataServiceNetTcpBinding;
                    // At client side, SendTimeout is used to initialize OperationTimeout. It governs
                    // the whole interaction for sending a message (including receiving a reply message).
                    // ReceiveTimeout is actually not used.
                    binding.SendTimeout = TimeSpan.FromMinutes(Constant.DataProxyOperationTimeoutInMinutes);
                    binding.ReceiveTimeout = TimeSpan.FromMinutes(Constant.DataProxyOperationTimeoutInMinutes);
                }
                else
                {
                    binding = BindingHelper.HardCodedDataServiceNetTcpBinding;
                }
            }
            else
            {
                throw new NotImplementedException(string.Format("[DataServiceAgent] Not supported Uri scheme: {0}", uri));
            }

            return binding;
        }

        /// <summary>
        /// Get endpoint address from uri
        /// </summary>
        /// <param name="uri">target service uri</param>
        /// <returns>endpoint address of target service</returns>
        private static EndpointAddress GetEndpoint(Uri uri)
        {
            EndpointIdentity identity = EndpointIdentity.CreateSpnIdentity("HOST/" + uri.Host);
            return new EndpointAddress(uri, identity);
        }
    }
}
