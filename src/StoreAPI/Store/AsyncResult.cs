using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    public class AsyncResult : IAsyncResult
    {
        object           _param;
        WaitHandle      _waithandle;
        bool            _fComplete = false;
        int             _jobResultState = 0;
        int             _stateMask = 0;
        Int32           _objectId;
        AsyncCallback   _callback;
        bool            _fExpired = false;

        ObjectType _objecttype;

        public AsyncResult(ObjectType objectType, Int32 objectId, int stateMask, AsyncCallback callback, object param)
        {
            _objecttype = objectType;
            
            _stateMask = stateMask;
            
            _objectId = objectId;
            
            _param = param;
            
            _callback = callback;
            
            _waithandle = new ManualResetEvent(false);
        }

        SchedulerStoreSvc _owner;

        internal void RegisterForEvent(SchedulerStoreSvc owner)
        {
            _owner = owner;
            
            switch (_objecttype)
            {
                case ObjectType.Job:
                    _owner.JobEvent += this.JobEvent;
                    break;
                    
                case ObjectType.Task:
                    _owner.TaskEvent += this.TaskEvent;
                    break;
            }
        }

        public ObjectType objectType 
        {
            get { return _objecttype; }
        }

        int _closed = 0;

        public void Close()
        {
            if (Interlocked.CompareExchange(ref _closed, 1, 0) == 0)
            {
                _fExpired = true;

                _waithandle.Close();
                _waithandle = null;

                if (_owner != null)
                {
                    switch (_objecttype)
                    {
                        case ObjectType.Job:
                            _owner.JobEvent -= this.JobEvent;
                            break;

                        case ObjectType.Task:
                            _owner.TaskEvent -= this.TaskEvent;
                            break;
                    }
                }
            }
        }

        public bool DoesStateMatch(JobState state)
        {
            if (((int)state & _stateMask) != 0)
            {
                return true;
            }
            
            return false;
        }

        public bool DoesStateMatch(TaskState state)
        {
            if (((int)state & _stateMask) != 0)
            {
                return true;
            }

            return false;
        }

        public void Invoke()
        {
            if (_callback != null)
            {
                _callback.Invoke(this);
            }
        }

        public Int32 JobId
        {
            get { return _objectId; }
        }

        public int ResultState 
        {
            get { return _jobResultState; }
            set { _jobResultState = value; }
        }

        public object AsyncState
        {
            get { return _param; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return _waithandle; }
        }

        public bool CompletedSynchronously
        {
            get { return false; }
        }

        public bool IsCompleted
        {
            get { return _fComplete; }
        }

        internal bool IsExpired
        {
            get { return _fExpired; }
            set { _fExpired = value; }
        }

        void JobEvent(Int32 jobId, EventType eventType, StoreProperty[] props)
        {
            // This is just a holder to make sure that events for jobs
            // are registered with the server while the async request is
            // open.
        }

        void TaskEvent(Int32 jobId, Int32 taskId, TaskId taskId2, EventType eventType, StoreProperty[] props)
        {
            // This is just a holder to make sure that events for tasks
            // are registered with the server while the async request is
            // open.
        }
        


    }
}
