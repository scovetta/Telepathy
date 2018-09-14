using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    public interface IClusterJobProfile : IClusterStoreObject
    {
        IClusterJobProfile Clone(string profileNameNew);
        
        ClusterJobProfileItem GetProfileItemForPropId(PropertyId propertyId);
        
        ClusterJobProfileItem[] GetProfileItems();
    
        void AddProfileItem(ClusterJobProfileItem item);
        
        void ModifyProfileItem(ClusterJobProfileItem item);
        
        void DeleteProfileItem(PropertyId propId);

        bool ValidateJobProperty(StoreProperty jobProperty);
        
        void ValidateJobPropertyWithThrow(StoreProperty jobProperty);
        
        void ReplaceProfileItems(IEnumerable<ClusterJobProfileItem> items);
        
    }
}
