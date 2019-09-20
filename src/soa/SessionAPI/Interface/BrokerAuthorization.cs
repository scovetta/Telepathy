// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Interface
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Principal;
    using System.ServiceModel;

    using Microsoft.Telepathy.Session.Common;

    using TelepathyCommon.Service;

    /// <summary>
    /// Broker Authroization
    /// </summary>
    [Serializable]
    public class BrokerAuthorization : ISerializable
    {
        /// <summary>
        /// Stores the allowed user
        /// </summary>
        private SecurityIdentifier allowedUser;

        /// <summary>
        /// Stores the security descriptor
        /// </summary>
        private SecurityDescriptor sd;

        /// <summary>
        /// Stores the desired access
        /// </summary>
        private int desiredAccess;

        /// <summary>
        /// Stores the generic rights mapping
        /// </summary>
        private SecurityDescriptorNativeMethods.GENERIC_MAPPING genericRightsMapping;

        /// <summary>
        /// keep a copy of cddl to make the class serializable
        /// </summary>
        private string sddl;

        /// <summary>
        /// Initializes a new instance of the BrokerAuthorization class
        /// Only allow specified user to access
        /// </summary>
        /// <param name="allowedUser">indicate the allowed user</param>
        public BrokerAuthorization(SecurityIdentifier allowedUser)
        {
            this.allowedUser = allowedUser;
        }

        /// <summary>
        /// Initializes a new instance of the BrokerAuthorization class
        /// authorize based on current user and an ACL
        /// </summary>
        /// <param name="sddl">indicating sddl</param>
        /// <param name="desiredAccess">indicating desired access</param>
        /// <param name="genericRead">indicating the generic read</param>
        /// <param name="genericWrite">indicating the generic write</param>
        /// <param name="genericExecute">indicating the generic execute</param>
        /// <param name="genericAll">indicating the generic all</param>
        public BrokerAuthorization(string sddl, int desiredAccess, int genericRead, int genericWrite, int genericExecute, int genericAll)
        {
            if (!string.IsNullOrEmpty(sddl))
            {
                this.sddl = sddl;
                this.sd = SecurityDescriptor.FromSddl(sddl);
            }

            this.desiredAccess = desiredAccess;
            this.genericRightsMapping = new SecurityDescriptorNativeMethods.GENERIC_MAPPING(genericRead, genericWrite, genericExecute, genericAll);
        }

        /// <summary>
        /// Initializes a new instance of the BrokerAuthorization class
        /// </summary>
        /// <param name="info">indicating the serialization info</param>
        /// <param name="context">indicating the streaming context</param>
        protected BrokerAuthorization(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new System.ArgumentNullException("info");
            }

            this.allowedUser = new SecurityIdentifier(info.GetString("allowedUser"));

            if (!String.IsNullOrEmpty(info.GetString("sddl")))
            {
                this.sddl = info.GetString("sddl");
                this.sd = SecurityDescriptor.FromSddl(this.sddl);

                this.desiredAccess = info.GetInt32("desiredAccess");
                this.genericRightsMapping = new SecurityDescriptorNativeMethods.GENERIC_MAPPING(
                       info.GetInt32("genericRead"),
                       info.GetInt32("genericWrite"),
                       info.GetInt32("genericExecute"),
                       info.GetInt32("genericAll"));
            }
        }

        /// <summary>
        /// Check access
        /// </summary>
        /// <returns>whether the access is allowed</returns>
        public bool CheckAccess()
        {
            return this.CheckAccess(ServiceSecurityContext.Current);
        }

        /// <summary>
        /// Check access
        /// </summary>
        /// <param name="context">indicating the security context</param>
        /// <returns>whether the access is allowed</returns>
        public virtual bool CheckAccess(ServiceSecurityContext context)
        {
            if (SoaHelper.IsOnAzure())
            {
                // Skip this check on Azure.
                return true;
            }

            if (context == null)
            {
                return false;
            }

            if (WcfChannelModule.IsX509Identity(context.PrimaryIdentity))
            {
                return true;
            }

            WindowsIdentity user = context.WindowsIdentity;
            return this.CheckAccess(user);
        }

        /// <summary>
        /// Check access for the user
        /// </summary>
        /// <param name="user">indicating the user</param>
        /// <returns>returns if the user can access</returns>
        public virtual bool CheckAccess(WindowsIdentity user)
        {
            if (SoaHelper.IsOnAzure())
            {
                // Skip this check on Azure.
                return true;
            }

            if (user.User == this.allowedUser)
            {
                return true;
            }

            if (this.sd != null)
            {
                return this.sd.CheckAccess(user.Token, this.desiredAccess, this.genericRightsMapping);
            }

            return false;
        }

        /// <summary>
        /// Gets the object data
        /// </summary>
        /// <param name="info">indicating the serialization info</param>
        /// <param name="context">indicating the streaming context</param>
        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new System.ArgumentNullException("info");
            }

            info.AddValue("allowedUser", this.allowedUser.Value);
            info.AddValue("sddl", this.sddl);
            if (this.sddl != null)
            {
                info.AddValue("genericRead", this.genericRightsMapping.GenericRead);
                info.AddValue("genericWrite", this.genericRightsMapping.GenericWrite);
                info.AddValue("genericExecute", this.genericRightsMapping.GenericExecute);
                info.AddValue("genericAll", this.genericRightsMapping.GenericAll);
                info.AddValue("desiredAccess", this.desiredAccess);
            }
        }
    }
}
