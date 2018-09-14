namespace Microsoft.Hpc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Win32;
    using System.Security.AccessControl;

    public class HARegistry : WindowsRegistryBase
    {
        protected override RegistryKey CreateOrOpenSubKey(string key)
        {
            RegistrySecurity regDACL = new RegistrySecurity();
            RegistryAccessRule jsFullControl = new RegistryAccessRule(@"NT SERVICE\HPCScheduler", RegistryRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow);

            regDACL.SetAccessRule(jsFullControl);

            var tokens = key.Split(new char[] { '\\' }, 2);

            return this.GetRootKey(tokens[0]).CreateSubKey(tokens[1], RegistryKeyPermissionCheck.ReadWriteSubTree, regDACL);
        }
    }
}
