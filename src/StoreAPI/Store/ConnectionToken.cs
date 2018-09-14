using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Store
{

    [Serializable]
    public class ConnectionToken
    {
        public ConnectionToken(Guid id)
        {
            _id = id;
        }

        public Guid ID
        {
            get { return _id; }
        }

        Guid _id;

        public string UserSid { get; set; }

        public string UserName
        {
            get { return _username; }
            set { _username = value; }
        }

        string _username = "n/a";

        public override bool Equals(object obj)
        {
            ConnectionToken other = obj as ConnectionToken;
            if (other != null)
            {
                return _id.Equals(other._id);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }
}

