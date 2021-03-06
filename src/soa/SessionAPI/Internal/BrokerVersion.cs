﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
{
    /// <summary>
    /// Maintains broker's versioning information
    /// </summary>
    public class BrokerVersion
    {
        /// <summary>broker persist version number</summary>
        //Note: DO NOT change this version number unless:
        // 1) you changed the message schema and,
        // 2) you decided that old broker should not be able to handle the new message schema.
        public const int PersistVersion = 1;

        /// <summary>default persist version number</summary>
        //Note: NEVER change this number.
        public const int DefaultPersistVersion = 1;

        /// <summary>
        /// Check if a persist version is supported by this broker
        /// </summary>
        /// <param name="persistVersion">the persist version to be checked</param>
        /// <returns>true if the persist version is supported, false otherwise</returns>
        public static bool IsSupportedPersistVersion(int persistVersion)
        {
            for (int i = 0; i < SupportedPersistVersions.Length; i++)
            {
                if (SupportedPersistVersions[i] == persistVersion)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary> array of supported persist versions </summary>        
        private static int[] SupportedPersistVersions = { PersistVersion };
    };
}
