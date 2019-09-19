// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.EchoClient
{
    using System;

    public class Logger
    {
        private const ConsoleColor WARN_FG_COLOR = ConsoleColor.Yellow;
        private const ConsoleColor ERROR_FG_COLOR = ConsoleColor.Red;

        public static void Info(string msg, params object[] objToLog)
        {
            DateTime dt = DateTime.Now;
            string message = string.Join(" ",string.Format("[INFO][{0:HH:mm:ss.fff}]", dt), string.Format(msg, objToLog));
            Console.WriteLine(message);
        }

        public static void Warning(string msg, params object[] objToLog)
        {
            DateTime dt = DateTime.Now;
            string message = string.Join(" ", string.Format("[Warning][{0:HH:mm:ss.fff}]", dt), string.Format(msg, objToLog));
            ConsoleColor prevFGColor = Console.ForegroundColor;
            Console.ForegroundColor = WARN_FG_COLOR;
            Console.WriteLine(message);
            Console.ForegroundColor = prevFGColor;
        }

        public static void Error(string msg, params object[] objToLog)
        {
            DateTime dt = DateTime.Now;
            string message = string.Join(" ", string.Format("[Error][{0:HH:mm:ss.fff}]", dt), string.Format(msg, objToLog));
            ConsoleColor prevFGColor = Console.ForegroundColor;
            Console.ForegroundColor = ERROR_FG_COLOR;
            Console.WriteLine(message);
            Console.ForegroundColor = prevFGColor;
        }
    }
}
