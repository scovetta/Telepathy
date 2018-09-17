namespace Microsoft.Hpc
{
    using System.Collections.Generic;
    using System.Linq;

    public class ClientCertificateValidationResult
    {
        public bool CertificateValid { get; }

        public List<string> ValidationExceptions { get; }

        public ClientCertificateValidationResult(bool certificateValid)
        {
            this.CertificateValid = certificateValid;
            this.ValidationExceptions = new List<string>();
        }

        public void AddValidationExceptions(IEnumerable<string> exceptions)
        {
            this.ValidationExceptions.AddRange(exceptions);
        }

        public void AddValidationException(string validationException)
        {
            this.ValidationExceptions.Add(validationException);
        }
    }
}
