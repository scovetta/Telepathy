using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler
{
    static class Util
    {
        internal static void CheckArgumentNull(object arg, string argName)
        {
            if (null == arg)
            {
                throw new ArgumentNullException(argName);
            }
        }

        internal static void CheckArgumentNullOrEmpty(string arg, string argName)
        {
            if (null == arg)
            {
                throw new ArgumentNullException(argName);
            }
            if (arg.Trim() == string.Empty)
            {
                throw new ArgumentOutOfRangeException(argName);
            }
        }

        internal static void CheckCollectionForNullOrEmptyStrings(IStringCollection collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("nodeNames", "The collection may not be null");
            }
            foreach (string item in collection)
            {
                if (string.IsNullOrEmpty(item))
                {
                    throw new ArgumentException("nodeNames", "The collection may not contain any null or empty string");
                }
            }
        }

        internal static void CheckArgumentRange(bool ok, string argName)
        {
            if (!ok)
            {
                throw new ArgumentOutOfRangeException(argName);
            }
        }

        internal static char[] Separators = new Char[] { ',' };

        static internal StringCollection String2Collection(string s)
        {
            return String2Collection(s, false, Separators);
        }

        static internal StringCollection String2Collection(string s, bool readOnly, params char[] separators)
        {
            if (separators == null || separators.Length == 0)
            {
                separators = Separators;
            }

            if (!string.IsNullOrEmpty(s))
            {
                return new StringCollection(s.Trim().Split(separators, StringSplitOptions.RemoveEmptyEntries), readOnly);
            }
            return new StringCollection(readOnly);
        }

        static internal IntCollection String2IntCollection(string s)
        {
            return String2IntCollection(s, Separators);
        }

        static internal IntCollection String2IntCollection(string s, params char[] separators)
        {
            if (separators == null || separators.Length == 0)
            {
                separators = Separators;
            }

            if (!string.IsNullOrEmpty(s))
            {
                StringCollection strCollection = String2Collection(s, false, Separators);
                IList<int> list = new List<int>();

                foreach (string str in strCollection)
                {
                    int result = 0;

                    if (int.TryParse(str, out result))
                    {
                        list.Add(result);
                    }
                    else
                    {
                        throw new ArgumentException("The string cannot be parsed into an integer collection", "s");
                    }
                }

                return new IntCollection(list);
            }
            return new IntCollection();
        }

        static internal string Enumerable2String(IEnumerable<string> enumer)
        {
            bool first = true;
            StringBuilder builder = new StringBuilder();
            foreach (string item in enumer)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(Separators[0]);
                }

                builder.Append(item);
            }

            //if there are nothing in the enumerable, return null;
            if (first)
            {
                return null;
            }

            return builder.ToString();
        }

        static internal string EnumerableInt2String(IEnumerable<int> enumer)
        {
            bool first = true;
            StringBuilder builder = new StringBuilder();
            foreach (int item in enumer)
            {
                string str = item.ToString();

                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(Separators[0]);
                }

                builder.Append(str);
            }

            //if there are nothing in the enumerable, return null;
            if (first)
            {
                return null;
            }

            return builder.ToString();
        }

        internal static T[] Collection2Array<T>(ICollection<T> collection)
        {
            if (collection == null)
            {
                return null;
            }
            T[] result = new T[collection.Count];
            collection.CopyTo(result, 0);
            return result;
        }
    }
}
