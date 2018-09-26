

namespace Microsoft.Hpc.Excel
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract(Namespace = "http://hpc.microsoft.com/excel/")]
    internal class ExcelServiceError
    {
        private Exception ex;

        public Exception Cause
        {
            get { return this.ex; }
            private set { this.ex = value; }
        }

        public ExcelServiceError(Exception errorCause)
        {
            this.Cause = errorCause;
        }
    }
}
