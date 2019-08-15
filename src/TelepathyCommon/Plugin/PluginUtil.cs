using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TelepathyCommon.Plugin
{
    public static class PluginUtil
    {
        public static IEnumerable<T> CreateInstances<T>(string name) where T : class
        {
            IEnumerable<Type> types;

            try
            {
                var path = Environment.ExpandEnvironmentVariables($@"%CCP_HOME%\Bin\{name}");
                var assembly = Assembly.LoadFrom(path);
                types = assembly.GetTypes().Where(t => t.GetInterface(typeof(T).Name) == typeof(T));
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is FileLoadException)
            {
                Trace.TraceWarning("Unable to load file {0}. Exception {1}", name, ex);
                yield break;
            }
            catch (ReflectionTypeLoadException ex)
            {
                Trace.TraceWarning("Unable to load types. Exception {0}", ex);
                foreach (var e in ex.LoaderExceptions)
                {
                    Trace.TraceWarning("Ex {0}", e);
                }

                yield break;
            }

            foreach (var communicatorType in types)
            {
                T instance;

                try
                {
                    instance = Activator.CreateInstance(communicatorType) as T;
                    if (instance == null)
                    {
                        throw new InvalidProgramException();
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Unable to initialize the type {0}. Exception {1}", communicatorType.Name, ex);
                    continue;
                }

                yield return instance;
            }
        }
    }
}
