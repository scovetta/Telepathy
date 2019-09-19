// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.CcpServiceHosting
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;

    class OperationInvokerWrapper : IOperationInvoker
    {
        private IOperationInvoker innerInvoker;

        private CcpServiceHostWrapper hostWrapper;

        public OperationInvokerWrapper(IOperationInvoker invoker, CcpServiceHostWrapper hostWrapper)
        {
            this.innerInvoker = invoker;
            this.hostWrapper = hostWrapper;
        }

        #region IOperationInvoker Members

        public object[] AllocateInputs()
        {
            return this.innerInvoker.AllocateInputs();
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            Guid messageId;
            bool getGuid = OperationContext.Current.IncomingMessageHeaders.MessageId.TryGetGuid(out messageId);
            Debug.Assert(getGuid, "OperationInvokerWrapper fails to get message id in Guid type.");

            // After receiving cancel event, skip invoking the hosted service.
            // The message inspector will reply a fault message to the broker.
            if (this.hostWrapper.ReceivedCancelEvent)
            {
                this.hostWrapper.SkippedMessageIds.Add(messageId);
                outputs = new object[0];
                return null;
            }
            else
            {
                this.hostWrapper.ProcessingMessageIds.Add(messageId);
                return this.innerInvoker.Invoke(instance, inputs, out outputs);
            }
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            // this method is not called because IsSynchronous is true
            return this.innerInvoker.InvokeBegin(instance, inputs, callback, state);
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            // this method is not called because IsSynchronous is true
            return this.innerInvoker.InvokeEnd(instance, out outputs, result);
        }

        /// <summary>
        /// The default invoker dispatches messages to the synchronous operation by default.
        /// This value is cached by the dispatcher and therefore should not change over the
        /// lifetime of the object that implements IOperationInvoker.
        /// </summary>
        public bool IsSynchronous
        {
            get
            {
                return this.innerInvoker.IsSynchronous;
            }
        }

        #endregion
    }
}
