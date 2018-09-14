using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    [Serializable]
    public class ServerPropertyDescriptor
    {
        PropertyId _pid;
        string     _name;
        ObjectType _obtype;
        string     _description;
        
        public ServerPropertyDescriptor(
            PropertyId pid,
            ObjectType obType,
            string name,
            string description
            )
        {
            _pid = pid;
            _obtype = obType;
            _name = name;
            _description = description;
        }

        public PropertyId Id
        {
            get { return _pid; }
        }
        
        public ObjectType Type
        {
            get { return _obtype; }
        }
        
        public string Name
        {
            get { return _name; }
        }
        
        public string Description
        {
            get { return _description; }
        }
    }
}
