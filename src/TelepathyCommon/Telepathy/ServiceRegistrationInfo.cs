namespace TelepathyCommon.Telepathy
{
    internal class ServiceRegistrationInfo
    {
        public static readonly ServiceRegistrationInfo Empty = new ServiceRegistrationInfo { ServiceRegistration = string.Empty, Md5 = string.Empty };

        public ServiceRegistrationInfo(string serviceRegistration)
        {
            this.ServiceRegistration = serviceRegistration;
            this.Md5 = SoaRegistrationAuxModule.CalculateMd5Hash(serviceRegistration);
        }

        private ServiceRegistrationInfo()
        {
        }

        public string Md5 { get; private set; }

        public string ServiceRegistration { get; private set; }
    }
}