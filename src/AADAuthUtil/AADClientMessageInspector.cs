namespace Microsoft.Hpc.AADAuthUtil
{
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    public class AADClientMessageInspector : IClientMessageInspector
    {
        private string authorization;

        public AADClientMessageInspector(string authorization)
        {
            this.authorization = authorization;
        }

        #region IClientMessageInspector Members
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
        }

        public object BeforeSendRequest(ref Message request, System.ServiceModel.IClientChannel channel)
        {
            // Prepare the request message copy to be modified
            MessageBuffer buffer = request.CreateBufferedCopy(int.MaxValue);
            request = buffer.CreateMessage();
            AADAuthMessageHeader header = new AADAuthMessageHeader(authorization);

            // Add the custom header to the request.
            request.Headers.Add(header);
            return null;
        }
        #endregion
    }
}
