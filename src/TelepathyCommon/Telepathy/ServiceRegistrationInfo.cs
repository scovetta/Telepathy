namespace Microsoft.Hpc
{
    internal class ServiceRegistrationInfo
    {
        public static readonly ServiceRegistrationInfo Empty = new ServiceRegistrationInfo { ServiceRegistration = string.Empty, Md5 = string.Empty };

        private ServiceRegistrationInfo()
        {
        }

        public ServiceRegistrationInfo(string serviceRegistration)
        {
            this.ServiceRegistration = serviceRegistration;
            this.Md5 = SoaRegistrationAuxModule.CalculateMd5Hash(serviceRegistration);
        }

        public string ServiceRegistration { get; private set; }

        public string Md5 { get; private set; }
    }
}
