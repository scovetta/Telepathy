namespace Microsoft.Hpc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class RegistryValueChangedArgs<T> : EventArgs
    {
        public enum ChangeType
        {
            Created = 0,
            Modified = 1,
            Deleted = 2,
        }

        public RegistryValueChangedArgs(ChangeType type, T oldValue, T newValue)
        {
            this.ValueChangeType = type;
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        public ChangeType ValueChangeType
        {
            get; private set;
        }

        public T OldValue { get; private set; }

        public T NewValue { get; private set; }
    }
}
