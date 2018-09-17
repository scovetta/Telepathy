namespace Microsoft.Hpc
{
    using System.Security.Cryptography.X509Certificates;

    public interface IClientCertificateValidator
    {
        ClientCertificateValidationResult Validate(X509Certificate2 certificate);
    }
}
