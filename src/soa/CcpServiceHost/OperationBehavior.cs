// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.CcpServiceHost
{
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    internal class OperationBehavior : IOperationBehavior
    {
        private CcpServiceHostWrapper hostWrapper;

        public OperationBehavior(CcpServiceHostWrapper hostWrapper)
        {
            this.hostWrapper = hostWrapper;
        }

        #region IOperationBehavior Members

        public void AddBindingParameters(OperationDescription operationDescription, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            // customize the operation invoker
            dispatchOperation.Invoker = new OperationInvokerWrapper(dispatchOperation.Invoker, this.hostWrapper);
        }

        public void Validate(OperationDescription operationDescription)
        {
        }

        #endregion
    }
}
