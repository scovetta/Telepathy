using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    [Flags]
    public enum XmlExportOptions
    {
        None                        = 0,
        VersionOneCompatible        = 0x01,
        AllProperties               = 0x02,
        NoTasks                     = 0x08,
        NoJobElement                = 0x10,
        NoTaskElement               = 0x20,
    }

    [Flags]
    public enum XmlImportOptions
    {
        None                        = 0,
        NoTasks                     = 0x01,
        NoTaskGroups                = 0x02,
        UpdateJobTemplateItems      = 0x04,
    }

    public interface IClusterStoreObject
    {
        int Id { get; }
        
        PropertyRow GetProps(params PropertyId[] propertyIds);

        PropertyRow GetPropsByName(params string[] propertyNames);

        PropertyRow GetAllProps();

        void SetProps(params StoreProperty[] properties);
        
        void PersistToXml(XmlWriter writer, XmlExportOptions flags);

        void RestoreFromXml(XmlReader reader, XmlImportOptions flags);
    }
}
