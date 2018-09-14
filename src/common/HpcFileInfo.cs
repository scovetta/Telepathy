using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Hpc.Management.FileTransfer
{
    [Serializable]
    public class HpcFileInfo
    {
        public HpcFileInfo(string fileName, DateTime creationTimeUtc, DateTime lastWriteTimeUtc)
        {
            this.Name = fileName;
            this.LastWriteTimeUtc = lastWriteTimeUtc;
            this.CreationTimeUtc = creationTimeUtc;
        }

        public string Name
        {
            get;
            internal set;
        }

        public DateTime LastWriteTimeUtc
        {
            get;
            internal set;
        }

        public DateTime CreationTimeUtc
        {
            get;
            internal set;
        }
    }
}
