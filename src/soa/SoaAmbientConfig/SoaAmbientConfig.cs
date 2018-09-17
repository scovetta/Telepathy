namespace SoaAmbientConfig
{
    using System;

    public class SoaAmbientConfig
    {
        private static bool? standAlone;

        public static bool StandAlone
        {
            set
            {
                if (!standAlone.HasValue)
                {
                    standAlone = value;
                }
                else if (standAlone.Value != value)
                {
                    throw new Exception($"StandAlone has been set to {standAlone}.");
                }
            }
            get
            {
                if (!standAlone.HasValue)
                {
                    throw new Exception("Value do not be set.");
                }
                else
                {
                    return standAlone.Value;
                }
            }
        }

        public static string StorageCredential { get; set; }
    }
}
