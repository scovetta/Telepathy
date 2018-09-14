using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.Threading;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines the methods that provide the identity of calling client.</para>
    /// </summary>
    public static class StandardServiceAsClientIdentityProviders
    {
        /// <summary>
        ///   <para>Retrieves the name of the current user.</para>
        /// </summary>
        /// <returns>
        ///   <para>Returns a <see cref="System.String" /> object that contains the name of the current user..</para>
        /// </returns>
        public static string ServiceSecurityContext()
        {
            if (OperationContext.Current == null || OperationContext.Current.ServiceSecurityContext.PrimaryIdentity == null)
            {
                return null;
            }

            return OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name;
        }

        /// <summary>
        ///   <para>Retrieves the identity of calling client.</para>
        /// </summary>
        /// <returns>
        ///   <para>Returns a <see cref="System.String" /> object that contains the identity..</para>
        /// </returns>
        public static string CurrentIdentity()
        {
            if (Thread.CurrentPrincipal == null || Thread.CurrentPrincipal.Identity == null)
            {
                return null;
            }

            return Thread.CurrentPrincipal.Identity.Name;
        }
    }
}
