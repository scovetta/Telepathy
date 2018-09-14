using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    public class JobProfilePropertyBag : ProfileValidation.IProfileItemQuery
    {
        Dictionary<PropertyId, ClusterJobProfileItem>   _existing = null;
        Dictionary<PropertyId, StoreProperty>           _props = new Dictionary<PropertyId,StoreProperty>();
        Dictionary<PropertyId, ClusterJobProfileItem>   _items = new Dictionary<PropertyId,ClusterJobProfileItem>();
        private bool _inSchedulerProc;  // true if executing in scheduler process

        private JobProfilePropertyBag()
        {
        }

        public JobProfilePropertyBag(bool inSchedulerProc)
        {
            _inSchedulerProc = inSchedulerProc;
        }
        
        public void SetItem(ClusterJobProfileItem item)
        {
            _items[item.PropId] = item;
        }
        
        public void RemoveItem(PropertyId pid)
        {
            if (_items.ContainsKey(pid))
            {
                _items.Remove(pid);
            }
        }
        
        public bool ContainsProperty(PropertyId pid)
        {
            return _items.ContainsKey(pid);
        }

        public ICollection<ClusterJobProfileItem> Items
        {
            get { return _items.Values; }
        }

        public void SetProps(params StoreProperty[] props)
        {
            if (props != null)
            {
                foreach (StoreProperty prop in props)
                {
                    _props[prop.Id] = prop;
                }
            }
        }

        public PropertyRow GetProps(params PropertyId[] pids)
        {
            List<StoreProperty> result = new List<StoreProperty>();
            
            if (pids != null)
            {
                foreach (PropertyId pid in pids)
                {
                    StoreProperty prop;
                    if (_props.TryGetValue(pid, out prop))
                    {
                        result.Add(prop);
                    }
                    else
                    {
                        result.Add(new StoreProperty(StorePropertyIds.Error, PropertyError.NotFound));
                    }
                }
            }
            else
            {
                result.AddRange(_props.Values);
            }
            
            return new PropertyRow(result.ToArray());
        }

        public ClusterJobProfileItem GetItemForValidation(PropertyId pid)
        {
            ClusterJobProfileItem item = null;
            
            // Changed items trump existing properties
            
            if (_items.TryGetValue(pid, out item))
            {
                return item;
            }
            
            // Check to see if we have this item already in the profile.
            
            if (_existing.TryGetValue(pid, out item))
            {
                return item;
            }
            
            // Item does not exist currently.
            
            return null;
        }

        public IClusterJobProfile Create(ISchedulerStore store, string name)
        {
            if (name != null)
            {
                name = name.Trim();
            }
            
            // Make sure that we have a name.

            if (string.IsNullOrEmpty(name))
            {
                StoreProperty prop = null;
                
                if (_props.TryGetValue(JobTemplatePropertyIds.Name, out prop) && prop.Value != null)
                {                
                    name = prop.Value.ToString().Trim();
                }
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new SchedulerException(ErrorCode.Operation_MustProvideProfileName, "");
            }

            // Check to see if we have a valid set of 
            // constraints that will work.
            
            Validate(null);

            // Create the profile
            
            IClusterJobProfile profile = store.OpenStoreManager().CreateProfile(name);

            if (_props.ContainsKey(JobTemplatePropertyIds.Name))
            {
                _props.Remove(JobTemplatePropertyIds.Name);
            }

            if (_props.Count > 0)
            {
                List<StoreProperty> props = new List<StoreProperty>(_props.Values);
                
                profile.SetProps(props.ToArray());
            }
            
            // Now call replace, which will write all the new values.
            
            profile.ReplaceProfileItems(_items.Values);
            
            return profile;
        }
        
        public void Replace(IClusterJobProfile profile)
        {
            Validate(null);
            
            profile.ReplaceProfileItems(_items.Values);
        }
        
        public void Update(IClusterJobProfile profile)
        {
            Validate(profile.GetProfileItems());
            
            // Add code to update all items in one call to server
            
            throw new NotImplementedException();
        }
        
        void Validate(IEnumerable<ClusterJobProfileItem> existingItems)
        {
            _existing = new Dictionary<PropertyId,ClusterJobProfileItem>();

            if (existingItems != null)
            {
                foreach (ClusterJobProfileItem item in existingItems)
                {
                    _existing[item.PropId] = item;
                }
            }
            
            ProfileValidation validation = new ProfileValidation(_inSchedulerProc);
            
            foreach (ClusterJobProfileItem newItem in _items.Values)
            {
                CallResult cr = validation.ValidateItem(this, newItem);
                if (cr.Code != ErrorCode.Success)
                {
                    cr.Throw();
                }
            }
        }

        public void ReadXML(XmlReader reader, XmlImportOptions options)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == XmlNames.JobProfile)
                {
                    string name = reader.GetAttribute(JobTemplatePropertyIds.Name.Name);
                    
                    if (string.IsNullOrEmpty(name) == false)
                    {
                        _props[JobTemplatePropertyIds.Name] = new StoreProperty(JobTemplatePropertyIds.Name, name);
                    }
                    
                    string description = reader.GetAttribute(JobTemplatePropertyIds.Description.Name);
                    
                    if (string.IsNullOrEmpty(description) == false)
                    {
                        _props[JobTemplatePropertyIds.Description] = new StoreProperty(JobTemplatePropertyIds.Description, description);
                    }
                } 
                else if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == XmlNames.JobProfile)
                {
                    break;
                }
                else if (reader.LocalName == XmlNames.ProfileItem)
                {
                    ClusterJobProfileItem item = ClusterJobProfileItem.RestoreFromXml(reader);

                    if (item == null)
                    {
                        // Ignore it. This is for patch the SoftwareLicense invalid format
                        // See V3 bug 7852

                    }
                    else
                    {
                        _items[item.PropId] = item;
                    }
                }
            }
        }

    }
}
