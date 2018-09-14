//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Common command line parser
// </summary>
//------------------------------------------------------------------------------


namespace Microsoft.Hpc.EchoClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class CmdParser
    {
        private Dictionary<string, string> argsGroup;
        private List<string> switchGroup;

        private List<string> usedArguments;
        private List<string> usedSwitchParames;
        private List<string> storeArgs;

        public CmdParser(string[] args)
        {
            argsGroup = new Dictionary<string, string>();
            switchGroup = new List<string>();

            usedArguments = new List<string>();
            usedSwitchParames = new List<string>();
            storeArgs = new List<string>();

            foreach (string arg in args)
            {
                storeArgs.Add(arg.ToLowerInvariant());
                Regex rp = new Regex("(?<=^/).*?(?=:|$)");
                Match pMatch = rp.Match(arg);
                if (pMatch.Success)
                {
                    string paramName = pMatch.Value.ToLowerInvariant();
                    Regex rv = new Regex("(?<=(^/.*?:)).*$");
                    Match vMatch = rv.Match(arg);
                    if (vMatch.Success)
                    {
                        string paramValue = Environment.ExpandEnvironmentVariables(vMatch.Value);
                        if (!argsGroup.ContainsKey(paramName))
                        {
                            argsGroup.Add(paramName, paramValue);
                        }
                    }
                    else
                    {
                        if (!switchGroup.Contains(paramName))
                        {
                            switchGroup.Add(paramName);
                        }
                    }
                }
            }

            //for -args value -switches type
            string preArg = string.Empty;
            foreach (string arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    string argTrimmed = arg.TrimStart(new char[] { '-' }).ToLowerInvariant();
                    if (preArg != string.Empty)
                    {
                        if (!switchGroup.Contains(preArg))
                        {
                            switchGroup.Add(preArg);
                        }
                    }
                    preArg = argTrimmed;
                }
                else
                {
                    if (preArg != string.Empty)
                    {
                        if (!argsGroup.ContainsKey(preArg))
                        {
                            argsGroup.Add(preArg, arg);
                            preArg = string.Empty;
                        }
                    }
                }
            }

            if (preArg != string.Empty)
            {
                if (!switchGroup.Contains(preArg))
                {
                    switchGroup.Add(preArg);
                }
            }

        }

        public void TryGetArgList<T>(string arg, ref List<T> t)
        {
            List<string> value = GetArgList(arg);
            if (value.Count > 0)
            {
                try
                {
                    Type type = typeof(T);
                    t = new List<T>();
                    foreach (string str in value)
                    {
                        t.Add((T) Convert.ChangeType(str,
                            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                                ? Nullable.GetUnderlyingType(typeof(T))
                                : typeof(T)));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Input Error]: Parameter -{0} value \"{1}\" cannot be parsed : {2}", arg, value, e.ToString());
                }
            }
        }

        private List<string> GetArgList(string arg)
        {
            arg = arg.ToLowerInvariant();
            string _arg = "-" + arg;
            List<string> value = new List<string>();
            if (storeArgs.Contains(_arg))
            {
                usedArguments.Add(arg);
                for (int i = storeArgs.FindIndex((s) => { return s.Equals(_arg); }) + 1; i < storeArgs.Count; i++)
                {
                    if (!storeArgs[i].StartsWith("-"))
                    {
                        value.Add(storeArgs[i]);
                        usedArguments.Add(storeArgs[i]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (value.Count == 0)
            {
                int start;
                for (start = 0; start < storeArgs.Count; start++)
                {
                    if (_arg.StartsWith(storeArgs[start], StringComparison.InvariantCultureIgnoreCase))
                    {
                        usedArguments.Add(storeArgs[start].TrimStart(new char[] { '-' }).ToLowerInvariant());
                        break;
                    }
                }

                for (int i = start + 1; i < storeArgs.Count; i++)
                {
                    if (!storeArgs[i].StartsWith("-"))
                    {
                        value.Add(storeArgs[i]);
                        usedArguments.Add(storeArgs[i]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return value;
        }

        public void TryGetArg<T>(string arg, ref T t)
        {
            string value = GetArg(arg);
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    Type type = typeof(T);
                    t = (T)Convert.ChangeType(value, type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(typeof(T)) : typeof(T));
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Input Error]: Parameter -{0} value \"{1}\" cannot be parsed : {2}", arg, value, e.ToString());
                }
            }
        }

        private string GetArg(string arg)
        {
            arg=arg.ToLowerInvariant();
            string value = null;
            if (argsGroup.ContainsKey(arg))
            {
                value = argsGroup[arg];
                usedArguments.Add(arg);
            }
            if (value == null)
            {
                foreach (string key in argsGroup.Keys)
                {
                    if (arg.StartsWith(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        value = argsGroup[key];
                        usedArguments.Add(key);
                        break;
                    }
                }
            }
            return value;
        }

        public bool GetSwitch(string swi)
        {
            bool on = switchGroup.Contains(swi);
            if (on)
            {
                usedSwitchParames.Add(swi);
            }
            else
            {
                foreach (string s in switchGroup)
                {
                    if (swi.StartsWith(s, StringComparison.InvariantCultureIgnoreCase))
                    {
                        on = true;
                        usedSwitchParames.Add(s);
                        break;
                    }
                }
            }
            return on;
        }

        public void Unused(out Dictionary<string, string> unusedArgs, out List<string> unusedSwitches)
        {
            unusedArgs = argsGroup.Where(kv => !usedArguments.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
            unusedSwitches = switchGroup.Where(s => !usedSwitchParames.Contains(s)).ToList();
        }

        public void Used(out Dictionary<string, string> usedArgs, out List<string> usedSwitches)
        {
            usedArgs = argsGroup.Where(kv => usedArguments.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
            usedSwitches = switchGroup.Where(s => usedSwitchParames.Contains(s)).ToList();
        }
    }
}
