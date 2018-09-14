using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    internal class StoreTransactionWrapper : IClusterStoreTransaction
    {
        StoreTransaction _transaction;
        
        public StoreTransaction Transaction
        {
            get { return _transaction; }
        }

        SchedulerStoreSvc _owner;

        public StoreTransactionWrapper(int threadId, SchedulerStoreSvc owner)
        {
            _transaction = new StoreTransaction(threadId);
            _owner = owner;
        }

        public void Dispose()
        {
            _owner.CancelTransaction(this);
        }

        public void Commit()
        {
            _owner.RunTransaction(this);
        }

        public void Detach()
        {
            _owner.DetachTransaction(this);
        }

        public void Attach()
        {
            _owner.AttachTransaction(this);
        }

        public override string ToString()
        {
            return _transaction.ToString();
        }
    }
    
    [Serializable]
    public class StoreTransaction
    {
        [Serializable]
        public enum Action 
        {
            Nothing             = 0,
            SetProps            = 1,
            ScheduleResource    = 2,
            SetAllocationStats  = 3,
            ScheduleResourceOnPhantom = 4,
        }
        
        [Serializable]
        public class TransactionItem : IComparable<TransactionItem>
        {
            public TransactionItem(Action type, Int32 id, StoreProperty[] props)
            {
                _action = type;
                _id1 = id;
                _props = props;
            }
            
            public TransactionItem(Action type, Int32 id, Int32 id2, StoreProperty[] props)
            {
                _action = type;
                _id1 = id;
                _id2 = id2;
                _props = props;
            }

            public TransactionItem(Action type, Int32 id, Int32 id2, Int32 id3, StoreProperty[] props)
            {
                _action = type;
                _id1 = id;
                _id2 = id2;
                _id3 = id3;
                _props = props;
            }

            public TransactionItem(ObjectType obType, Int32 id, StoreProperty[] props)
            {
                _action = Action.SetProps;
                _obType = obType;
                _id1 = id;
                _props = props;
            }

            Action              _action;
            ObjectType          _obType;
            StoreProperty[]     _props;
            Int32               _id1 = 0;
            Int32               _id2 = 0;
            Int32               _id3 = 0;
            
            public Action ActionType
            {
                get { return _action; }
            }
            
            public ObjectType ObType 
            {
                get { return _obType; }
            }
            
            public StoreProperty[] Properties
            {
                get { return _props; }
            }
            
            public Int32 ObjectId
            {
                get { return _id1; }
            }
            
            public Int32 ObjectId2
            {
                get { return _id2; }
            }

            public Int32 ObjectId3
            {
                get { return _id3; }
            }

            public override string ToString()
            {
                StringBuilder bldr = new StringBuilder(255);
                
                bldr.Append(ActionType.ToString());
                
                bldr.Append(": ");

                if (ObType != ObjectType.None)
                {
                    bldr.Append(ObType.ToString());
                    bldr.Append(": ");
                }

                switch (ActionType)
                {
                    case Action.ScheduleResource:
                    {
                        bldr.AppendFormat("resource ID: {0}, Job ID: {1}, ", _id1, _id2);
                        break;
                    }
                    case Action.SetAllocationStats:
                    {
                        bldr.AppendFormat("Node ID: {0}, Job ID: {1}, Task ID: {2}, ", _id1, _id2, _id3);
                        break;
                    }
                    case Action.ScheduleResourceOnPhantom:
                    {
                        bldr.AppendFormat("Job ID: {0}, Node ID: {1}, ", _id1, _id2);
                        break;
                    }

                    default:
                    {
                        bldr.AppendFormat("ID: {0}, ", _id1);
                        break;
                    }
                }

                    
                foreach (StoreProperty prop in Properties)
                {
                    bldr.Append(prop.ToString());
                    bldr.Append(", ");
                }
                
                return bldr.ToString();
            }

            public int CompareTo(TransactionItem other)
            {
                int result = (int)(this._action) - (int)(other._action);
                
                if (result == 0)
                {
                    result = (int)(this._obType) - (int)(other._obType);
                }
                
                return result;
            }

        }
        
        List<TransactionItem> _items = new List<TransactionItem>();
        
        int _threadid;
        
        public StoreTransaction(int threadId)
        {
            _threadid = threadId;
        }
        
        public int ThreadId 
        {
            get { return _threadid; }
        }

        public List<TransactionItem> Items
        {
            get { return _items; }
        }

        public void SortItems()
        {
            _items.Sort();
        }
        
        public void SetObjectProps(ObjectType obType, Int32 itemId, StoreProperty[] props)
        {
            _items.Add(new TransactionItem(obType, itemId, props));
        }
        
        public void ScheduleJob(int resourceId, int jobId, StoreProperty[] jobProperties)
        {
            _items.Add(new TransactionItem(Action.ScheduleResource, resourceId, jobId, jobProperties));
        }

        public void UpdateTaskNodeStats(int nodeId, int jobId, int taskId, StoreProperty[] props)
        {
            _items.Add(new TransactionItem(Action.SetAllocationStats, nodeId, jobId, taskId, props));
        }

        public void ScheduleJobOnPhantomResource(int jobId, int nodeId, StoreProperty[] phantomResourceProperties)
        {
            _items.Add(new TransactionItem(Action.ScheduleResourceOnPhantom,jobId,nodeId,phantomResourceProperties));
        }


        public override string ToString()
        {
            return "Thrd: " + _threadid.ToString() + " - " + _items.Count.ToString() + " items";
        }
    }
}
