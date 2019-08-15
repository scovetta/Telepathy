using Microsoft.Win32;

namespace TelepathyCommon.Registry
{
    public class NonHARegistry : WindowsRegistryBase
    {
        protected override RegistryKey CreateOrOpenSubKey(string key)
        {
            var tokens = key.Split(new char[] { '\\' }, 2);

            return this.GetRootKey(tokens[0]).CreateSubKey(tokens[1]);
        }
    }
}
