// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Common.Registry
{
    using Microsoft.Win32;

    public class NonHARegistry : WindowsRegistryBase
    {
        protected override RegistryKey CreateOrOpenSubKey(string key)
        {
            var tokens = key.Split(new[] { '\\' }, 2);

            return this.GetRootKey(tokens[0]).CreateSubKey(tokens[1]);
        }
    }
}