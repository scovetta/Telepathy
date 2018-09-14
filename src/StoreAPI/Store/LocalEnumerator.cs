using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    public class RowSetEnumerator : IEnumerator<PropertyRow>
    {
        internal RowSetEnumerator(LocalRowSet owner)
        {
            _owner = owner;
        }

        LocalRowSet     _owner;
        PropertyRow[]   _cacheRows = null;
        int             _index = 0;

        public void Dispose()
        {
            // Suppress finalization of this disposed instance.
            GC.SuppressFinalize(this);
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public PropertyRow Current
        {
            get
            {
                Debug.Assert(_cacheRows != null);
                return _cacheRows[_index];
            }
        }

        public bool MoveNext()
        {
            if (_cacheRows != null)
            {
                ++_index;

                if (_index < _cacheRows.GetLength(0))
                {
                    return true;
                }
            }

            _cacheRows = _owner.GetRows(128);

            if (_cacheRows == null || _cacheRows.GetLength(0) == 0)
            {
                return false;
            }

            _index = 0;

            return true;
        }

        public void Reset()
        {
            _owner.Seek(SeekMethod.Begin, 0);
            _cacheRows = null;
            _index = 0;
        }
    }


}
