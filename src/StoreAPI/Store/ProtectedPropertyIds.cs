using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Hpc.Scheduler.Properties;

/// <summary>
/// This file contains property IDs visible to the Store API and the scheduler, but not visible to the public APIs
/// </summary>
    
namespace Microsoft.Hpc.Scheduler.Store
{
    public class ProtectedStorePropertyIds
    {
        private ProtectedStorePropertyIds() {}
    }

    public class ProtectedJobPropertyIds
    {
        private ProtectedJobPropertyIds() {}

        public static PropertyId HasNodePrepTask
        {
            get { return _HasNodePrepTask; }
        }

        public static PropertyId HasNodeReleaseTask
        {
            get { return _HasNodeReleaseTask; }
        }

        public static PropertyId HasServiceTask
        {
            get { return _HasServiceTask; }
        }

        public static PropertyId ComputedProgress
        {
            get { return _ComputedProgress; }
        }

        public static PropertyId IsProgressAutoCalculated
        {
            get { return _IsProgressAutoCalculated; }
        }

        public static PropertyId ProfileMinResources
        {
            get { return _ProfileMinResources; }
        }

        public static PropertyId ProfileMaxResources
        {
            get { return _ProfileMaxResources; }
        }

        public static PropertyId ActivationFilters
        {
            get { return _ActivationFilters; }
        }

        public static PropertyId SubmissionFilters
        {
            get { return _SubmissionFilters; }
        }

        /// <summary>
        /// This property is used only to transfer the user preference for reusing the credential between
        /// the client and the server
        /// </summary>
        public static PropertyId ReuseCredentials
        {
            get { return _ReuseCredential; }
        }

        static PropertyId _HasNodePrepTask          = new PropertyId(StorePropertyType.Boolean, "HasNodePrepTask",          PropertyIdConstants.ProtectedJobPropertyIdStart + 1);
        static PropertyId _HasNodeReleaseTask       = new PropertyId(StorePropertyType.Boolean, "HasNodeReleaseTask",       PropertyIdConstants.ProtectedJobPropertyIdStart + 2);
        static PropertyId _HasServiceTask           = new PropertyId(StorePropertyType.Boolean, "HasServiceTask",           PropertyIdConstants.ProtectedJobPropertyIdStart + 3);

        static PropertyId _ComputedProgress         = new PropertyId(StorePropertyType.Int32,   "ComputedProgress",         PropertyIdConstants.ProtectedJobPropertyIdStart + 4,  PropFlags.Calculated);
        static PropertyId _IsProgressAutoCalculated = new PropertyId(StorePropertyType.Boolean, "IsProgressAutoCalculated", PropertyIdConstants.ProtectedJobPropertyIdStart + 5,  PropFlags.Calculated);
        static PropertyId _ProfileMinResources      = new PropertyId(StorePropertyType.Int32, "ProfileMinResources",        PropertyIdConstants.ProtectedJobPropertyIdStart + 6);
        static PropertyId _ProfileMaxResources      = new PropertyId(StorePropertyType.Int32, "ProfileMaxResources",        PropertyIdConstants.ProtectedJobPropertyIdStart + 7);
        
        static PropertyId _ReuseCredential          = new PropertyId(StorePropertyType.Boolean, "ReuseCredentials",         PropertyIdConstants.ProtectedJobPropertyIdStart + 8);

        static PropertyId _ActivationFilters         = new PropertyId(StorePropertyType.StringList, "ActivationFilters",      PropertyIdConstants.ProtectedJobPropertyIdStart + 9);
        static PropertyId _SubmissionFilters         = new PropertyId(StorePropertyType.StringList, "SubmissionFilters",      PropertyIdConstants.ProtectedJobPropertyIdStart + 10);
    }


    public class ProtectedTaskPropertyIds
    {
        private ProtectedTaskPropertyIds() {}

        public static PropertyId RecordId
        {
            get { return _RecordId; }
        }

        public static PropertyId ParametricRunningCount
        {
            get { return _ParametricRunningCount; }
        }

        public static PropertyId ParametricCanceledCount
        {
            get { return _ParametricCanceledCount; }
        }

        public static PropertyId ParametricFailedCount
        {
            get { return _ParametricFailedCount; }
        }

        public static PropertyId ParametricQueuedCount
        {
            get { return _ParametricQueuedCount; }
        }

        public static PropertyId ParametricFinishedCount
        {
            get { return _ParametricFinishedCount; }
        }

        public static PropertyId ParametricConfiguringCount
        {
            get { return _ParametricConfiguringCount; }
        }

        public static PropertyId InstantiatedSubTaskCount
        {
            get { return _InstantiatedSubTaskCount; }
        }

        public static PropertyId ViewTasksStateFilter
        {
            get { return _DisplayFilterState; }
        }

        public static PropertyId ParametricTotalCount
        {
            get { return _ParametricTotalCount; }
        }

        public static PropertyId ParametricDispatchingCount
        {
            get { return _ParametricDispatchingCount; }
        }

        public static PropertyId ParametricFinishingCount
        {
            get { return _ParametricFinishingCount; }
        }

        public static PropertyId ParametricCancelingCount
        {
            get { return _ParametricCancelingCount; }
        }

        public static PropertyId ParametricSubmittedCount
        {
            get { return _ParametricSubmittedCount; }
        }

        public static PropertyId ParametricValidatingCount
        {
            get { return _ParametricValidatingCount; }
        }

        public static PropertyId ParametricFailedNonPreemptionCount
        {
            get { return _ParametricFailedNonPreemptionCount; }
        }


        static PropertyId _RecordId                = new PropertyId(StorePropertyType.Int32, "RecordId",                 PropertyIdConstants.ProtectedTaskPropertyIdStart + 1);

        static PropertyId _ParametricRunningCount  = new PropertyId(StorePropertyType.Int32, "ParametricRunningCount",   PropertyIdConstants.ProtectedTaskPropertyIdStart + 2, PropFlags.Calculated);
        static PropertyId _ParametricCanceledCount = new PropertyId(StorePropertyType.Int32, "ParametricCanceledCount",  PropertyIdConstants.ProtectedTaskPropertyIdStart + 3, PropFlags.Calculated);
        static PropertyId _ParametricFailedCount   = new PropertyId(StorePropertyType.Int32, "ParametricFailedCount",    PropertyIdConstants.ProtectedTaskPropertyIdStart + 4, PropFlags.Calculated);
        static PropertyId _ParametricQueuedCount   = new PropertyId(StorePropertyType.Int32, "ParametricQueuedCount",    PropertyIdConstants.ProtectedTaskPropertyIdStart + 5, PropFlags.Calculated);
        static PropertyId _ParametricFinishedCount = new PropertyId(StorePropertyType.Int32, "ParametricFinishedCount",  PropertyIdConstants.ProtectedTaskPropertyIdStart + 6, PropFlags.Calculated);
        static PropertyId _ParametricConfiguringCount = new PropertyId(StorePropertyType.Int32, "ParametricConfiguringCount",  PropertyIdConstants.ProtectedTaskPropertyIdStart + 7, PropFlags.Calculated);
        
        static PropertyId _InstantiatedSubTaskCount   = new PropertyId(StorePropertyType.Int32, "InstantiatedSubTaskCount", PropertyIdConstants.ProtectedTaskPropertyIdStart + 8, PropFlags.Calculated);

        static PropertyId _DisplayFilterState      = new PropertyId(StorePropertyType.TaskState, "DisplayFilterState",   PropertyIdConstants.ProtectedTaskPropertyIdStart + 9, PropFlags.Calculated);

        static PropertyId _ParametricTotalCount = new PropertyId(StorePropertyType.Int32, "ParametricTotalCount", PropertyIdConstants.ProtectedTaskPropertyIdStart + 10, PropFlags.Calculated);
        
        static PropertyId _ParametricDispatchingCount = new PropertyId(StorePropertyType.Int32, "ParametricDispatchingCount", PropertyIdConstants.ProtectedTaskPropertyIdStart + 11, PropFlags.Calculated);
        static PropertyId _ParametricFinishingCount = new PropertyId(StorePropertyType.Int32, "ParametricFinishingCount", PropertyIdConstants.ProtectedTaskPropertyIdStart + 12, PropFlags.Calculated);
        static PropertyId _ParametricCancelingCount = new PropertyId(StorePropertyType.Int32, "ParametricCancelingCount", PropertyIdConstants.ProtectedTaskPropertyIdStart + 13, PropFlags.Calculated);
        static PropertyId _ParametricSubmittedCount = new PropertyId(StorePropertyType.Int32, "ParametricSubmittedCount", PropertyIdConstants.ProtectedTaskPropertyIdStart + 14, PropFlags.Calculated);
        static PropertyId _ParametricValidatingCount = new PropertyId(StorePropertyType.Int32, "ParametricValidatingCount", PropertyIdConstants.ProtectedTaskPropertyIdStart + 15, PropFlags.Calculated);

        static PropertyId _ParametricFailedNonPreemptionCount = new PropertyId(StorePropertyType.Int32, "ParametricFailedNonPreemptionCount", PropertyIdConstants.ProtectedTaskPropertyIdStart + 16, PropFlags.Calculated);
    }

    public class ProtectedResourcePropertyIds
    {
        private ProtectedResourcePropertyIds() {}
    }

    public class ProtectedJobTemplatePropertyIds
    {
        private ProtectedJobTemplatePropertyIds() {}
    }

    public class ProtectedNodePropertyIds
    {

        private ProtectedNodePropertyIds() {}

        /// <summary>
        /// This is for scheduler on Azure only
        /// </summary>
        public static PropertyId RoleName
        {
            get { return _roleName; }
        }

        static PropertyId _roleName = new PropertyId(StorePropertyType.String, "RoleName", PropertyIdConstants.ProtectedNodePropertyIdStart + 1);
    }

    public class ProtectedAllocationPropertyIds
    {
        private ProtectedAllocationPropertyIds() {} 
    }

    public class ProtectedJobMessagePropertyIds
    {
        private ProtectedJobMessagePropertyIds() {}
    }

    public class ProtectedPoolPropertyIds
    {
        private ProtectedPoolPropertyIds() { }
    }

}
