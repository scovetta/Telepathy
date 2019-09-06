namespace TelepathyCommon.Service
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;

    [Serializable]
    public class ExceptionWrapper
    {
        private readonly byte[] exceptions;

        private readonly string message = string.Empty;

        public ExceptionWrapper(Exception e)
        {
            try
            {
                var bf = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    bf.Serialize(ms, e);
                    ms.Seek(0, 0);
                    this.exceptions = ms.GetBuffer();
                }
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is SerializationException || ex is SecurityException)
            {
                Trace.TraceError("Exception during serialize: {0}", ex);
                this.message = ex.ToString();
            }
        }

        public Exception DeserializeException()
        {
            if (!string.IsNullOrEmpty(this.message))
            {
                return new Exception(this.message);
            }

            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream(this.exceptions))
            {
                return (Exception)bf.Deserialize(ms);
            }
        }
    }
}