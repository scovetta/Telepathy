// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

#if HPCPACK
namespace Microsoft.Hpc
{
    using System;
    using System.Diagnostics;
    using System.DirectoryServices.ActiveDirectory;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading;

    using Microsoft.Hpc.Scheduler.Properties;

    /// <summary>
    /// Authorization providers allow the business logic to discover the roles, if any, the
    /// given identity is granted.
    /// 
    /// HPC Authorization Providers should be sealed to deter injection/spoofing.
    /// </summary>
    internal interface IHpcAuthorizationProvider
    {
        /// <summary>
        /// Returns a mask that includes all roles for which the current
        /// thread's principal qualifies.
        /// </summary>
        /// <returns></returns>
        UserRoles GetUserRoles();

        /// <summary>
        /// Returns a mask that includes all roles for which the given IPrincipal
        /// qualifies.
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        UserRoles GetUserRoles(IPrincipal principal);

        /// <summary>
        /// Returns true if the component should accept calls from the given
        /// identity.
        /// </summary>
        /// <param name="principao"></param>
        /// <returns></returns>
        bool AcceptApiCallsFromThisIdentity(IPrincipal principal);
    }

    /// <summary>
    /// Utilities for authentication.
    /// </summary>
    internal static class AuthenticationUtil
    {
        private static IHpcAuthorizationProvider _authProvider = new HpcAuthorizationProviderForWindowsIdentiy();

        #region // names of security groups
        /// <summary>
        /// Name of hpc admin group
        /// </summary>
        internal const string HpcAdminGroupName = "HpcAdminMirror";

        /// <summary>
        /// Name of hpc user group
        /// </summary>
        internal const string HpcUserGroupName = "HpcUsers";

        /// <summary>
        /// Name of hpc job administrators group
        /// </summary>
        internal const string HpcJobAdministratorsGroupName = "HpcJobAdministrators";

        /// <summary>
        /// Name of hpc job operators group
        /// </summary>
        internal const string HpcJobOperatorsGroupName = "HpcJobOperators";

        #endregion // names of security groups


        #region // accessors for group SIDs

        /// <summary>
        /// Sid of hpc admin group
        /// </summary>
        private static SecurityIdentifier _hpcAdminMirrorSid = GetGroupSid(HpcAdminGroupName);

        internal static SecurityIdentifier HpcAdminMirrorSid { get { return _hpcAdminMirrorSid; } }

        /// <summary>
        /// Sid of hpc user group
        /// </summary>
        private static SecurityIdentifier _hpcUsersSid = GetGroupSid(HpcUserGroupName);

        internal static SecurityIdentifier HpcUsersSid { get { return _hpcUsersSid; } }

        /// <summary>
        /// Sid of hpc job administrators group
        /// </summary>
        private static SecurityIdentifier _hpcJobAdministratorSid = GetGroupSid(HpcJobAdministratorsGroupName);

        internal static SecurityIdentifier HpcJobAdministratorSid { get { return _hpcJobAdministratorSid; } }

        /// <summary>
        /// Sid of hpc job operators group
        /// </summary>
        private static SecurityIdentifier _hpcJobOperatorsSid = GetGroupSid(HpcJobOperatorsGroupName);

        internal static SecurityIdentifier HpcJobOperatorsSid { get { return _hpcJobOperatorsSid; } }


        #endregion // accessors for group SIDs

        //BUGBUG: for v4sp1 consumers of this routine should be updated to use multi-roles
        public static bool IsHpcAdminOrUser(WindowsIdentity identity)
        {
            Debug.Assert(identity != null, "identity");

            WindowsPrincipal principal = new WindowsPrincipal(identity);

            // if a user account is in Administrator group, it must also be an hpc admin
            return principal.IsInRole(WindowsBuiltInRole.Administrator) || principal.IsInRole(HpcAdminMirrorSid) || principal.IsInRole(HpcUsersSid);
        }

        /// <summary>
        /// Check if a user is a member of HpcAdmin group
        /// </summary>
        /// <param name="identity">user identity</param>
        /// <returns>true if user is a member of HpcAdminMirror group, false otherwise</returns>
        /// 

        // TODO: integrate with Roles code... and fixup comment (admin = administrators + adminmirror etc)
        public static bool IsHpcAdmin(WindowsIdentity identity)
        {
            Debug.Assert(identity != null, "identity");

            WindowsPrincipal principal = new WindowsPrincipal(identity);

            // if a user account is in Administrator group, it must also be an hpc amdin
            return principal.IsInRole(WindowsBuiltInRole.Administrator) || principal.IsInRole(HpcAdminMirrorSid);
        }

        #region IHpcAuthorizationProvider

        internal static UserRoles GetUserRoles()
        {
            UserRoles roles = _authProvider.GetUserRoles();

            return roles;
        }

        internal static UserRoles GetUserRoles(IPrincipal principal)
        {
            UserRoles roles = _authProvider.GetUserRoles(principal);

            return roles;
        }

        internal static bool AcceptApiCallsFromThisIdentity(IPrincipal principal)
        {
            bool acceptApiCalls = _authProvider.AcceptApiCallsFromThisIdentity(principal);

            return acceptApiCalls;
        }

        /// <summary>
        /// Allows components to override the default auth provider to suit their
        /// own needs.
        /// </summary>
        internal static IHpcAuthorizationProvider CurrentAuthorizationProvider
        {
            get { return _authProvider; }
            set { _authProvider = value; }
        }

        #endregion IHpcAuthorizationProvider

        /// <summary>
        /// Check if current machine is domain controller
        /// </summary>
        /// <returns>true if current machine is domain controller, false otherwise</returns>
        internal static bool IsDomainController()
        {
            CredentialNativeMethods.OSVERSIONINFOEX osVersion = new CredentialNativeMethods.OSVERSIONINFOEX();
            osVersion.dwOSVersionInfoSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(osVersion);

            if (!CredentialNativeMethods.GetVersionEx(ref osVersion))
            {
                return false;
            }

            if (osVersion.wProductType == CredentialNativeMethods.VER_NT_DOMAIN_CONTROLLER)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Obtain the SID for the local group or domain local group (if Domain Controler)
        /// </summary>
        /// <param name="groupName">group name</param>
        /// <returns>security identifier of the group</returns>
        internal static SecurityIdentifier GetGroupSid(string groupName)
        {
            NTAccount identity;
            if (!IsDomainController())
            {
                identity = new NTAccount(Environment.MachineName, groupName);
            }
            else
            {
                identity = new NTAccount(Domain.GetComputerDomain().Name, groupName);
            }

            try
            {
                return identity.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
            }
            catch (System.Security.Principal.IdentityNotMappedException)
            {
                if (!DomainUtil.IsInDomain())
                {
                    return DomainUtil.LookupAccountName(Environment.MachineName, groupName);
                }

                // Unable to find a local group.
                // this might happen in contexts like WAHS where not all groups are used/created by setup (delegated admin, etc).
                return null;
            }
        }
    }

    /// <summary>
    /// This provider implements the authorization of roles based on Active Directory, 
    /// WindowsIdentity and membership in local nt security groups.
    /// </summary>
    internal sealed class HpcAuthorizationProviderForWindowsIdentiy : IHpcAuthorizationProvider
    {
        public UserRoles GetUserRoles()
        {
            IPrincipal principal = Thread.CurrentPrincipal;
            UserRoles roles = GetUserRoles(principal);

            return roles;
        }

        public UserRoles GetUserRoles(IPrincipal principal)
        {
            UserRoles roles = UserRoles.AccessDenied;

            if (principal.Identity is WindowsIdentity)
            {
                WindowsPrincipal winPrincipal = new WindowsPrincipal(principal.Identity as WindowsIdentity);

                // if a user account is in Administrator group, it must also be an hpc admin
                if (winPrincipal.IsInRole(WindowsBuiltInRole.Administrator) || winPrincipal.IsInRole(AuthenticationUtil.HpcAdminMirrorSid))
                {
                    roles |= UserRoles.Administrator;
                }

                if (winPrincipal.IsInRole(AuthenticationUtil.HpcJobAdministratorSid))
                {
                    roles |= UserRoles.JobAdministrator;
                }

                if (winPrincipal.IsInRole(AuthenticationUtil.HpcJobOperatorsSid))
                {
                    roles |= UserRoles.JobOperator;
                }

                // HpcUsers are users and PowerUsers are users...
                if (winPrincipal.IsInRole(AuthenticationUtil.HpcUsersSid) || winPrincipal.IsInRole(WindowsBuiltInRole.PowerUser))
                {
                    roles |= UserRoles.User;
                }
            }
            else if (principal.IsHpcAadPrincipal())
            {
                ClaimsPrincipal claimPrincipal = (ClaimsPrincipal)principal;
                if (claimPrincipal.IsInRole(AuthenticationUtil.HpcAdminGroupName))
                {
                    roles |= UserRoles.Administrator;
                }

                if (claimPrincipal.IsInRole(AuthenticationUtil.HpcUserGroupName))
                {
                    roles |= UserRoles.User;
                }

                if (claimPrincipal.IsInRole(AuthenticationUtil.HpcJobAdministratorsGroupName))
                {
                    roles |= UserRoles.JobAdministrator;
                }

                if (claimPrincipal.IsInRole(AuthenticationUtil.HpcJobOperatorsGroupName))
                {
                    roles |= UserRoles.JobOperator;
                }
            }
            else if(WcfChannelModule.IsX509Identity(principal.Identity))
            {
                roles |= UserRoles.Administrator;
            }

            return roles;
        }

        public bool AcceptApiCallsFromThisIdentity(IPrincipal principal)
        {
            UserRoles roles = GetUserRoles(principal);
            bool acceptApiCalls = UserRoles.AccessDenied != roles;

            return acceptApiCalls;
        }
    }
}
#endif