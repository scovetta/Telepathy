// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license

using System;
using System.Globalization;

namespace LogHelper
{
    public class Logger
    {
        public static string GetTimestamp()
        {
            return "[" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "]";
        }
    }
}
