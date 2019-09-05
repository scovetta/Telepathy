using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;

namespace TelepathyCommon.Service
{
    [Serializable]
    public class ExceptionWrapper
    {
        private readonly string message = string.Empty;
        private readonly byte[] exceptions;

        public ExceptionWrapper(Exception e)
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, e);
                    ms.Seek(0, 0);
                    exceptions = ms.GetBuffer();
                }
            }
            catch (Exception ex) when (ex is ArgumentNullException ||
                                        ex is SerializationException ||
                                        ex is SecurityException)
            {
                Trace.TraceError("Exception during serialize: {0}", ex);
                message = ex.ToString();
            }
        }

        public Exception DeserializeException()
        {
            if (!string.IsNullOrEmpty(this.message))
            {
                return new Exception(this.message);
            }

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(exceptions))
            {
                return (Exception)bf.Deserialize(ms);
            }
        }
    }
}
