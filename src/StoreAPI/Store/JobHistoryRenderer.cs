namespace Microsoft.Hpc.Scheduler.Store
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Security.Principal;
    using Microsoft.Hpc.Scheduler.Properties;

    public class JobHistoryRenderer
    {

        internal class NodeRecord
        {
            internal int        RefCount = 0;
            internal string     Name = null;
            internal int        Id = 0;
            internal int        OriginalAllocation = 0;
            internal int        LastRefCount = 0;
            internal AllocationEvent LastEvent = null;
        }

        internal class AllocationEvent
        {
            internal enum EventType { None,StartEvent, EndEvent};
            
            EventType _type = EventType.None;            
            int _nodeId = 0;
            string _nodeName = null;
            DateTime _eventTime = DateTime.MinValue;
            int _resourceId=0;
            int _requeueCount = 0;
            

            internal EventType Type { get { return _type; } }
            internal int NodeId { get { return _nodeId; } }
            internal string NodeName { get {return _nodeName;} }
            internal DateTime EventTime { get {return _eventTime;}  }
            internal int ResourceId { get { return _resourceId; } }
            internal int RequeueCount { get { return _requeueCount; } }
            


            internal AllocationEvent(EventType type, int nodeId, string nodeName, DateTime eventTime,int resourceId,int requeueCount)
            {
                _type = type;
                _nodeId = nodeId;
                _nodeName = nodeName;
                _eventTime = eventTime;
                _resourceId = resourceId;
                _requeueCount = requeueCount;
            }
            
        }

        internal class CancelRequestRecord
        {
            string _actorSid = null;
            DateTime _eventTime = DateTime.MinValue;
            int _requeueCount = 0;
            CancelRequest _request = CancelRequest.None;

            internal CancelRequestRecord(string actorSid, DateTime eventTime, int requeueCount,CancelRequest request)
            {
                _actorSid = actorSid;
                _eventTime = eventTime;
                _requeueCount = requeueCount;
                _request = request;
            }

            public override string ToString()
            {
                SecurityIdentifier sid = null;
                string name = string.Empty;
                string formatString = null;
                if (_actorSid != null)
                {
                    sid = new SecurityIdentifier(_actorSid);
                    try
                    {
                        name = (sid.Translate(typeof(NTAccount)) as NTAccount).Value;
                    }
                    catch
                    {
                        //if the SID to name translation fails just continue,
                        //it is not a critical problem since this is on the client
                    }
                    if (name == null)
                    {
                        //if the name translation failed 
                        //just use the SID
                        //This should happen rarely
                        name = _actorSid;                       
                    }
                }
                 

                switch (_request)
                {
                    case CancelRequest.CancelForceByUser:
                        formatString = SR.FormatJobEventCancelRequestForceReceived;
                        break;
                    case CancelRequest.Finish:
                    case CancelRequest.FinishGraceful:
                        formatString = SR.FormatJobEventFinishRequestReceived;
                        break;
                    case CancelRequest.Preemption:
                        return string.Format(SR.FormatJobEventCancelRequestPreemptionReceived, _eventTime.ToLocalTime().ToString());                        
                    case CancelRequest.CancelByUser:
                    case CancelRequest.CancelGraceful:
                        formatString = SR.FormatJobEventCancelRequestReceived;
                        break;
                    default:
                        return null;
                        
                }
                
                return string.Format(formatString, _eventTime.ToLocalTime().ToString(), name);
            }

            internal DateTime EventTime
            {
                get { return _eventTime; }
            }
        }

        internal class AllocationEventComparer : IComparer<AllocationEvent>
        {
            public int Compare(AllocationEvent lhs, AllocationEvent rhs)
            {
                if (lhs.EventTime < rhs.EventTime)
                {
                    return -1;
                }
                if (lhs.EventTime > rhs.EventTime)
                {
                    return 1;
                }
                if (lhs.NodeId < rhs.NodeId)
                {
                    return -1;
                }
                if (lhs.NodeId > rhs.NodeId)
                {
                    return 1;
                }
                if (lhs.Type < rhs.Type)
                {
                    return -1;
                }
                if (lhs.Type > rhs.Type)
                {
                    return 1;
                }
                return 0;
            }
        }

                
            
        Dictionary<int, NodeRecord> _nodes = new Dictionary<int,NodeRecord>();
        
        SortedDictionary<DateTime, List<string>> result = new SortedDictionary<DateTime, List<string>>();
     
        List<AllocationEvent> allEvents = new List<AllocationEvent>();
        int nextEventIdx = 0;
        int closedStartRecordCount = 0, closedEndRecordCount = 0;
        Dictionary<int, int> resourceStartedCount = new Dictionary<int, int>();

        Queue<CancelRequestRecord> cancelRequests = new Queue<CancelRequestRecord>();


        private void collectAllAllocationRecords(IClusterJob job)
        {            
            using (IRowEnumerator rows = job.OpenAllocationEnumerator())
            {
                rows.SetColumns(
                    AllocationProperties.NodeId,
                    AllocationProperties.NodeName,
                    AllocationProperties.StartTime,
                    AllocationProperties.EndTime,
                    AllocationProperties.ResourceId,
                    AllocationProperties.JobRequeueCount
                    );

                rows.SetFilter(
                    new FilterProperty(FilterOperator.Equal, AllocationProperties.TaskId, 0)
                    );


                foreach (PropertyRow row in rows)
                {
                    int nodeId = (int)row[0].Value;
                    string nodeName = (string )row[1].Value;
                    DateTime startTime = (DateTime)row[2].Value;
                    int resourceId = (int)row[4].Value;
                    int requeueCount = (int)row[5].Value;

                    allEvents.Add(new AllocationEvent(AllocationEvent.EventType.StartEvent, nodeId, nodeName, startTime,resourceId,requeueCount));

                    if (row[3].Id == AllocationProperties.EndTime)
                    {
                        DateTime endTime = (DateTime)row[3].Value;
                        
                        allEvents.Add(new AllocationEvent(AllocationEvent.EventType.EndEvent, nodeId, nodeName, endTime,resourceId,requeueCount));
                    }                    
                }
            }
            
            allEvents.Sort(new AllocationEventComparer());
        }
       
        
        void ExamineRun(IClusterJob job, int requeueId,DateTime runStartTime,DateTime runEndTime)
        {
            _nodes.Clear();
            resourceStartedCount.Clear();

            NodeRecord openStartRecord = null, openEndRecord = null; //Variables represent the last nodes that have an ongoing sequence of start and end events
            int oldNextEventIdx = nextEventIdx;

            
            for (; nextEventIdx < allEvents.Count  && allEvents[nextEventIdx].RequeueCount == requeueId && allEvents[nextEventIdx].EventTime <= runEndTime;nextEventIdx++)
            {
                AllocationEvent allocEvent = allEvents[nextEventIdx];
                NodeRecord eventNode;                


                //For the event look up its node, if we can't find it, add the node
                if (!_nodes.TryGetValue(allocEvent.NodeId, out eventNode))
                {
                    eventNode = new NodeRecord();
                    eventNode.Id = allocEvent.NodeId;
                    eventNode.Name = allocEvent.NodeName;
                    _nodes.Add(eventNode.Id, eventNode);
                }
                int startedCount;

                switch (allocEvent.Type)
                {
                    case AllocationEvent.EventType.StartEvent:
                        //Check first if that this resource has not already been started
                       
                        if (!resourceStartedCount.TryGetValue(allocEvent.ResourceId, out startedCount))
                        {
                            startedCount = 0;
                            resourceStartedCount.Add(allocEvent.ResourceId, startedCount);
                        }
                        startedCount++;
                        resourceStartedCount[allocEvent.ResourceId] = startedCount;

                        if (startedCount > 1)
                        {
                            //This resource is already allocated to this job when this start is detected
                            //We will just ignore it since this resource already belongs to this node and has been accounted for
                            //This can happen because a transaction from the job monitor writing the resource table as well as the allocation history 
                            //can interleave with a scheduler thread that reads from the resource table and writes a scheduler decision to the
                            //resource table and allocation history
                            continue;
                        }                       

                        if (openEndRecord != null)
                        {
                            //if a end record is open we need to close it
                            CloseEndRecord(openEndRecord);
                            openEndRecord = null;
                        }
                        //Increment the number of resources allocated to the job on this node
                        eventNode.RefCount++;

                        if (openStartRecord == null)
                        {
                            //if no start record is open, mark this as the new open start record
                            openStartRecord = eventNode;
                        }
                        else
                        {
                            if (openStartRecord.Id != eventNode.Id)
                            {
                                //if the current start event does not belong to the previous open start record
                                // we need to close the old one                                
                                CloseStartRecord(openStartRecord);

                                //Set this as the New open start record
                                openStartRecord = eventNode;
                            }
                        }
                        
                        break;
                    case AllocationEvent.EventType.EndEvent:


                        //Check first if that this resource has already been started                        
                        if (!resourceStartedCount.TryGetValue(allocEvent.ResourceId, out startedCount))
                        {
                            continue;
                        }

                        startedCount--;
                        resourceStartedCount[allocEvent.ResourceId] = startedCount;

                        if (startedCount > 0)
                        {
                            //We had found start events events earlier that did not have matching end events before
                            //another start event showed up. So, this end event just marks the end of one of the unmatched start events.
                            //However since there are more unmatched start events this resource still belongs to this job
                            continue;
                        }

                        if (startedCount < 0)
                        {
                            continue;
                        }

                        if (openStartRecord != null)
                        {
                            //if a start record is already open close it
                            CloseStartRecord(openStartRecord);
                            openStartRecord = null;
                        }

                        eventNode.RefCount--;

                        if (openEndRecord == null)
                        {
                            //if no end record is open, mark this as the new open end record
                            openEndRecord = eventNode;
                        }
                        else
                        {
                            if (openEndRecord.Id != eventNode.Id)
                            {
                                //this end event does not belong to the previously open end record 
                                // so close the previously open one
                                CloseEndRecord(openEndRecord);

                                //The node of the current end event is the new open end record
                                openEndRecord = eventNode;
                            }
                        }
                        break;                    
                    default:
                        Debug.Assert(false,"AllocationEvent of type None detected");
                        break;
                }

                eventNode.LastEvent = allocEvent;

            }
    
            if (openStartRecord != null)
            {
                CloseStartRecord(openStartRecord);
            }
            if (openEndRecord != null)
            {
                CloseEndRecord(openEndRecord);
            }            
        }

        private void CloseEndRecord(NodeRecord openEndRecord)
        {
            Debug.Assert(openEndRecord.LastEvent != null);

            OutputCancelRequests(openEndRecord.LastEvent.EventTime);

            if (openEndRecord.RefCount == 0)
            {
                //allocation on this node is finished
                AddResult(openEndRecord.LastEvent.EventTime, string.Format(SR.FormatJobEndedOnNode, openEndRecord.LastEvent.EventTime.ToLocalTime().ToString(), openEndRecord.Name));
            }
            else
            {
                //allocation on this node is reduced
                AddResult(openEndRecord.LastEvent.EventTime, string.Format(SR.FormatAllocationReduced, openEndRecord.LastEvent.EventTime.ToLocalTime().ToString(), openEndRecord.Name, openEndRecord.RefCount));
            }
            openEndRecord.LastRefCount = openEndRecord.RefCount;
            closedEndRecordCount++;
        }
  

        private void CloseStartRecord(NodeRecord openStartRecord)
        {
            Debug.Assert(openStartRecord.LastEvent != null);

            OutputCancelRequests(openStartRecord.LastEvent.EventTime);


            if (openStartRecord.LastRefCount == 0)
            {
                //new allocation on this node
                AddResult(openStartRecord.LastEvent.EventTime, string.Format(SR.FormatJobStartedOnNode, openStartRecord.LastEvent.EventTime.ToLocalTime().ToString(),openStartRecord.Name,openStartRecord.RefCount));                
            }
            else
            {
                //increasing allocation on this node                
                AddResult(openStartRecord.LastEvent.EventTime, string.Format(SR.FormatAllocationIncreased, openStartRecord.LastEvent.EventTime.ToLocalTime().ToString(), openStartRecord.Name, openStartRecord.RefCount));
            }
            openStartRecord.LastRefCount = openStartRecord.RefCount;
            closedStartRecordCount++;
        }

        private void OutputCancelRequests(DateTime triggerEventTime)
        {
            while (cancelRequests.Count > 0)
            {
                CancelRequestRecord firstRec = cancelRequests.Peek();
                if (firstRec.EventTime < triggerEventTime)
                {
                    CancelRequestRecord rec = cancelRequests.Dequeue();
                    
                    string recString = rec.ToString();
                    if (recString != null)
                    {
                        AddResult(rec.EventTime, recString);
                    }                                        
                }
                else
                {
                    break;
                }
            }
        }

        public void Build(ISchedulerStore store, IClusterJob job)
        {
            List<PropertyRow> jobRunsAndModification = new List<PropertyRow>();

            using (IRowEnumerator rows = store.OpenStoreManager().OpenJobHistoryEnumerator())
            {
                rows.SetColumns(
                    JobHistoryPropertyIds.SubmitTime,
                    JobHistoryPropertyIds.StartTime,
                    JobHistoryPropertyIds.EventTime,
                    JobHistoryPropertyIds.JobEvent,
                    JobHistoryPropertyIds.RequeueId,
                    JobHistoryPropertyIds.ActorSid,
                    JobHistoryPropertyIds.CancelRequest,
                    JobHistoryPropertyIds.Operator,
                    JobHistoryPropertyIds.PropChange
                    );
                    
                rows.SetFilter(
                    new FilterProperty(FilterOperator.Equal, JobHistoryPropertyIds.JobId, job.Id)
                    );
                    
                foreach (PropertyRow row in rows)
                {
                    //if we read a cancelrequest event, we should convert it into an appropriate allocation event
                    //and not add it to a run. The cancelrequestevent is a single event and not a run 
                    JobEvent endEventType = PropertyUtil.GetValueFromPropRow(row, JobHistoryPropertyIds.JobEvent, JobEvent.None);                    
                                        
                    if (endEventType == JobEvent.CancelRequestReceived)
                    {
                        string actorSid = PropertyUtil.GetValueFromPropRow(row,JobHistoryPropertyIds.ActorSid,(string )null);
                        DateTime eventTime = PropertyUtil.GetValueFromPropRow(row, JobHistoryPropertyIds.EventTime, DateTime.MinValue);
                        int requeueCount = PropertyUtil.GetValueFromPropRow(row, JobHistoryPropertyIds.RequeueId, 0);
                        CancelRequest request = PropertyUtil.GetValueFromPropRow(row,JobHistoryPropertyIds.CancelRequest,CancelRequest.None);

                        
                        
                        cancelRequests.Enqueue(new CancelRequestRecord(actorSid,eventTime,requeueCount,request));
                        
                    }
                    else
                    {
                        jobRunsAndModification.Add(row);
                    }
                }
            }
            
            PropertyRow jobRow = job.GetProps(
                    JobPropertyIds.CreateTime, 
                    JobPropertyIds.Owner,
                    JobPropertyIds.State,
                    JobPropertyIds.RequeueCount,
                    JobPropertyIds.SubmitTime,
                    JobPropertyIds.StartTime
                    );
            
            collectAllAllocationRecords(job);

            if (jobRow[0].Value != null)
            {
                AddResult((DateTime)jobRow[0].Value, string.Format(SR.FormatJobCreated, ((DateTime )jobRow[0].Value).ToLocalTime().ToString(), jobRow[1].Value.ToString()));
            }

            StoreProperty prop;
            
            JobState currentState = JobState.All;
            prop = jobRow[JobPropertyIds.State];
            if (prop != null)
            {
                currentState = (JobState)prop.Value;
            }

            DateTime currentStartTime = DateTime.MinValue;
            prop = jobRow[JobPropertyIds.StartTime];
            if (prop != null)
            {
                currentStartTime = (DateTime)prop.Value;
            }

            List<DateTime> allStartTimes = new List<DateTime>();
            for (int jobIdx = 0; jobIdx < jobRunsAndModification.Count; jobIdx++)
            {
                if (jobRunsAndModification[jobIdx][JobHistoryPropertyIds.StartTime] != null)
                {
                    allStartTimes.Add((DateTime)jobRunsAndModification[jobIdx][JobHistoryPropertyIds.StartTime].Value);
                }
            }

            if (MayHaveResources(currentState))
            {
                //if the current job state is one where the current run has not yet come to an end (running, finishing or canceling)
                //the last start time should be the start of the current run.
                allStartTimes.Add(currentStartTime);
            }
            else
            {
                //if the job has already finished, then the last start time is just a book end and is set to DateTime.MaxValue
                allStartTimes.Add(DateTime.MaxValue);
            }                        

            int runCount = 0;
            
            foreach (PropertyRow item in jobRunsAndModification)
            {
                //print submit time only the first time
                //later submit times are not currently logged
                //so the old submit time will just reappear again
                
                prop = item[JobHistoryPropertyIds.SubmitTime];
                DateTime submitTime = DateTime.MinValue;
                if (prop != null && runCount == 0)
                {                    
                    submitTime = (DateTime)prop.Value;
                }

                prop = item[JobHistoryPropertyIds.StartTime];
                DateTime startTime = DateTime.MinValue;
                if (prop != null)
                {
                    startTime = (DateTime)prop.Value;                    
                }

                DateTime eventTime = DateTime.MinValue;

                prop = item[JobHistoryPropertyIds.EventTime];
                if (prop != null)
                {
                    eventTime = (DateTime)prop.Value;
                }

                JobEvent endEventType = JobEvent.None;
                prop = item[JobHistoryPropertyIds.JobEvent];
                if (prop != null)
                {
                    endEventType = (JobEvent)(prop.Value);
                }

                if (submitTime != DateTime.MinValue)
                {
                    // if submit time is mroe than or equal to the event time it is a dummy value
                    // added while writing the event to the db
                    if (submitTime < eventTime)
                    {
                        AddResult(submitTime, string.Format(SR.FormatJobSubmitted, submitTime.ToLocalTime().ToString()));
                    }
                }


                //write out the start time
                if (startTime != DateTime.MinValue)
                {
                    //Special case: if a job was canceled without ever having run, 
                    //its start time is the greater than equal to its cancelled time since the start time is 
                    // a place holder value that was filled in when the cancel event was being written to db
                    if (!(JobEvent.Canceled == endEventType && eventTime <= startTime))
                    {
                        AddResult(startTime, string.Format(SR.FormatJobStarted, startTime.ToLocalTime().ToString()));
                    }
                }

                prop = item[JobHistoryPropertyIds.RequeueId];
                if (prop != null)
                {
                    DateTime runEndTime;
                    if (eventTime != DateTime.MinValue)
                    {
                        runEndTime = eventTime;
                        if (runEndTime > allStartTimes[runCount + 1])
                        {
                            runEndTime = allStartTimes[runCount + 1];
                        }
                    }
                    else
                    {
                        runEndTime = allStartTimes[runCount + 1];
                    }
                    ExamineRun(job, (int)prop.Value, allStartTimes[runCount], runEndTime);
                }


               
                
                if (endEventType != JobEvent.None)
                {                    
                    switch (endEventType)
                    {
                        case JobEvent.Finished:
                            AddResult(eventTime, string.Format(SR.FormatJobEventFinished, eventTime.ToLocalTime()));
                            break;  
                        case JobEvent.Failed:
                            AddResult(eventTime, string.Format(SR.FormatJobEventFailed, eventTime.ToLocalTime()));
                            break;
                        case JobEvent.Canceled:
                            //A canceled event should be preceded by a cancel request.
                            //If it has not been printed during a call to ExamineRun (for a job that never started)
                            //we should try to print the cancel request here.
                            //If it has already been printed, it is safe to call it again.
                            OutputCancelRequests(eventTime);
                            AddResult(eventTime, string.Format(SR.FormatJobEventCanceled, eventTime.ToLocalTime()));
                            break;
                        case JobEvent.PropChange:
                            AddResult(eventTime, string.Format(SR.FormatJobEventPropChange, eventTime.ToLocalTime(), item[JobHistoryPropertyIds.Operator].Value, item[JobHistoryPropertyIds.PropChange].Value));
                            continue;
                        default:
                            break;
                    }
                }
                runCount++;
            }
            
            // If the job is still running, add any remaining records
            
            if (jobRow[2].Id == JobPropertyIds.State)
            {
                if ((JobState)jobRow[2].Value == JobState.Queued)
                {
                    if (runCount == 0)
                    {
                        prop = jobRow[JobPropertyIds.SubmitTime];
                        if (prop != null)
                        {
                            AddResult((DateTime)prop.Value, string.Format(SR.FormatJobSubmitted, ((DateTime)prop.Value).ToLocalTime().ToString()));
                        }
                    }
                }
                else if (MayHaveResources((JobState)jobRow[2].Value))
                {
                    if (runCount == 0)
                    {
                        DateTime submitTime;
                        prop = jobRow[JobPropertyIds.SubmitTime];
                        submitTime = (DateTime)prop.Value;
                        if (prop != null)
                        {
                            AddResult((DateTime)prop.Value, string.Format(SR.FormatJobSubmitted, ((DateTime)prop.Value).ToLocalTime().ToString()));
                        }
                    }
                    
                    prop = jobRow[JobPropertyIds.StartTime];                    
                    if (prop != null)
                    {                    
                        AddResult(currentStartTime, string.Format(SR.FormatJobStarted, currentStartTime.ToLocalTime().ToString()));                        
                    }
                    
                    prop = jobRow[JobPropertyIds.RequeueCount];
                    if (prop != null)
                    {
                        ExamineRun(job, (int)prop.Value,currentStartTime,DateTime.MaxValue);
                    }
                }
            }
        }

        /// <summary>
        /// Is the job state one where the job could have resources allocated to it
        /// </summary>
        /// <param name="currentState"></param>
        /// <returns></returns>
        private static bool MayHaveResources(JobState currentState)
        {
            return JobState.Running == currentState
                            || JobState.Finishing == currentState
                            || JobState.Canceling == currentState;
        }

        private void AddResult(DateTime dt, string record)
        {
            List<string> records;
            if (!result.TryGetValue(dt, out records))
            {
                result.Add(dt, records = new List<string>());
            }

            records.Add(record);
        }

        public IEnumerable<string> Result
        {
            get { return result.Values.SelectMany(list => list); }
        }
    }
}
