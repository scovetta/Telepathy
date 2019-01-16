using System;
using Microsoft.Hpc.Scheduler.Session.Internal;

namespace Microsoft.Hpc.Scheduler.Session.HpcPack.Internal
{
    public abstract class HpcSessionFactory : AbstractSessionFactory
    {
        /// <summary>
        /// Build an instance of the AbstractSessionFactory class to create session
        /// </summary>
        /// <param name="startInfo">indicating the session start information</param>
        /// <returns>returns an instance of the AbstractSessionFactory class</returns>
        public static AbstractSessionFactory BuildSessionFactory(SessionStartInfo startInfo) => new OnPremiseSessionFactory();

        /// <summary>
        /// Build an instance of the AbstractSessionFactory class to create session
        /// </summary>
        /// <param name="sessionAttachInfo">indicating the session attach information</param>
        /// <returns>returns an instance of the AbstractSessionFactory class</returns>
        public static AbstractSessionFactory BuildSessionFactory(SessionAttachInfo sessionAttachInfo)
        {
            if ((sessionAttachInfo.TransportScheme & TransportScheme.WebAPI) == TransportScheme.WebAPI)
            {
                throw new ArgumentException(SR.TransportSchemeWebAPIExclusive);
            }

            return new OnPremiseSessionFactory();
        }

    }
}
