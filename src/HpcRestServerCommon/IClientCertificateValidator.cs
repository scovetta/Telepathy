// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Common.Rest.Server
{
    using System.Security.Cryptography.X509Certificates;

    public interface IClientCertificateValidator
    {
        ClientCertificateValidationResult Validate(X509Certificate2 certificate);
    }
}
