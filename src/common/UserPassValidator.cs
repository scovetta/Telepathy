using System;
using System.Collections.Generic;
using System.Linq;
using System.IdentityModel.Tokens;
using System.Threading;
using System.Diagnostics;
using System.Net;

using Microsoft.WindowsAzure.ServiceRuntime;

using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.Store;
using Microsoft.Hpc.Azure.Common;

namespace Microsoft.Hpc.Azure.Common
{
    class UserPassValidator : System.IdentityModel.Selectors.UserNamePasswordValidator
    {
        ISchedulerStore _store;
        string headnode;
        object lockObject = new object();

        public UserPassValidator()
        {
            this.headnode = RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.ClusterName);
        }

        public UserPassValidator(ISchedulerStore store)
        {
            _store = store;
        }

        public override void Validate(string userName, string password)
        {
            try
            {
                lock (this.lockObject)
                {
                    // Get connection to scheduler dynamically (instead of in validator's constructor) so that 
                    // REST service can start even if scheduler isnt
                    if (_store == null)
                    {
                        GetStore();
                    }

                    // Authenticate users by calling scheduler to validate user. The scheduler has a cache
                    // so the call should be efficient. A cache can be considered here as well if needed 
                    if (_store.ValidateAzureUser(userName, password))
                    {
                        return;  // user creds are authenticated. 
                    }
                }
            }
            catch (Exception ex)
            {
                // here we catch any communication or other system errors unrelated to authentication.
                Trace.TraceError("UserPassValidator.Validate exception: " + ex.ToString());

                // mapped to 500 to assist test automation
                ex.Data["HttpStatusCode"] =  HttpStatusCode.InternalServerError;

                throw;
            }

            //  by default we fail authentication. 
            throw new SecurityTokenException("Validation Failed: The given user or password is not valid.");
        }

        private void GetStore()
        {
            // Try 6 times, then give up and it will be automatically restarted by the role environment
            int cnt = 6;
            while (cnt > 0)
            {
                try
                {
                    _store = SchedulerStore.ServiceAsClient(this.headnode, StandardServiceAsClientIdentityProviders.ServiceSecurityContext);
                    break;
                }
                catch
                {
                    if (cnt != 0)
                    {
                        cnt--;
                        Thread.Sleep(10000);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}