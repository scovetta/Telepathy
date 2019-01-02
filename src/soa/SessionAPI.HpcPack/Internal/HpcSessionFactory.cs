using System;
using Microsoft.Hpc.Scheduler.Session.Internal;

namespace Microsoft.Hpc.Scheduler.Session.HpcPack.Internal
{
    public abstract class HpcSessionFactory : SessionFactory
    {
        /// <summary>
        /// Build an instance of the SessionFactory class to create session
        /// </summary>
        /// <param name="startInfo">indicating the session start information</param>
        /// <returns>returns an instance of the SessionFactory class</returns>
        public static SessionFactory BuildSessionFactory(SessionStartInfo startInfo) => new OnPremiseSessionFactory();

        /// <summary>
        /// Build an instance of the SessionFactory class to create session
        /// </summary>
        /// <param name="sessionAttachInfo">indicating the session attach information</param>
        /// <returns>returns an instance of the SessionFactory class</returns>
        public static SessionFactory BuildSessionFactory(SessionAttachInfo sessionAttachInfo)
        {
            if ((sessionAttachInfo.TransportScheme & TransportScheme.WebAPI) == TransportScheme.WebAPI)
            {
                throw new ArgumentException(SR.TransportSchemeWebAPIExclusive);
            }

            return new OnPremiseSessionFactory();
        }

    }
}
