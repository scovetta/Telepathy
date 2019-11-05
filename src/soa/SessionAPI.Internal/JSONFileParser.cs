// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
{
    using Microsoft.Telepathy.RuntimeTrace;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    public static class JSONFileParser
    {
        public static string[] parse(string filePath)
        {
            Dictionary<string, string> items;
            List<string> cmd = new List<string>();
            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string json = sr.ReadToEnd();
                    items = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                    foreach (KeyValuePair<string, string> item in items)
                    {
                        cmd.Add("--" + item.Key);
                        cmd.Add(item.Value);
                    }
                }
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Critical, "File path is : {0}, Json file err: {1}.", filePath, e);
                throw;
            }

            string[] argsInJson = cmd.ToArray();
            return argsInJson;
        }
    }
}
