using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Hpc.Scheduler.Store;

namespace Microsoft.Hpc.Scheduler.Internal
{
    internal class PropertyUtils2
    {
        internal static char[] Separators = new Char[] { ',' };

        static internal string[] String2Array(string s)
        {
            return String2Array(s, Separators);
        }

        static internal string[] String2Array(string s, params char[] separators)
        {
            if (!string.IsNullOrEmpty(s))
            {
                return s.Trim().Split(separators, StringSplitOptions.RemoveEmptyEntries);
            }
            return new string[] { };
        }
    }
}
