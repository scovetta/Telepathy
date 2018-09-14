//------------------------------------------------------------------------------
// <copyright file="SchedulerPropertyIdConstants.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">sayanch</owner>
// <securityReview name="" date=""/>
//------------------------------------------------------------------------------



using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Properties
{
    internal class PropertyIdConstants
    {
        public const int JobPropertyIdStart = 1000;
        public const int ProtectedJobPropertyIdStart = 1400;
        public const int PrivateJobPropertyIdStart = 1000000;

        public const int JobHistoryPropertyIdStart = 1500;
        public const int ProtectedJobHistoryPropertyIdStart = 1700;
        public const int PrivateJobHistoryPropertyIdStart = 1001000;

        public const int TaskPropertyIdStart = 2000;
        public const int ProtectedTaskPropertyIdStart = 2400;
        public const int PrivateTaskPropertyIdStart = 1002000;

        public const int ResourcePropertyIdStart = 3000;
        public const int ProtectedResourcePropertyIdStart = 3200;
        public const int PrivateResourcePropertyIdStart = 1003000;

        public const int JobMessagePropertyIdStart = 3500;
        public const int ProtectedJobMessagePropertyIdStart = 3700;
        public const int PrivateJobMessagePropertyIdStart = 1004000;

        public const int ProfilePropertyIdStart = 5000;
        public const int ProtectedProfilePropertyIdStart = 5100;
        public const int PrivateProfilePropertyIdStart = 1005000;

        public const int TaskGroupPropertyIdStart = 6000;
        public const int ProtectedTaskGroupPropertyIdStart = 6100;
        public const int PrivateTaskGroupPropertyIdStart = 10006000;

        public const int NodePropertyIdStart = 8000;
        public const int ProtectedNodePropertyIdStart = 8200;
        public const int PrivateNodePropertyIdStart = 1007000;

        public const int NodeHistoryPropertyIdStart = 8500;
        public const int ProtectedNodeHistoryPropertyIdStart = 8700;
        public const int PrivateNodeHistoryPropertyIdStart = 1008000;

        public const int AllocationPropertyIdStart = 9000;
        public const int ProtectedAllocationPropertyIdStart = 9200;
        public const int PrivateAllocationPropertyIdStart = 1009000;

        public const int PoolPropertyIdStart = 9500;
        public const int ProtectedPoolPropertyIdStart = 9700;
        public const int PrivatePoolPropertyIdStart = 1010000;

        public const int AzureDeploymentPropertyIdStart = 9800;
        public const int ProtectedAzureDeploymentPropertyIdStart = 9900;
        public const int PrivateAzureDeploymentPropertyIdStart = 1011000;
    }

}
