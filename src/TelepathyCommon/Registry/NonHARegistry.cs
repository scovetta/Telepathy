namespace Microsoft.Hpc
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class NonHARegistry : WindowsRegistryBase
    {
        protected override RegistryKey CreateOrOpenSubKey(string key)
        {
            var tokens = key.Split(new char[] { '\\' }, 2);

            return this.GetRootKey(tokens[0]).CreateSubKey(tokens[1]);
        }
    }
}
