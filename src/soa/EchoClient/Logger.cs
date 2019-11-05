// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.EchoClient
{
    using System;
    using System.Threading;

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
        
        public static void Progress(string msg, int currElementIndex, int totalElementCount)
        {
            DateTime dt = DateTime.Now;
            string message = string.Join(" ",string.Format("\r[INFO][{0:HH:mm:ss.fff}]", dt), msg);
            ShowPercentProgress(message, currElementIndex, totalElementCount);
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

        static void ShowPercentProgress(string message, int currElementIndex, int totalElementCount)
        {
            if (currElementIndex < 0 || currElementIndex >= totalElementCount)
            {
                throw new InvalidOperationException("currElement out of range");
            }

            Console.Write("\r{0}{1}/{2} complete", message, currElementIndex + 1, totalElementCount);
            if (currElementIndex == totalElementCount - 1)
            {
                Console.WriteLine();
            }
        }
    }
}
