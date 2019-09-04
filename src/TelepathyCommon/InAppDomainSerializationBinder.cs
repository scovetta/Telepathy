namespace TelepathyCommon
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
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
                string toassname = assemblyName.Split(',')[0];
                Assembly[] asmblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly ass in asmblies)
                {
                    if (ass.FullName.Split(',')[0] == toassname)
                    {
                        ttd = ass.GetType(typeName);
                        break;
                    }
                }
            }
            catch (System.Exception e)
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
