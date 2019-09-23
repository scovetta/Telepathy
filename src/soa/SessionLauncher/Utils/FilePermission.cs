// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.DirectoryServices.AccountManagement;
    using System.IO;
    using System.Security.AccessControl;
    using System.Security.Principal;

    /// <summary>
    /// The FilePermission class implements some common functionality related to
    /// file permission check
    /// </summary>
    internal static class FilePermission
    {
        /// <summary>
        /// Check if a user has specified file system rights to a path
        /// </summary>
        /// <param name="identity">user's windows identity</param>
        /// <param name="path">target file/directory path</param>
        /// <param name="isDir">a flag indicating if the specified path points to a directory or a file</param>
        /// <param name="rights">file system rights to be checked</param>
        public static void CheckPermission(WindowsIdentity identity, string path, bool isDir, FileSystemRights rights)
        {
            Debug.Assert(identity != null, "identity");
            if (rights == 0)
            {
                return;
            }

            List<string> sids = GetRelatedSids(identity);
            AuthorizationRuleCollection accessRules = GetAccessRules(path, isDir);
            CheckEffectiveRights(sids, accessRules, rights);
        }

        /// <summary>
        /// Check if a user has specified file system rights to a path
        /// </summary>
        /// <param name="user">user principal</param>
        /// <param name="path">target file/directory path</param>
        /// <param name="isDir">a flag indicating whether the specified path points to a directory or a file</param>
        /// <param name="rights">file system rights to be checked</param>
        public static void CheckPermission(UserPrincipal user, string path, bool isDir, FileSystemRights rights)
        {
            Debug.Assert(user != null, "user");
            if (rights == 0)
            {
                return;
            }

            List<string> sids = GetRelatedSids(user);
            AuthorizationRuleCollection accessRules = GetAccessRules(path, isDir);
            CheckEffectiveRights(sids, accessRules, rights);
        }

        /// <summary>
        /// Grant a user specified file system rights to a path
        /// </summary>
        /// <param name="userName">user name</param>
        /// <param name="path">target file/directory path</param>
        /// <param name="isDir">a flag indicating whether the specified path points to a directory or a file</param>
        /// <param name="rights">file system rights to be granted</param>
        public static void GrantPermission(string userName, string path, bool isDir, FileSystemRights rights)
        {
            if (!isDir)
            {
                FileSecurity fileSecurity = File.GetAccessControl(path);
                fileSecurity.AddAccessRule(new FileSystemAccessRule(userName, rights, AccessControlType.Allow));
                File.SetAccessControl(path, fileSecurity);
            }
            else
            {
                DirectorySecurity dirSecurity = Directory.GetAccessControl(path);
                dirSecurity.AddAccessRule(new FileSystemAccessRule(userName, rights, AccessControlType.Allow));
                Directory.SetAccessControl(path, dirSecurity);
            }
        }

        /// <summary>
        /// Get sids related to a user
        /// </summary>
        /// <param name="user">user principal</param>
        /// <returns>list of all sids related to the specified user</returns>
        private static List<string> GetRelatedSids(UserPrincipal user)
        {
            List<string> sidList = new List<string>();
            sidList.Add(user.Sid.Value);

            // NOTE: Principal.GetGroups() returns only the groups of which the principal is directly a member.
            foreach (Principal group in user.GetAuthorizationGroups())
            {
                using (group)
                {
                    sidList.Add(group.Sid.Value);
                }
            }

            return sidList;
        }

        /// <summary>
        /// Get sids related to a windows identity
        /// </summary>
        /// <param name="identity">windows identity</param>
        /// <returns>list of all sids related to the specified windows identity</returns>
        private static List<string> GetRelatedSids(WindowsIdentity identity)
        {
            List<string> sidList = new List<string>();
            sidList.Add(identity.User.Value);
            foreach (IdentityReference ir in identity.Groups)
            {
                sidList.Add(ir.Value);
            }

            return sidList;
        }

        /// <summary>
        /// Get access rules for a file/directory 
        /// </summary>
        /// <param name="path">target file/directory path</param>
        /// <param name="isDir">a flag indicating if the path points to a directory or a file</param>
        /// <returns>all access rules for the path</returns>
        private static AuthorizationRuleCollection GetAccessRules(string path, bool isDir)
        {
            if (!isDir)
            {
                FileInfo fi = new FileInfo(path);
                return fi.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(path);
                return di.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
            }
        }

        /// <summary>
        /// Check if the list of sids have specified rights against a set of ACEs
        /// </summary>
        /// <param name="sids">list of sids</param>
        /// <param name="accessRules">access rules to be checked against</param>
        /// <param name="rights">file system rights to be checked</param>
        private static void CheckEffectiveRights(List<string> sids, AuthorizationRuleCollection accessRules, FileSystemRights rights)
        {
            Debug.Assert(sids != null && sids.Count > 0, "sids");
            Debug.Assert(accessRules != null, "accessRules");

            // MSDN reference:
            // 1. How DACLs Control Access to an Object
            // http://msdn.microsoft.com/en-us/library/windows/desktop/aa446683(v=vs.85).aspx
            // 2. Order of ACEs in DACL
            // http://technet.microsoft.com/en-us/library/cc961994.aspx
            // 3. a blog on MSDN: Why does canonical order for ACEs put deny ACEs ahead of allow ACEs?
            // http://blogs.msdn.com/b/oldnewthing/archive/2007/06/08/3150719.aspx
            FileSystemRights rightsNeeded = rights;
            foreach (AuthorizationRule ar in accessRules)
            {
                FileSystemAccessRule accessRule = ar as FileSystemAccessRule;
                if (sids.Contains(accessRule.IdentityReference.Value))
                {
                    if (accessRule.AccessControlType == AccessControlType.Deny)
                    {
                        if ((rightsNeeded & accessRule.FileSystemRights) != 0)
                        {
                            throw new UnauthorizedAccessException();
                        }
                    }
                    else
                    {
                        rightsNeeded &= ~accessRule.FileSystemRights;
                        if (rightsNeeded == 0)
                        {
                            return;
                        }
                    }
                }
            }

            throw new UnauthorizedAccessException();
        }

        /// <summary>
        /// Canonicalize the Dacl
        /// </summary>
        /// <param name="objectSecurity"></param>
        public static void CanonicalizeDacl(NativeObjectSecurity objectSecurity)
        {
            if (objectSecurity.AreAccessRulesCanonical)
            {
                return;
            }

            // A canonical ACL must have ACES sorted according to the following order:
            //   1. Access-denied on the object
            //   2. Access-denied on a child or property
            //   3. Access-allowed on the object
            //   4. Access-allowed on a child or property
            //   5. All inherited ACEs 
            RawSecurityDescriptor descriptor = new RawSecurityDescriptor(objectSecurity.GetSecurityDescriptorSddlForm(AccessControlSections.Access));

            var explicitDenyDacl = new List<CommonAce>();
            var explicitDenyObjectDacl = new List<CommonAce>();
            var inheritedDacl = new List<CommonAce>();
            var explicitAllowDacl = new List<CommonAce>();
            var explicitAllowObjectDacl = new List<CommonAce>();

            foreach (CommonAce ace in descriptor.DiscretionaryAcl)
            {
                if ((ace.AceFlags & AceFlags.Inherited) == AceFlags.Inherited)
                {
                    inheritedDacl.Add(ace);
                }
                else
                {
                    switch (ace.AceType)
                    {
                        case AceType.AccessAllowed:
                            explicitAllowDacl.Add(ace);
                            break;

                        case AceType.AccessDenied:
                            explicitDenyDacl.Add(ace);
                            break;

                        case AceType.AccessAllowedObject:
                            explicitAllowObjectDacl.Add(ace);
                            break;

                        case AceType.AccessDeniedObject:
                            explicitDenyObjectDacl.Add(ace);
                            break;
                    }
                }
            }

            int aceIndex = 0;
            var newDacl = new RawAcl(descriptor.DiscretionaryAcl.Revision, descriptor.DiscretionaryAcl.Count);
            explicitDenyDacl.ForEach(x => newDacl.InsertAce(aceIndex++, x));
            explicitDenyObjectDacl.ForEach(x => newDacl.InsertAce(aceIndex++, x));
            explicitAllowDacl.ForEach(x => newDacl.InsertAce(aceIndex++, x));
            explicitAllowObjectDacl.ForEach(x => newDacl.InsertAce(aceIndex++, x));
            inheritedDacl.ForEach(x => newDacl.InsertAce(aceIndex++, x));

            if (aceIndex != descriptor.DiscretionaryAcl.Count)
            {
                throw new InvalidOperationException("The Dacl cannot be canonicalized since it would potentially result in a loss of information");
            }

            descriptor.DiscretionaryAcl = newDacl;
            objectSecurity.SetSecurityDescriptorSddlForm(descriptor.GetSddlForm(AccessControlSections.Access), AccessControlSections.Access);
        }
    }
}
