//-------------------------------------------------------------------------------------------------
// <copyright file="CommonUtils.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Hpc.Azure.Utils
{
    class CommonUtils
    {
        public static void AddEnvPath(string path, EnvironmentVariableTarget target)
        {
            // if binPath has not been defined before
            if (HasPathDefined(path, target) == false)
            {
                Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) + ";" + path, target);
            }
        }

        /// <summary>
        /// Check whether the bin path has been added into the "PATH" environment variable
        /// </summary>
        /// <param name="pathToAdd"></param>
        /// <returns></returns>
        static bool HasPathDefined(string pathToAdd, EnvironmentVariableTarget target)
        {
            pathToAdd = pathToAdd.Trim();
            string envPath = Environment.GetEnvironmentVariable("PATH", target);

            if (String.IsNullOrWhiteSpace(envPath))
            {
                return false;
            }

            string[] paths = envPath.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            if (paths.Length == 0 || String.IsNullOrWhiteSpace(pathToAdd) == true)
            {
                return false;
            }

            string tempStr = String.Empty;
            int length = pathToAdd.Length;

            // binPath may end with '\' 
            // we need a version of the string without '\' and one with it
            // if the string already ends with '\', create one without it
            if (pathToAdd[length - 1] == '\\')
            {
                tempStr = pathToAdd.Substring(0, length - 1);
            }
            // If the string does not end with '\', create one with it
            else
            {
                tempStr = pathToAdd + "\\";
            }

            foreach (string path in paths)
            {
                string pathTrimmed = path.Trim();

                // Check whether either of binPath or tempStr has been added into PATH environment variable
                if (String.Compare(pathTrimmed, pathToAdd, true, System.Globalization.CultureInfo.InvariantCulture) == 0
                    || String.Compare(pathTrimmed, tempStr, true, System.Globalization.CultureInfo.InvariantCulture) == 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
