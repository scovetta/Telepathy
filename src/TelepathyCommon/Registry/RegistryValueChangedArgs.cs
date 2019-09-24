// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Common.Registry
{
    using System;

    public class RegistryValueChangedArgs<T> : EventArgs
    {
        public RegistryValueChangedArgs(ChangeType type, T oldValue, T newValue)
        {
            this.ValueChangeType = type;
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        public enum ChangeType
        {
            Created = 0,

            Modified = 1,

            Deleted = 2
        }

        public T NewValue { get; }

        public T OldValue { get; }

        public ChangeType ValueChangeType { get; }
    }
}