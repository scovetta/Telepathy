namespace TestClient
{
    using System.Collections.Specialized;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Test Framework Related Functions
    /// </summary>
    class ArgumentsParser
    {
        // Variables
        private StringDictionary Parameters;

        // Constructor
        public ArgumentsParser(string[] Args)
        {
            Parameters = new StringDictionary();
            Regex Spliter = new Regex(@"^-{1,2}|^/|=|:",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            Regex Remover = new Regex(@"^['""]?(.*?)['""]?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            string Parameter = null;
            string[] Parts;

            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples: 
            // -param1 value1 --param2 /param3:"Test-:-work" 
            //   /param4=happy -param5 '--=nice=--'
            foreach (string Txt in Args)
            {
                // Skip the URI protocal prefix
                if (!Txt.StartsWith("net.tcp://") && !Txt.Contains(@":\"))
                {
                    // Look for new parameters (-,/ or --) and a
                    // possible enclosed value (=,:)
                    Parts = Spliter.Split(Txt, 3);
                }
                else
                {
                    Parts = new string[1] { Txt };
                }

                switch (Parts.Length)
                {
                    // Found a value (for the last parameter 
                    // found (space separator))
                    case 1:
                        if (Parameter != null)
                        {
                            if (!Parameters.ContainsKey(Parameter))
                            {
                                Parts[0] =
                                    Remover.Replace(Parts[0], "$1");

                                Parameters.Add(Parameter, Parts[0]);
                            }
                            Parameter = null;
                        }
                        // else Error: no parameter waiting for a value (skipped)
                        break;

                    // Found just a parameter
                    case 2:
                        // The last parameter is still waiting. 
                        // With no value, set it to true.
                        if (Parameter != null)
                        {
                            if (!Parameters.ContainsKey(Parameter))
                                Parameters.Add(Parameter, "true");
                        }
                        Parameter = Parts[1];
                        break;

                    // Parameter with enclosed value
                    case 3:
                        // The last parameter is still waiting. 
                        // With no value, set it to true.
                        if (Parameter != null)
                        {
                            if (!Parameters.ContainsKey(Parameter))
                                Parameters.Add(Parameter, "true");
                        }

                        Parameter = Parts[1];

                        // Remove possible enclosing characters (",')
                        if (!Parameters.ContainsKey(Parameter))
                        {
                            Parts[2] = Remover.Replace(Parts[2], "$1");
                            Parameters.Add(Parameter, Parts[2]);
                        }

                        Parameter = null;
                        break;
                }
            }
            // In case a parameter is still waiting
            if (Parameter != null)
            {
                if (!Parameters.ContainsKey(Parameter))
                    Parameters.Add(Parameter, "true");
            }
        }

        // Retrieve a parameter value if it exists 
        // (overriding C# indexer property)
        public string this[string Param]
        {
            get
            {
                return (Parameters[Param]);
            }
        }
        /// <summary>
        /// if toget is not null or empty, set it to toset
        /// </summary>
        /// <param name="toget"></param>
        /// <param name="toset"></param>
        public static void SetIfExist(string toget, ref string toset)
        {
            if (!string.IsNullOrEmpty(toget))
                toset = toget;
        }

        /// <summary>
        /// if toget is valid int format, set it to toset
        /// </summary>
        /// <param name="toget"></param>
        /// <param name="toset"></param>
        public static void SetIfExist(string toget, ref int toset)
        {
            int dummy;
            if (!string.IsNullOrEmpty(toget) && int.TryParse(toget, out dummy))
                toset = dummy;
        }

        /// <summary>
        /// if toget is valid long format, set it to toset
        /// </summary>
        /// <param name="toget"></param>
        /// <param name="toset"></param>
        public static void SetIfExist(string toget, ref long toset)
        {
            long dummy;
            if (!string.IsNullOrEmpty(toget) && long.TryParse(toget, out dummy))
                toset = dummy;
        }

        /// <summary>
        /// if toget is valid int format, set it to toset
        /// </summary>
        /// <param name="toget"></param>
        /// <param name="toset"></param>
        public static void SetIfExist(string toget, ref bool toset)
        {
            if (!string.IsNullOrEmpty(toget))
                toset = true;
        }
    }
}
