using System;

namespace TelepathyCommon.Registry
{
    public class ReliableProperty
    {
        public ReliableProperty(string name, Type valueType, bool readOnly = false)
            : this(name, valueType, HpcConstants.HpcFullKeyName, readOnly)
        {
        }

        public ReliableProperty(string name, Type valueType, string parentName, bool readOnly = false)
        {
            this.Name = name;
            this.ParentName = parentName;
            this.ValueType = valueType;
            this.ReadOnly = readOnly;
        }

        public string Name { get; set; }

        public string ParentName { get; set; }

        public Type ValueType { get; set; }

        public bool ReadOnly { get; set; }
    }
}
