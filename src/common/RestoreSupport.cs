//-------------------------------------------------------------------------------------------------
// <copyright file="RestoreSupport.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// Security review: 
// 
// <summary>
//     Utilities to help determine if a system restore has just occured
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Hpc
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Win32;

    public class MgmtSvcRestore
    {
        VSSRestoreHelper vss = new VSSRestoreHelper(@"HpcManagement");

    }

    public class SchedulerSvcRestore
    {
        VSSRestoreHelper vss = new VSSRestoreHelper(@"HpcScheduler");

        public bool DatabaseRestored()
        {
            return vss.DatabaseRestored();
        }
    }

    public class VSSRestoreHelper
    {
        private string serviceName;
        private struct ValueData
        {
            public string Path, Name;
            public ValueData(string path, string name)
            {
                Path = path;
                Name = name;
            }
        };

        public VSSRestoreHelper(string ServiceName)
        {
            this.serviceName = ServiceName;
        }

        public bool DatabaseRestored()
        {
            ValueData lastInstance = new ValueData(@"System\CurrentControlSet\Control\BackupRestore\SystemStateRestore", "LastInstance");
            ValueData lastRestoreId = new ValueData(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ASR\RestoreSession", "LastRestoreId");

            // open service's SCM key - this is a Win32 API to do this but not easily called.
            RegistryKey svcParamKey = Registry.LocalMachine.CreateSubKey(@"System\CurrentControlSet\Services\" + this.serviceName + @"\Parameters");
            if (svcParamKey == null)
            {
                throw new Exception(String.Format("Couldn't create parameter subkey for service {0}", this.serviceName));
            }

            bool newInstance = false;
            bool newRestoreId = false;

            newInstance = this.CheckDatabaseRestoredValue(svcParamKey, ref lastInstance);
            newRestoreId = this.CheckDatabaseRestoredValue(svcParamKey, ref lastRestoreId);

            return newInstance || newRestoreId;
        }

        private bool CheckDatabaseRestoredValue(RegistryKey SvcParamKey, ref ValueData Value)
        {
            string serviceValue = SvcParamKey.GetValue(Value.Name) as string;

            using (RegistryKey restoreKey = Registry.LocalMachine.OpenSubKey(Value.Path))
            {
                string restoreValue = null;

                if (restoreKey != null)
                {
                    restoreValue = restoreKey.GetValue(Value.Name) as string;
                }

                // if one is empty and the other is not, then something changed. Or if both have values, then they need to be the same.
                if ((string.IsNullOrEmpty(serviceValue) != string.IsNullOrEmpty(restoreValue))
                    ||
                    (!string.IsNullOrEmpty(serviceValue) && !string.IsNullOrEmpty(restoreValue) && serviceValue != restoreValue))
                {
                    SvcParamKey.SetValue(Value.Name, restoreValue);
                    return true;
                }
            }

            return false;
        }
    }
}
