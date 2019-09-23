// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.CcpServiceHost
{
    using System.Security.Principal;
    using System.ServiceModel;

    using Microsoft.Telepathy.Session.Common;

    /// <summary>
    /// An authorization manager which only allows access from the a specified user
    /// </summary>
    class UserBasedAuthManager : ServiceAuthorizationManager
    {
        SecurityIdentifier allowedUser;

        internal UserBasedAuthManager()
            : this(WindowsIdentity.GetCurrent().User)
        {
        }

        internal UserBasedAuthManager(SecurityIdentifier allowedUser)
        {
            this.allowedUser = allowedUser;
        }

        protected override bool CheckAccessCore(OperationContext operationContext)
        {
            if (SoaHelper.IsOnAzure())
            {
                // Skip it in the Azure cluster.
                return true;
            }

            return ServiceSecurityContext.Current.WindowsIdentity.User == this.allowedUser;
        }
    }
}