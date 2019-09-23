// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.CcpServiceHost
{
    using System;

    class ProductConstants
    {
        public class ProductNames
        {
            public const string FullProductName = FullProductName_V5;
            public const string ShortProductName = ShortProductname_V5;
            public const string InstallRootDirectory = FullProductName_V5;

            public const string FullProductName_V5 = "Microsoft " + ShortProductname_V5;
            public const string ShortProductname_V5 = "HPC Pack 2016";
            
            public const string FullProductName_V4SP2 = "Microsoft " + ShortProductname_V4SP2;
            public const string ShortProductname_V4SP2 = "HPC Pack 2012 R2";

            public const string FullProductName_V4 = "Microsoft " + ShortProductName_V4;
            public const string ShortProductName_V4 = "HPC Pack 2012";

            public const string FullProductName_V3 = "Microsoft " + ShortProductName_V3;
            public const string ShortProductName_V3 = "HPC Pack 2008 R2";

            public const string FullProductName_V3_SP1 = "Microsoft " + ShortProductName_V3_SP1;
            public const string ShortProductName_V3_SP1 = ShortProductName_V3 +  " Service Pack 1";

            public const string FullProductName_V3_SP2 = "Microsoft " + ShortProductName_V3_SP2;
            public const string ShortProductName_V3_SP2 = ShortProductName_V3 + " Service Pack 2";

            public const string FullProductName_V3_SP3 = "Microsoft " + ShortProductName_V3_SP3;
            public const string ShortProductName_V3_SP3 = ShortProductName_V3 + " Service Pack 3";

            public const string FullProductName_V3_SP4 = "Microsoft " + ShortProductName_V3_SP4;
            public const string ShortProductName_V3_SP4 = ShortProductName_V3 + " Service Pack 4";

            public const string FullProductName_V2 = "Microsoft " + ShortProductName_V3;
            public const string ShortProductName_V2 = "HPC Pack 2008";
        }

        public class MsiProductIds
        {
            // non-version specific reference to current version. Redirect when new version is added
            public const string HpcServer_X64 = HpcServerV5_X64;
            public const string HpcClient_X64 = HpcClientV5_X64;
            public const string HpcExcel_X64 = HpcExcelV5_X64;

            public const string HpcServer_X86 = HpcServerV5_X86;
            public const string HpcClient_X86 = HpcClientV5_X86;
            public const string HpcExcel_X86 = HpcExcelV5_X86;

            public const string HpcServerV5_X64 = "{02985CCE-D7D5-40FF-9C81-6334523210F9}";
            public const string HpcClientV5_X64 = "{186B7E1A-6C30-46AB-AB83-4AE925377838}";
            public const string HpcExcelV5_X64  = "{F22EA2E8-08F0-4675-B10F-60C939D336D6}";

            public const string HpcServerV5_X86 = "{179E6026-D40A-4A88-AA03-6BAB8A8C192C}";
            public const string HpcClientV5_X86 = "{E278C9CF-D06B-47DB-8C97-6F12D9FCCCDA}";
            public const string HpcExcelV5_X86  = "{25BBC125-40C3-4ED2-9E0E-5D192809F653}";

            public const string HpcServerV4_X64 = "{166692AA-06DD-43E6-97F3-7D0B58220094}";
            public const string HpcClientV4_X64 = "{63235828-843B-49E0-93A8-A0571EB6169A}";
            public const string HpcMpiV4_X64    = "{F280A816-C0CB-4700-A3C6-9FDD8C80FD18}";
            public const string HpcExcelV4_X64  = "{D94E3DAD-588E-43FC-AE8F-91B324774709}";

            public const string HpcServerV4_X86 = "{1310CCD5-382C-4C79-8D8E-EC56C790E2B7}";
            public const string HpcClientV4_X86 = "{1BF24E11-D39A-41E7-9551-AD1DBE0FE18E}";
            public const string HpcMpiV4_X86    = "{90A714B2-9835-418B-BB0F-62750CC442B2}";
            public const string HpcExcelV4_X86  = "{C9C2D3D0-BA05-458F-9760-AB936FDC2B43}";

            public const string HpcServerV3_X64 = "{CD5190DD-A85D-4844-9BF4-AC6B04EB1A12}";
            public const string HpcServerV3_X86 = "{F1BB42C1-B9AE-4f47-843A-12FE8F956FFA}";
        
            public const string HpcServerV2_X64 = "{21CE1D35-AB84-46fe-B2FD-B57271BE7B93}";
            public const string HpcServerV2_X86 = "{3EEC9986-EDE0-4ee2-ACFB-A3CD78A16F6D}";

            public const string HpcServerV1_X64 = "{01493E6E-2473-4DE5-963B-BF17BACC21C3}";
            public const string HpcServerV1_X86 = "{C87F3322-A8A0-4239-A81C-36292A250593}";
        }

        public class ProductVersions
        {
            /// <summary>
            /// The v4 RTM model version
            /// </summary>
            public static readonly Version V4ModelVersion = new Version(4, 0);

            /// <summary>
            /// The v4 SP1 model version
            /// </summary>
            public static readonly Version V4Sp1ModelVersion = new Version(4, 1);

            /// <summary>
            /// The v4 SP2 model version
            /// </summary>
            public static readonly Version V4Sp2ModelVersion = new Version(4, 2);

            /// <summary>
            /// The v4 SP3 model version
            /// </summary>
            public static readonly Version V4Sp3ModelVersion = new Version(4, 3);

            /// <summary>
            /// The v4 SP4 model version
            /// </summary>
            public static readonly Version V4Sp4ModelVersion = new Version(4, 4);

            /// <summary>
            /// The v4 SP5 model version
            /// </summary>
            public static readonly Version V4Sp5ModelVersion = new Version(4, 5);

            /// <summary>
            /// The QFE for v4 SP5 model version
            /// </summary>
            public static readonly Version V4Sp5_1ModelVersion = new Version(4, 5, 1);

            /// <summary>
            /// HPC 2016 Update 1
            /// </summary>
            public static readonly Version V5Sp1ModelVersion = new Version(5, 1, 0);

            /// <summary>
            /// HPC 2016 Update 1 QFE 1
            /// </summary>
            public static readonly Version V5Sp1QFE1ModelVersion = new Version(5, 1, 1);

            /// <summary>
            /// HPC 2016 Update 2
            /// </summary>
            public static readonly Version V5Sp2ModelVersion = new Version(5, 2, 0);

            /// <summary>
            /// The latest model version, it should be updated if new version added
            /// </summary>
            public static readonly Version LatestModelVersion = V5Sp2ModelVersion;
        }

        public class ErrorCodes
        {
            public const int ForceNodeManagerResync = -2;
        }
    }
}
