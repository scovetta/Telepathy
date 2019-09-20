namespace Microsoft.Hpc.ServiceBroker.UnitTest.Dispatcher
{
    using System.Reflection;

    using Microsoft.Telepathy.ServiceBroker.BackEnd;
    using Microsoft.Telepathy.ServiceBroker.BackEnd.AzureQueue;
    using Microsoft.Telepathy.ServiceBroker.BackEnd.DispatcherComponents;
    using Microsoft.Telepathy.ServiceBroker.BackEnd.nettcp;

    internal static class Utility
    {
        public static ServiceClient GetServiceClient(OnPremiseRequestSender sender)
        {
            return Utility.GetNonPublicProperty<OnPremiseRequestSender>(sender, "Client") as ServiceClient;
        }

        public static AzureServiceClient GetAzureServiceClient(AzureNettcpRequestSender sender)
        {
            return Utility.GetNonPublicProperty<AzureNettcpRequestSender>(sender, "Client") as AzureServiceClient;
        }

        public static AzureHttpsServiceClient GetAzureHttpsServiceClient(AzureHttpsRequestSender sender)
        {
            return Utility.GetNonPublicProperty<AzureHttpsRequestSender>(sender, "Client") as AzureHttpsServiceClient;
        }

        public static object GetNonPublicProperty<T>(T t, string propertyName)
        {
            PropertyInfo info = typeof(T).GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            return info.GetValue(t, null);
        }
    }
}
