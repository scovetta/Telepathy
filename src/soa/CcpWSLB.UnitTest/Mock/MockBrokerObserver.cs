namespace Microsoft.Telepathy.ServiceBroker.UnitTest.Mock
{
    using System.Threading;

    using Microsoft.Telepathy.ServiceBroker.Common;

    internal class MockBrokerObserver : IBrokerObserver
    {
        private int requestProcessingCompletedInvokedTimes;

        public long Duration
        {
            get;
            private set;
        }

        public bool? ReplySentProperty
        {
            get;
            private set;
        }

        public int RequestProcessingCompletedInvokedTimes
        {
            get { return this.requestProcessingCompletedInvokedTimes; }
        }

        public void RequestProcessingCompleted()
        {
            Interlocked.Increment(ref this.requestProcessingCompletedInvokedTimes);
        }

        /// <summary>
        /// Informs that a reply has been sent back to the client
        /// </summary>
        /// <param name="isFault">true if the reply is a fault.</param>
        public void ReplySent(bool isFault)
        {
            this.ReplySentProperty = isFault;
        }

        /// <summary>
        /// Indicates the observer that a call is completed
        /// </summary>
        /// <param name="duration">indicating the call duration</param>
        public void CallComplete(long duration)
        {
            this.Duration = duration;
        }
    }
}
