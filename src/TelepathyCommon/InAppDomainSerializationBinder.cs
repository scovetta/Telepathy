// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Common
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Runtime.Serialization.Formatters.Binary;

    internal class InAppDomainSerializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type ttd = null;
            try
            {
                var toassname = assemblyName.Split(',')[0];
                var asmblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var ass in asmblies)
                {
                    if (ass.FullName.Split(',')[0] == toassname)
                    {
                        ttd = ass.GetType(typeName);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }

            return ttd;
        }
    }

    public static class BinaryFormatterExtention
    {
        public static void UseInAppDomainSerializationBinder(this BinaryFormatter binaryFormatter)
        {
            binaryFormatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
            binaryFormatter.Binder = new InAppDomainSerializationBinder();
        }
    }
}