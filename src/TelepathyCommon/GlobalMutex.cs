namespace TelepathyCommon
{
    using System;
    using System.Diagnostics;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Threading;

    public class GlobalMutex : IDisposable
    {
        private static readonly MutexSecurity MutexSecuritySetting;

        private readonly bool hasHandle;

        private Mutex mutex;

        static GlobalMutex()
        {
            MutexSecuritySetting = new MutexSecurity();
            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            MutexSecuritySetting.AddAccessRule(allowEveryoneRule);
        }

        public GlobalMutex(string mutexName, int timeOut)
        {
            mutexName = mutexName.Replace('\\', '_');
            this.InitMutex(mutexName);
            try
            {
                if (timeOut < 0)
                {
                    this.hasHandle = this.mutex.WaitOne(Timeout.Infinite, false);
                }
                else
                {
                    this.hasHandle = this.mutex.WaitOne(timeOut, false);
                }

                if (this.hasHandle == false)
                {
                    Trace.TraceError($"[{nameof(GlobalMutex)}]Get mutex {mutexName} timeout.");
                    throw new TimeoutException("Timeout waiting for exclusive access on SingleInstance");
                }
            }
            catch (AbandonedMutexException)
            {
                Trace.TraceWarning($"[{nameof(GlobalMutex)}]Recovered {nameof(AbandonedMutexException)}.");
                this.hasHandle = true;
            }
        }

        public void Dispose()
        {
            if (this.mutex != null)
            {
                if (this.hasHandle)
                {
                    this.mutex.ReleaseMutex();
                }

                this.mutex.Dispose();
            }

            // Suppress finalization of this disposed instance.
            GC.SuppressFinalize(this);
        }

        private void InitMutex(string mutexName)
        {
            var mutexId = $"Global\\{{{mutexName}}}";
            this.mutex = new Mutex(false, mutexId);
            this.mutex.SetAccessControl(MutexSecuritySetting);
        }
    }
}