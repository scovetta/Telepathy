namespace Microsoft.Hpc
{
    using System;

    [Serializable]
    public class AzureRmVMImageReference
    {
        public string Publisher { get; set; }

        public string Offer { get; set; }

        public string Sku { get; set; }

        public string Version { get; set; }

        public string Label { get; set; }

        public string Description { get; set; }

        public bool IsLinux { get; set; }
    }
}
