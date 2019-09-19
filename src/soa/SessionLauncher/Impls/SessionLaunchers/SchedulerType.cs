// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal enum SchedulerType
    {
        Unknown = 0,
        Local = 1,
        HpcPack = 2,
        AzureBatch = 3,
    }
}
