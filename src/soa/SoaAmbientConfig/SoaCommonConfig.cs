// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace SoaAmbientConfig
{
    using System;

    public class SoaCommonConfig
    {
        private static bool? withoutSessionLayer;

        public static bool WithoutSessionLayer
        {
            get
            {
                if (!withoutSessionLayer.HasValue)
                {
                    throw new Exception("Value do not be set.");
                }
                else
                {
                    return withoutSessionLayer.Value;
                }
            }

            set
            {
                if (!withoutSessionLayer.HasValue)
                {
                    withoutSessionLayer = value;
                }
                else if (withoutSessionLayer.Value != value)
                {
                    throw new Exception($"WithoutSessionLayer has been set to {withoutSessionLayer}.");
                }
            }
        }

        public static string StorageCredential { get; set; }
    }
}