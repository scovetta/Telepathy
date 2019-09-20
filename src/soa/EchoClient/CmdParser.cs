// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.EchoClient
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
            this.argsGroup = new Dictionary<string, string>();
            this.switchGroup = new List<string>();

            this.usedArguments = new List<string>();
            this.usedSwitchParames = new List<string>();
            this.storeArgs = new List<string>();

            foreach (string arg in args)
            {
                this.storeArgs.Add(arg.ToLowerInvariant());
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
                        if (!this.argsGroup.ContainsKey(paramName))
                        {
                            this.argsGroup.Add(paramName, paramValue);
                        }
                    }
                    else
                    {
                        if (!this.switchGroup.Contains(paramName))
                        {
                            this.switchGroup.Add(paramName);
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
                        if (!this.switchGroup.Contains(preArg))
                        {
                            this.switchGroup.Add(preArg);
                        }
                    }
                    preArg = argTrimmed;
                }
                else
                {
                    if (preArg != string.Empty)
                    {
                        if (!this.argsGroup.ContainsKey(preArg))
                        {
                            this.argsGroup.Add(preArg, arg);
                            preArg = string.Empty;
                        }
                    }
                }
            }

            if (preArg != string.Empty)
            {
                if (!this.switchGroup.Contains(preArg))
                {
                    this.switchGroup.Add(preArg);
                }
            }

        }

        public void TryGetArgList<T>(string arg, ref List<T> t)
        {
            List<string> value = this.GetArgList(arg);
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
            if (this.storeArgs.Contains(_arg))
            {
                this.usedArguments.Add(arg);
                for (int i = this.storeArgs.FindIndex((s) => { return s.Equals(_arg); }) + 1; i < this.storeArgs.Count; i++)
                {
                    if (!this.storeArgs[i].StartsWith("-"))
                    {
                        value.Add(this.storeArgs[i]);
                        this.usedArguments.Add(this.storeArgs[i]);
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
                for (start = 0; start < this.storeArgs.Count; start++)
                {
                    if (_arg.StartsWith(this.storeArgs[start], StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.usedArguments.Add(this.storeArgs[start].TrimStart(new char[] { '-' }).ToLowerInvariant());
                        break;
                    }
                }

                for (int i = start + 1; i < this.storeArgs.Count; i++)
                {
                    if (!this.storeArgs[i].StartsWith("-"))
                    {
                        value.Add(this.storeArgs[i]);
                        this.usedArguments.Add(this.storeArgs[i]);
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
            string value = this.GetArg(arg);
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
            if (this.argsGroup.ContainsKey(arg))
            {
                value = this.argsGroup[arg];
                this.usedArguments.Add(arg);
            }
            if (value == null)
            {
                foreach (string key in this.argsGroup.Keys)
                {
                    if (arg.StartsWith(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        value = this.argsGroup[key];
                        this.usedArguments.Add(key);
                        break;
                    }
                }
            }
            return value;
        }

        public bool GetSwitch(string swi)
        {
            bool on = this.switchGroup.Contains(swi);
            if (on)
            {
                this.usedSwitchParames.Add(swi);
            }
            else
            {
                foreach (string s in this.switchGroup)
                {
                    if (swi.StartsWith(s, StringComparison.InvariantCultureIgnoreCase))
                    {
                        on = true;
                        this.usedSwitchParames.Add(s);
                        break;
                    }
                }
            }
            return on;
        }

        public void Unused(out Dictionary<string, string> unusedArgs, out List<string> unusedSwitches)
        {
            unusedArgs = this.argsGroup.Where(kv => !this.usedArguments.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
            unusedSwitches = this.switchGroup.Where(s => !this.usedSwitchParames.Contains(s)).ToList();
        }

        public void Used(out Dictionary<string, string> usedArgs, out List<string> usedSwitches)
        {
            usedArgs = this.argsGroup.Where(kv => this.usedArguments.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
            usedSwitches = this.switchGroup.Where(s => this.usedSwitchParames.Contains(s)).ToList();
        }
    }
}
