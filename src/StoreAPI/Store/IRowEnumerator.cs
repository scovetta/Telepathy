using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    public interface IRowEnumerator : IEnumerable<PropertyRow>, IDisposable
    {
        void SetColumns(params PropertyId[] columnIds);

        void SetFilter(params FilterProperty[] filter);

        void SetSortOrder(params SortProperty[] sort);

        PropertyRowSet GetRows2(int numberOfRows);

        void SetOptions(int options);
        
        void SetProperties(params StoreProperty[] properties);
    }
}
