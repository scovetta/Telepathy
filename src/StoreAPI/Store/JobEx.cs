//------------------------------------------------------------------------------
// <copyright file="JobEx.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      JobEx object.  
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Store
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Security;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using Microsoft.Hpc.Scheduler.Properties;

    internal class JobEx : StoreObjectBase, IClusterJob
    {
        private Int32 _jobid;
        private ConnectionToken _token;
        private bool _fPassword = false;
        private StoreProperty _propUsername = null;

        int cancelFinishStateMask = (int)JobState.Canceled | (int)JobState.Failed | (int)JobState.Finished;

        int cancelFinishStateMaskForRunningJob = (int)JobState.Canceled | (int)JobState.Failed | (int)JobState.Finished | (int)JobState.Queued;

        public JobEx(Int32 jobId, ConnectionToken token, SchedulerStoreSvc helper, StoreProperty[] existingProps)
            : base(helper, ObjectType.Job)
        {
            Debug.Assert(jobId != 0);
            Debug.Assert(token != null);
            Debug.Assert(helper != null);

            if (token != null && helper != null)
            {
                _jobid = jobId;
                _token = token;
                _helper = helper;
            }

            _CheckProps(existingProps);
        }

        public override int Id
        {
            get { return _jobid; }
        }

        protected ConnectionToken Token
        {
            get { return _token; }
        }

        public IAllocationRowSet OpenAllocationRowSet()
        {
            return new LocalAllocationRowSet(_helper, RowSetType.Snapshot, _jobid, 0);
        }

        public IRowEnumerator OpenAllocationEnumerator()
        {
            return new LocalRowEnumerator(_helper, ObjectType.Allocation, AllocationProperties.AllocationObject, _jobid);
        }

        public IJobMessageRowSet OpenMessageRowset()
        {
            return OpenMessageRowset(RowSetType.Snapshot);
        }

        public IJobMessageRowSet OpenMessageRowset(RowSetType type)
        {
            return new LocalJobMessageRowset(_helper, type, _jobid);
        }

        public IRowEnumerator OpenMessageEnumerator()
        {
            return new LocalRowEnumerator(_helper, ObjectType.JobMessage, JobMessagePropertyIds.JobMessageObject, _jobid);
        }

        public IClusterJob Clone()
        {
            int jobIdNew = 0;

            _helper.ServerWrapper.Job_Clone(_jobid, ref jobIdNew);

            return new JobEx(jobIdNew, _token, _helper, null);
        }

        public override PropertyRow GetAllProps()
        {
            return GetProps();
        }

        public override PropertyRow GetProps(params PropertyId[] propertyIds)
        {
            return _helper.GetPropsFromServer(ObjectType.Job, _jobid, propertyIds);
        }

        public override PropertyRow GetPropsByName(params string[] propertyNames)
        {
            return GetProps(PropertyLookup.Job.PropertyIdsFromNames(propertyNames));
        }

        void _CheckProps(StoreProperty[] props)
        {
            if (props != null && props.Length > 0)
            {
                foreach (StoreProperty prop in props)
                {
                    if (prop.Id == JobPropertyIds.UserName)
                    {
                        _propUsername = prop;
                    }
                    else if (prop.Id == JobPropertyIds.Password)
                    {
                        _fPassword = true;
                    }
                    else if (prop.Id == JobPropertyIds.EncryptedPassword)
                    {
                        _fPassword = true;
                    }
                }
            }
        }

        public override void SetProps(params StoreProperty[] jobProperties)
        {
            // Check to see if the password and username are being
            // set.

            _CheckProps(jobProperties);

            _helper.SetPropsOnServer(ObjectType.Job, _jobid, jobProperties);
        }

        public void CreateTaskGroupsAndDependencies(List<string> newGroups, List<KeyValuePair<int, int>> newDependencies, int groupIdBase, out List<int> newGroupIds)
        {
            _helper.ServerWrapper.TaskGroup_CreateTaskGroupsAndDependencies(_jobid, newGroups, newDependencies, groupIdBase, out newGroupIds);
        }

        public int DeleteTask(int taskId)
        {
            int jobId;

            _helper.ServerWrapper.Task_ValidateTaskId(taskId, out jobId);

            _helper.ServerWrapper.Task_DeleteTask(_jobid, taskId);

            return 0;
        }

        public void CancelNoWait(object param, CancelRequest request, string message)
        {
            var async = this.BeginCancel(ar => { }, param, request, message);
            _helper.CancelAsyncWait(async);
        }

        public IAsyncResult BeginCancel(AsyncCallback callback, object param, string message)
        {
            return BeginCancel(callback, param, CancelRequest.CancelByUser, message);
        }

        const int MaxCancelMsgLen = 128;
        public IAsyncResult BeginCancel(AsyncCallback callback, object param, CancelRequest request, string message)
        {
            if (message != null && message.Length > MaxCancelMsgLen)
            {
                throw new SchedulerException(ErrorCode.Operation_CancelMessageIsTooLong, null);
            }

            JobState state = (JobState)GetProps(JobPropertyIds.State)[0].Value;
            if (state != JobState.Running &&
                state != JobState.Queued &&
                state != JobState.Configuring &&
                state != JobState.Submitted &&
                state != JobState.Validating)
            {
                if (state == JobState.Canceling || state == JobState.Finishing)
                {
                    if (request != CancelRequest.Finish
                        && request != CancelRequest.FinishGraceful
                        && request != CancelRequest.CancelByUser
                        && request != CancelRequest.CancelForceByUser
                        && request != CancelRequest.CancelGraceful)
                    {
                        throw new SchedulerException(ErrorCode.Operation_InvalidCancelJobState, state.ToString());
                    }
                }
                else
                {
                    throw new SchedulerException(ErrorCode.Operation_InvalidCancelJobState, state.ToString());
                }
            }

            int stateMask = 0;
            if (state == JobState.Running)
            {
                stateMask = cancelFinishStateMaskForRunningJob;
            }
            else
            {
                stateMask = cancelFinishStateMask;
            }

            AsyncResult async = _helper.RegisterForJobStateChange(
                    _jobid,
                    stateMask,
                    callback,
                    param
            );

            int errorCode = ErrorCode.Operation_CanceledByUser;
            if (request == CancelRequest.FinishGraceful || request == CancelRequest.Finish)
            {
                errorCode = ErrorCode.Operation_FinishedByUser;
            }

            StoreProperty[] cancelProps = 
            {
                new StoreProperty(JobPropertyIds.ErrorCode, errorCode),
                new StoreProperty(JobPropertyIds.ErrorParams, string.IsNullOrEmpty(message) ? SR.Job_EmptyCancelReason : message)
            };

            if (_helper.ServerVersion.IsV2)
            {
                //for back compat while talking to a v2 server convert the cancelforce by user to cancel force
                if (request == CancelRequest.CancelForceByUser)
                {
                    request = CancelRequest.CancelByUser;
                }
            }

            CallResult cr = _helper.ServerWrapper.Job_CancelJob(_jobid, request, cancelProps);
            if (cr.Code != ErrorCode.Success)
            {
                _helper.CancelAsyncWait(async);
                cr.Throw();
            }

            return async;
        }

        public JobState EndCancel(IAsyncResult result)
        {
            return EndAsyncRequest(result);
        }

        enum UserNameSource
        {
            ProvidedBySubmitter,
            CurrentUser,
            FromJob
        }

        private static string[] _credentialTypeDialogStrings = null;
        private static string[] CredentialTypeDialogStrings
        {
            get
            {
                if (_credentialTypeDialogStrings == null)
                {
                    _credentialTypeDialogStrings = new string[]
                    {
                       SR.CredentialTypeDialog_LabelString,
                       SR.CredentialTypeDialog_PwdCheckBoxString,
                       SR.CredentialTypeDialog_CertCheckBoxString,
                       SR.CredentialTypeDialog_OkButtonString,
                       SR.CredentialTypeDialog_CancelButtonString
                    };

                }
                return _credentialTypeDialogStrings;
            }
        }



        public IAsyncResult BeginSubmit(StoreProperty[] props, AsyncCallback callback, object param)
        {
            _CheckProps(props);

            props = PropertyLookup.ProcessSetProps(_helper, ObjectType.Job, props);

            CallResult cr = null;
            AsyncResult async = null;
            UserNameSource userNameSource = UserNameSource.CurrentUser;

            StoreProperty propReuseCredential = null;
            bool promptForSave = true;
            bool passwordCreds = true;
            bool needToChoseCredType = true; //true-> means type of cred needs to be chosen
            bool expiredCreds = false;
            string userNameFromServer = null;

            // There can be certain case where the job object is opened from the scheduler
            // with the _propUsername's value an empty string.
            // In this case, we should interpret it to "user name not specified"
            if (_propUsername != null && !string.IsNullOrEmpty(_propUsername.Value as string))
            {
                userNameSource = UserNameSource.ProvidedBySubmitter;
            }

            RetryManager retry = new RetryManager(InstantRetryTimer.Instance, 3);

            try
            {
                do
                {
                    // Make sure that we have a password and credentials for the user.
                    // If not prompt the user for the credentials.

                    StoreProperty propPassword = null;

                    if (userNameSource != UserNameSource.ProvidedBySubmitter || _fPassword == false)
                    {
                        string username = null;

                        // First try to get something from the cache.

                        if (userNameSource == UserNameSource.ProvidedBySubmitter)
                        {
                            username = (string)_propUsername.Value;
                        }
                        else
                        {
                            PropertyRow userRow = GetProps(JobPropertyIds.UserName);
                            StoreProperty userProp = userRow[0];
                            if (userProp == null || userProp.Id != JobPropertyIds.UserName || string.IsNullOrEmpty((string)userProp.Value))
                            {
                                if (_helper.IdentityProvider != null)
                                {
                                    username = _helper.IdentityProvider();
                                }
                                else
                                {
                                    if (_helper.ServerVersion.Version < VersionControl.V3SP2)
                                    {
                                        username = WindowsIdentity.GetCurrent().Name;
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(userNameFromServer))
                                        {
                                            username = userNameFromServer;
                                        }
                                    }
                                }
                                userNameSource = UserNameSource.CurrentUser;
                            }
                            else
                            {
                                username = userProp.Value as string;
                                userNameSource = UserNameSource.FromJob;
                            }
                        }

                        _propUsername = new StoreProperty(JobPropertyIds.UserName, username);

                        if (!_fPassword)
                        {
                            // See if there is a cached credential for the password.

                            // Here, we do not expect the user name to change after the LookupCredential call.                        
                            byte[] cached = null;

                            //if the server version is older than v3sp1, look up the credential in the cache
                            //For newer servers we should not lookup in order to allow passwordless submits
                            //if allowed by the server
                            if (_helper.ServerVersion.IsOlderThanV3SP1)
                            {
                                cached = CredentialCache.LookupCredential(_helper.clusterName, ref username);
                            }

                            if (cached != null)
                            {
                                propPassword = new StoreProperty(JobPropertyIds.EncryptedPassword, cached);
                            }
                            else if (retry.RetryCount > 0 || _helper.ServerVersion.IsV2)
                            {
                                try
                                {
                                    bool fSave = false;
                                    System.Security.SecureString password = null;
                                    if (passwordCreds)
                                    {
                                        GetPasswordCreds(ref userNameSource, ref propReuseCredential, promptForSave, ref propPassword, ref username, ref fSave, ref password);
                                    }
                                    else
                                    {
                                        bool certCreds = true;
                                        if (needToChoseCredType)
                                        {
                                            int choice = CredentialType.NoChoice;
                                            CredentialType.PromptForCredentialType(SchedulerStore._fConsole, SchedulerStore._hWnd, ref choice, CredentialTypeDialogStrings);
                                            if (choice == CredentialType.NoChoice)
                                            {
                                                //user refused to choose: so throw the exception as user canceled error
                                                throw new Win32Exception(0x000004C7);
                                            }
                                            if (choice == CredentialType.PwdChoice)
                                            {
                                                certCreds = false;
                                            }
                                        }
                                        if (certCreds)
                                        {
                                            string[] settingsToLoad = { "HpcSoftCardTemplate" };
                                            List<string> settingsValues = null;
                                            _helper.GetConfigSettingsValues(settingsToLoad, out settingsValues);

                                            string certificateTemplateName = settingsValues[0];

                                            byte[] certBytes = null;
                                            SecureString pfxPassword;
                                            certBytes = PropertyUtil.GetCertFromStore(null, certificateTemplateName, out  pfxPassword);
                                            if (certBytes == null)
                                            {
                                                if (!SchedulerStore._fConsole && SchedulerStore._hWnd == new IntPtr(-1))
                                                {
                                                    throw new System.Security.Authentication.InvalidCredentialException();
                                                }
                                                else
                                                {
                                                    if (expiredCreds)
                                                    {
                                                        throw new SchedulerException(ErrorCode.Operation_SoftCardAboutToExpireShort, "");
                                                    }
                                                    else
                                                    {
                                                        throw new SchedulerException(ErrorCode.Operation_NoCertificateFoundOnClient, "");
                                                    }
                                                }

                                            }

                                            //save the certificate from the store
                                            try
                                            {
                                                _helper.SaveUserCertificate(username, pfxPassword, true, certBytes);
                                            }
                                            catch (Exception)
                                            {
                                                if (!SchedulerStore._fConsole && SchedulerStore._hWnd == new IntPtr(-1))
                                                {
                                                    throw new System.Security.Authentication.InvalidCredentialException();
                                                }
                                                else
                                                {
                                                    throw;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            GetPasswordCreds(ref userNameSource, ref propReuseCredential, promptForSave, ref propPassword, ref username, ref fSave, ref password);
                                        }
                                    }
                                }
                                catch (System.Security.Authentication.InvalidCredentialException invCredEx)
                                {
                                    if (cr != null && cr.Code == ErrorCode.Operation_AuthenticationFailure)
                                    {
                                        throw new System.Security.Authentication.InvalidCredentialException(invCredEx.Message, new SchedulerException(cr.Code, cr.Params));
                                    }
                                    else
                                    {
                                        throw;
                                    }
                                }
                            }
                            else
                            {
                                propPassword = null;
                            }
                        }

                        // Replace the values within the specified properties
                        // or add these to the end of the properties.

                        List<StoreProperty> propsNew = new List<StoreProperty>();

                        if (props != null)
                        {
                            foreach (StoreProperty prop in props)
                            {
                                if (prop.Id != JobPropertyIds.UserName &&
                                    prop.Id != JobPropertyIds.Password &&
                                    prop.Id != JobPropertyIds.EncryptedPassword)
                                {
                                    propsNew.Add(prop);
                                }
                            }
                        }



                        // We need to write the user name of a job if it was specified during submission or 
                        // use local user if the job does not have one yet
                        if (userNameSource != UserNameSource.FromJob)
                        {
                            propsNew.Add(_propUsername);
                        }

                        if (propPassword != null)
                        {
                            propsNew.Add(propPassword);
                        }

                        //if the server is v3sp1 or newer and the user has specified the reusecredential property
                        //add this property to be passed to the server
                        if (!_helper.ServerVersion.IsOlderThanV3SP1 && propReuseCredential != null)
                        {
                            propsNew.Add(propReuseCredential);
                        }

                        props = propsNew.ToArray();
                    }

                    if (null != async)
                    {
                        // Given that we are in a retry loop, a previous
                        // try may have registered an AsyncResult.
                        // Here we clean up from any previous iteration.

                        AsyncResult temp = async;

                        async = null;

                        _helper.CancelAsyncWait(temp);
                    }

                    async = _helper.RegisterForJobStateChange(
                            _jobid,
                            (int)(JobState.Queued | JobState.Failed | JobState.Running | JobState.Finished | JobState.Canceled),
                            callback,
                            param
                    );

                    if (_helper.ServerVersion.Version < VersionControl.V3SP2)
                    {
                        cr = _helper.ServerWrapper.Job_SubmitJob(_jobid, props);
                    }
                    else
                    {
                        cr = _helper.ServerWrapper.Job_SubmitJob(_jobid, props, out userNameFromServer);
                    }

                    if (cr.Code == ErrorCode.Operation_AuthenticationFailure
                        || cr.Code == ErrorCode.Operation_CryptographyError)
                    {
                        string userName = _propUsername.Value as string;
                        if (!string.IsNullOrEmpty(userName))
                        {
                            //v3 bug 12362
                            //If the submission is happening by impersonating the owner and the impersonator 
                            //has not loaded that owner's profile, the owner's registry will not be available for 
                            //purging the credential
                            try
                            {
                                CredentialCache.PurgeCredential(_helper.clusterName, userName);
                            }
                            catch (Exception)
                            {
                            }
                        }

                        if (userNameSource != UserNameSource.ProvidedBySubmitter)
                        {
                            _propUsername = null;
                        }
                        _fPassword = false;

                        //if the authentication failed because credential reuse was disabled
                        //we do not want to prompt for saving the password
                        if (cr.Code == ErrorCode.Operation_AuthenticationFailure)
                        {
                            if (!string.IsNullOrEmpty(cr.Params))
                            {
                                ReadIntParam(cr, ref promptForSave, ref passwordCreds, ref needToChoseCredType, ref expiredCreds);
                            }
                        }


                        if (!retry.HasAttemptsLeft)
                        {
                            break;
                        }
                        retry.WaitForNextAttempt();
                    }
                    else
                    {
                        break;
                    }
                } while (true);

                if (cr.Code != ErrorCode.Success)
                {
                    cr.Throw();
                }
            }
            catch (Exception)
            {
                if (null != async)
                {
                    // Something threw an exception.
                    // we must clean up or we will leak
                    // the AsyncResult. This is because exceptions result 
                    // in no balancing "EndSubmit()" call...

                    _helper.CancelAsyncWait(async);
                }

                throw;
            }

            return async;
        }

        private static void ReadIntParam(CallResult cr, ref bool promptForSave, ref bool passwordCreds, ref bool needToChoseCredType, ref bool expiredCreds)
        {
            int paramCode = 0;
            bool intParamPresent = Int32.TryParse(cr.Params, out paramCode);
            if (intParamPresent)
            {
                switch (paramCode)
                {
                    case ErrorCode.AuthFailureDisableCredentialReuse:
                        promptForSave = false;
                        break;

                    case ErrorCode.AuthFailureAllowSoftCardNoValidSaved:
                        passwordCreds = false;
                        needToChoseCredType = true;
                        break;
                    case ErrorCode.AuthFailureAllowSoftCardAboutToExpireSaved:
                        expiredCreds = true;
                        goto case ErrorCode.AuthFailureAllowSoftCardNoValidSaved;

                    case ErrorCode.AuthFailureAllowSoftCardDisableCredentialReuse:
                        promptForSave = false;
                        goto case ErrorCode.AuthFailureAllowSoftCardNoValidSaved;

                    case ErrorCode.AuthFailureRequireSoftCardAboutToExpireSaved:
                        expiredCreds = true;
                        goto case ErrorCode.AuthFailureRequireSoftCardNoValidSaved;

                    case ErrorCode.AuthFailureRequireSoftCardNoValidSaved:
                    case ErrorCode.AuthFailureRequireSoftCardDisableCredentialReuse:
                        passwordCreds = false;
                        needToChoseCredType = false;
                        break;
                }
            }
        }

        private void GetPasswordCreds(ref UserNameSource userNameSource, ref StoreProperty propReuseCredential, bool promptForSave, ref StoreProperty propPassword, ref string username, ref bool fSave, ref System.Security.SecureString password)
        {
            if (promptForSave)
            {
                Credentials.PromptForCredentials(_helper.clusterName, ref username, ref password, ref fSave, SchedulerStore._fConsole, SchedulerStore._hWnd);

                if (fSave)
                {
                    byte[] encrypted = _helper.EncryptCredential(username, Credentials.UnsecureString(password));

                    try
                    {
                        //it is possible to get an exception if this job is being submitted by a process that is impersonating the 
                        //owner and has not loaded the owner's profile and registry
                        CredentialCache.CacheCredential(_helper.clusterName, username, encrypted);
                    }
                    catch (Exception)
                    {
                    }
                    propReuseCredential = new StoreProperty(ProtectedJobPropertyIds.ReuseCredentials, true);
                }
                else
                {
                    propReuseCredential = new StoreProperty(ProtectedJobPropertyIds.ReuseCredentials, false);
                }
            }
            else
            {
                Credentials.PromptForCredentials(_helper.clusterName, ref username, ref password, SchedulerStore._fConsole, SchedulerStore._hWnd);
            }

            _propUsername = new StoreProperty(JobPropertyIds.UserName, username);
            userNameSource = UserNameSource.ProvidedBySubmitter;
            propPassword = new StoreProperty(JobPropertyIds.Password, Credentials.UnsecureString(password));
        }

        public JobState EndSubmit(IAsyncResult result)
        {
            return EndAsyncRequest(result);
        }

        public void Submit(params StoreProperty[] submitProps)
        {
            // Check to see if someone called Submit(null)

            if (submitProps != null && submitProps.Length == 1 && submitProps[0] == null)
            {
                submitProps = null;
            }

            IAsyncResult async = BeginSubmit(submitProps, null, null);

            JobState state = EndSubmit(async);

            if (state == JobState.Failed)
            {
                // Something went wrong.  Throw an error.                
                PropertyRow props = GetProps(JobPropertyIds.ErrorCode,
                    JobPropertyIds.ErrorParams);

                int errorCode = (int)props[0].Value;
                string errorParams = props[1].Value as string;

                throw new SchedulerException(errorCode, errorParams);
            }
        }

        public void Requeue()
        {
            _helper.ServerWrapper.Job_RequeueJob(_jobid);
        }

        public void Cancel(string message)
        {
            IAsyncResult async = BeginCancel(null, null, message);

            JobState endState = EndCancel(async);
            VerifyEndState(SR.FormatJobCancelFailed, endState, JobState.Canceled);
        }

        private static void VerifyEndState(string operationFormatString, JobState endState, JobState expectedState)
        {
            if (endState != expectedState)
            {
                throw new SchedulerException(string.Format(operationFormatString, endState));
            }
        }


        public void Cancel(string message, bool isForced, bool isGraceful = false)
        {
            if (isGraceful && isForced)
            {
                throw new SchedulerException(SR.ForcedGracefulConflict);
            }

            if (isGraceful)
            {
                // only when graceful cancel/finish, we don't wait for the final results as it could take very long time.
                // This behavior is requested by customer.
                this.CancelNoWait(null, CancelRequest.CancelGraceful, message);
            }
            else if (isForced)
            {
                IAsyncResult async = BeginCancel(null, null, CancelRequest.CancelForceByUser, message);
                JobState endState = EndCancel(async);
                VerifyEndState(SR.FormatJobCancelFailed, endState, JobState.Canceled);
            }
            else
            {
                Cancel(message);
            }
        }

        /// <summary>
        /// send the cancelrequest.finish message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isForced">true if the job will cancel the the running tasks regardless of the graceful period.</param>
        /// <param name="isGraceful">true if the job won't cancel the running tasks.</param>
        public void Finish(string message, bool isForced, bool isGraceful)
        {
            if (isGraceful && isForced)
            {
                throw new SchedulerException(SR.ForcedGracefulConflict);
            }

            if (isForced)
            {
                throw new SchedulerException(string.Format(SR.MethodNotImplemented, "Finish(isForced = true)"));
            }
            else if (isGraceful)
            {
                // only when graceful cancel/finish, we don't wait for the final results as it could take very long time.
                // This behavior is requested by customer.
                this.CancelNoWait(null, CancelRequest.FinishGraceful, message);
            }
            else
            {
                IAsyncResult async = BeginCancel(null, null, CancelRequest.Finish, message);
                JobState endState = EndCancel(async);
                VerifyEndState(SR.FormatJobFinishFailed, endState, JobState.Finished);
            }
        }

        public void Pause()
        {
            throw new SchedulerException(string.Format(SR.MethodNotImplemented, "Pause"));
        }

        public void Resume()
        {
            throw new SchedulerException(string.Format(SR.MethodNotImplemented, "Resume"));
        }

        public void Configure()
        {
            _helper.ServerWrapper.Job_ConfigJob(_jobid);
        }


        public void SetHoldUntil(DateTime holdUntil)
        {
            _helper.ServerWrapper.Job_SetHoldUntil(_jobid, holdUntil);
        }



        public IClusterTask CreateTask(StoreProperty[] taskProperties)
        {
            taskProperties = PropertyLookup.ProcessSetProps(_helper, ObjectType.Task, taskProperties);

            Int32 taskid = 0;

            _helper.ServerWrapper.Task_AddTaskToJob(_jobid, ref taskid, taskProperties);

            return new TaskEx(_jobid, taskid, _helper);
        }

        public List<IClusterTask> CreateTasks(List<StoreProperty[]> taskPropertyList)
        {
            List<StoreProperty[]> processedTaskPropList = new List<StoreProperty[]>();
            foreach (StoreProperty[] props in taskPropertyList)
            {
                processedTaskPropList.Add(PropertyLookup.ProcessSetProps(_helper, ObjectType.Task, props));
            }

            List<Int32> taskids = null;

            _helper.ServerWrapper.Task_AddTasksToJob(_jobid, ref taskids, processedTaskPropList);

            List<IClusterTask> tasks = new List<IClusterTask>();

            if (taskids != null)
            {
                foreach (int taskid in taskids)
                {
                    tasks.Add(new TaskEx(_jobid, taskid, _helper));
                }
            }
            return tasks;
        }

        public IClusterTask OpenTask(Int32 taskId)
        {
            int jobId;

            _helper.ServerWrapper.Task_ValidateTaskId(taskId, out jobId);

            if (jobId != _jobid)
            {
                CallResult.LocalThrow(ErrorCode.Operation_InvalidTaskId);
            }

            return new TaskEx(_jobid, taskId, _helper);
        }

        public IClusterTask OpenTask(TaskId taskId)
        {
            int taskSystemId = 0;

            taskId.ParentJobId = _jobid;

            _helper.ServerWrapper.Task_FindTaskIdByTaskId(_jobid, taskId, out taskSystemId);

            return new TaskEx(_jobid, taskSystemId, _helper);
        }


        public ITaskRowSet OpenTaskRowSet()
        {
            return OpenTaskRowSet(RowSetType.Snapshot, TaskRowSetOptions.None);
        }


        public ITaskRowSet OpenTaskRowSet(RowSetType type)
        {
            return OpenTaskRowSet(type, TaskRowSetOptions.None);
        }

        public ITaskRowSet OpenTaskRowSet(RowSetType type, TaskRowSetOptions options)
        {
            _helper.ServerWrapper.EnumeratePermissionCheck(ObjectType.Task, _jobid);

            LocalTaskRowSet rowset = new LocalTaskRowSet(_helper, type, _jobid, options);

            return (ITaskRowSet)rowset;
        }

        public IRowEnumerator OpenTaskRowEnumerator()
        {
            _helper.ServerWrapper.EnumeratePermissionCheck(ObjectType.Task, _jobid);

            return new LocalRowEnumerator(_helper, ObjectType.Task, TaskPropertyIds.TaskObject, _jobid);
        }

        public IRowEnumerator OpenTaskRowEnumerator(TaskRowSetOptions options)
        {
            _helper.ServerWrapper.EnumeratePermissionCheck(ObjectType.Task, _jobid);

            return new LocalRowEnumerator(_helper, ObjectType.Task, TaskPropertyIds.TaskObject, _jobid, options);
        }

        public ITaskGroupRowSet OpenTaskGroupRowSet()
        {
            _helper.ServerWrapper.EnumeratePermissionCheck(ObjectType.TaskGroup, _jobid);

            return new LocalTaskGroupRowSet(_helper, RowSetType.Snapshot, _jobid);
        }

        public IAsyncResult BeginCancelTask(TaskId taskId, AsyncCallback callback, object param, string message)
        {
            return BeginCancelTask(taskId, CancelRequest.CancelByUser, callback, param, message);
        }

        public IAsyncResult BeginCancelTask(TaskId taskId, CancelRequest request, AsyncCallback callback, object param, string message)
        {
            int taskSystemId = 0;

            _helper.ServerWrapper.Task_FindTaskIdByTaskId(_jobid, taskId, out taskSystemId);

            return BeginCancelTask(taskSystemId, request, callback, param, message);
        }

        public IAsyncResult BeginCancelTask(Int32 taskId, AsyncCallback callback, object param, string message)
        {
            return BeginCancelTask(taskId, CancelRequest.CancelByUser, callback, param, message);
        }

        public IAsyncResult BeginCancelTask(Int32 taskId, CancelRequest request, AsyncCallback callback, object param, string message)
        {
            if (message != null && message.Length > MaxCancelMsgLen)
            {
                throw new SchedulerException(ErrorCode.Operation_CancelMessageIsTooLong, null);
            }

            int jobId;

            // Validate will throw for an invalid task Id.
            _helper.ServerWrapper.Task_ValidateTaskId(taskId, out jobId);

            AsyncResult async = _helper.RegisterForTaskStateChange(
                    taskId,
                    (int)TaskState.Canceled | (int)TaskState.Finished | (int)TaskState.Failed,
                    callback,
                    param
            );

            if (message == null)
            {
                message = string.Empty;
            }
            if (_helper.ServerVersion.IsV2)
            {
                if (request == CancelRequest.CancelForceByUser)
                {
                    request = CancelRequest.CancelByUser;
                }
            }

            CallResult result = _helper.ServerWrapper.Task_CancelTask(
                _jobid,
                taskId,
                request,
                ErrorCode.Operation_CanceledByUser,
                message);
            if (result.Code != ErrorCode.Success)
            {
                _helper.CancelAsyncWait(async);

                result.Throw();
            }

            return async;
        }

        public TaskState EndCancelTask(IAsyncResult result)
        {
            AsyncResult _result = (AsyncResult)result;

            // Wait for it to finish, note that this may
            // be already signalled.

            WaitHandle handle = _result.AsyncWaitHandle;
            if (handle != null)
            {
                handle.WaitOne();
            }

            TaskState state = (TaskState)_result.ResultState;

            _helper.CloseAsyncResult(_result);

            return state;
        }

        public void RequestToCancelTask(int taskId, string message)
        {
            throw new SchedulerException(string.Format(SR.MethodNotImplemented, "RequestToCancelTask"));
        }

        public void CancelTask(int taskId, string message)
        {
            IAsyncResult async = BeginCancelTask(taskId, null, null, message);

            TaskState state = EndCancelTask(async);
            checkPostCancelTaskState(taskId, state);
        }

        public void CancelTask(int taskId, string message, bool isForced)
        {
            if (!isForced)
            {
                CancelTask(taskId, message);
            }
            else
            {
                IAsyncResult async = BeginCancelTask(taskId, CancelRequest.CancelForceByUser, null, null, message);
                TaskState state = EndCancelTask(async);
                checkPostCancelTaskState(taskId, state);
            }
        }

        public void FinishTask(int taskId, string message)
        {
            var ar = this.BeginCancelTask(taskId, CancelRequest.Finish, null, null, message);
            TaskState state = this.EndCancelTask(ar);
            if (state != TaskState.Finished)
            {
                int errorCode = ErrorCode.Operation_InvalidCancelTaskState;
                var errorParams = state.ToString();
                throw new SchedulerException(errorCode, errorParams);
            }
        }

        public void FinishTaskByNiceId(int taskNiceId, string message)
        {
            int taskId = 0;
            _helper.ServerWrapper.Task_FindTaskIdByFriendlyId(_jobid, taskNiceId, ref taskId);
            this.FinishTask(taskId, message);
        }

        private void checkPostCancelTaskState(int taskId, TaskState state)
        {

            if (state != TaskState.Canceled && state != TaskState.Failed)
            {
                int errorCode;
                string errorParams;

                if (state == TaskState.Finished)
                {
                    errorCode = ErrorCode.Operation_InvalidCancelTaskState;
                    errorParams = TaskState.Finished.ToString();
                }
                else
                {
                    // Something went wrong.  Throw an error.

                    PropertyId[] ids = 
                    {
                        TaskPropertyIds.ErrorCode,
                        TaskPropertyIds.ErrorParams,
                    };

                    PropertyRow props = null;

                    props = _helper.GetPropsFromServer(ObjectType.Task, taskId, ids);

                    errorCode = (int)props[0].Value;
                    errorParams = props[0].Value as string;
                }

                throw new SchedulerException(errorCode, errorParams);
            }
        }

        public void SubmitTasks(int[] taskIds)
        {
            if (_helper.ServerVersion.IsV2)
            {
                throw new NotImplementedException("This is not implemented in V2");
            }
            _helper.ServerWrapper.Task_SubmitTasks(_jobid, taskIds);
        }


        //write out the custom properties from among these properties
        private void exportCustomPropsAsExtended(PropertyRow row, XmlWriter writer)
        {
            //Find all the custom properties and write them out            

            bool firstCustomProp = true;
            foreach (StoreProperty prop in row.Props)
            {
                if ((prop.Id.Flags & PropFlags.Custom) != 0)
                {
                    //This is a custom property

                    if (firstCustomProp)
                    {
                        //This is the first custom property so, write out the tag for extended terms
                        firstCustomProp = false;
                        writer.WriteStartElement(XmlNames.ExtendedTerms);
                    }
                    writer.WriteStartElement(XmlNames.Term);
                    writer.WriteElementString(XmlNames.Name, prop.Id.Name);
                    writer.WriteElementString(XmlNames.Value, prop.Value.ToString());

                    writer.WriteEndElement(); // close Term
                }
            }

            if (firstCustomProp == false)
            {
                //At least one custom prop was written, so we need to close the extended terms tag
                writer.WriteEndElement(); //close extended terms
            }
        }

        void _ExportV1Xml(XmlWriter writer)
        {
            writer.WriteStartElement(XmlNames.Job);

            // First write out the job properties

            PropertyXmlMap map = new PropertyXmlMap();

            // AllocatedNodes
            map.Add(JobPropertyIds.RequestedNodes, "AskedNodes");
            map.Add(JobPropertyIds.Id);
            map.Add(JobPropertyIds.IsBackfill);
            map.Add(JobPropertyIds.IsExclusive);
            map.Add(JobPropertyIds.MaxCores, "MaximumNumberOfProcessors");
            map.Add(JobPropertyIds.MinCores, "MinimumNumberOfProcessors");
            map.Add(JobPropertyIds.Name);
            map.Add(JobPropertyIds.Priority);
            map.Add(JobPropertyIds.Project);
            map.Add(JobPropertyIds.RunUntilCanceled);
            map.Add(JobPropertyIds.SoftwareLicense);
            map.Add(JobPropertyIds.Owner, "SubmittedBy");
            map.Add(JobPropertyIds.UserName, "User");
            map.Add(JobPropertyIds.RuntimeSeconds, new PropertyXmlMap.RunTimeProperty());
            // Status

            map.WriteProps(this, writer);


            // Now write out the custom props as extended terms.
            PropertyRow allProps = GetAllProps();
            exportCustomPropsAsExtended(allProps, writer);

            // Now write out the tasks.

            PropertyXmlMap taskMap = new PropertyXmlMap();

            taskMap.Add(TaskPropertyIds.AllocatedNodes, PropertyXmlMap.AllocationProperty.Writer);
            taskMap.Add(TaskPropertyIds.Id);
            taskMap.Add(TaskPropertyIds.CommandLine);
            taskMap.Add(TaskPropertyIds.DependsOn);
            taskMap.Add(TaskPropertyIds.IsExclusive);
            taskMap.Add(TaskPropertyIds.IsRerunnable);
            taskMap.Add(TaskPropertyIds.MaxCores, "MaximumNumberOfProcessors");
            taskMap.Add(TaskPropertyIds.MinCores, "MinimumNumberOfProcessors");
            taskMap.Add(TaskPropertyIds.Name);
            taskMap.Add(TaskPropertyIds.ParentJobId);
            taskMap.Add(TaskPropertyIds.RequiredNodes);
            taskMap.Add(TaskPropertyIds.RuntimeSeconds);
            taskMap.Add(TaskPropertyIds.State);
            taskMap.Add(TaskPropertyIds.StdErrFilePath);
            taskMap.Add(TaskPropertyIds.StdInFilePath);
            taskMap.Add(TaskPropertyIds.StdOutFilePath);
            taskMap.Add(TaskPropertyIds.WorkDirectory);

            IRowSet rowset = OpenTaskRowSet(RowSetType.Snapshot);

            taskMap.SetRowSetColumns(rowset);

            if (rowset.GetCount() > 0)
            {
                writer.WriteStartElement(XmlNames.Tasks);

                foreach (PropertyRow row in rowset)
                {
                    writer.WriteStartElement(XmlNames.Task);

                    taskMap.WriteProps(row, writer);

                    int taskId = (int)row[TaskPropertyIds.Id].Value;
                    //Output custom properties
                    TaskEx taskObj = new TaskEx(_jobid, taskId, _helper);
                    PropertyRow allTaskProps = taskObj.GetAllProps();

                    exportCustomPropsAsExtended(allTaskProps, writer);

                    // Write env variables.                                                         
                    TaskEx._ExportEnvToXml(writer, _helper, taskId);

                    writer.WriteEndElement(); // task
                }

                writer.WriteEndElement(); // tasks
            }

            writer.WriteEndElement(); // job

            writer.Flush();
        }


        static PropertyId[] _exportV3XmlPids = 
            { 
                JobPropertyIds.Id,
                JobPropertyIds.State,      
                JobPropertyIds.SubmitTime,
                JobPropertyIds.CreateTime,
                JobPropertyIds.StartTime,                
                JobPropertyIds.Name,
                JobPropertyIds.IsExclusive,
                JobPropertyIds.RunUntilCanceled,
                JobPropertyIds.UnitType,
                JobPropertyIds.RuntimeSeconds,
                JobPropertyIds.Owner,
                JobPropertyIds.UserName,
                JobPropertyIds.Project,
                JobPropertyIds.JobType,
                JobPropertyIds.JobTemplate,
                JobPropertyIds.ExpandedPriority,
                JobPropertyIds.RequestedNodes,
                JobPropertyIds.NodeGroups,
                JobPropertyIds.SoftwareLicense,                
                JobPropertyIds.OrderBy,                
                JobPropertyIds.RequeueCount,
                JobPropertyIds.AutoRequeueCount,                
                JobPropertyIds.PendingReason,
                JobPropertyIds.AutoCalculateMax,
                JobPropertyIds.AutoCalculateMin,
                JobPropertyIds.MinMemory,
                JobPropertyIds.MaxMemory,
                JobPropertyIds.MinCoresPerNode,
                JobPropertyIds.MaxCoresPerNode,
                JobPropertyIds.FailOnTaskFailure,
                JobPropertyIds.Progress,
                JobPropertyIds.ProgressMessage,
                JobPropertyIds.TargetResourceCount,
                JobPropertyIds.MinCores,
                JobPropertyIds.MaxCores,
                JobPropertyIds.MinSockets,
                JobPropertyIds.MaxSockets,
                JobPropertyIds.MinNodes,
                JobPropertyIds.MaxNodes,
                JobPropertyIds.NotifyOnStart,
                JobPropertyIds.NotifyOnCompletion,
                JobPropertyIds.EmailAddress,
                JobPropertyIds.HoldUntil,
                JobPropertyIds.NodeGroupOp,
                JobPropertyIds.SingleNode,
                JobPropertyIds.JobValidExitCodes,
                JobPropertyIds.ParentJobIds,
                JobPropertyIds.EstimatedProcessMemory,
                JobPropertyIds.TaskExecutionFailureRetryLimit,
            };

        protected override PropertyId[] GetExportV3Pids()
        {
            return _exportV3XmlPids;
        }

        //in v2 all job and task properties were exported,
        //so if the server is v2, we need to export all job and task properties
        //from v3 onwards only the props in _exportV3XMlPids as well as custom props
        //and environment variables are exported
        void _ExportV3Xml(XmlWriter writer, XmlExportOptions flags)
        {
            writer.WriteStartElement(XmlNames.Job, XmlNames.NameSpace);
            bool isV2Server = _helper.ServerVersion.IsV2;




            StoreProperty[] propsToExport = null;

            propsToExport = GetPropsToExport(isV2Server);

            // Use a JobPropertyBag to write out the XML.

            JobPropertyBag jobBag = new JobPropertyBag();
            jobBag.SetProperties(propsToExport);

            if (!isV2Server)
            {
                Dictionary<string, string> vars = _helper.ServerWrapper.GetJobEnvVars(_jobid);
                jobBag.SetEnvironmentVariables(vars);
            }

            jobBag.WriteXml(writer, flags | XmlExportOptions.NoJobElement);

            // Export the task group dependencies, using pre-order traverse

            List<int> visitedGrpIds = new List<int>();
            writer.WriteStartElement(XmlNames.Dependencies);
            ExportTaskGroupDependency(writer, this.GetRootTaskGroup(), visitedGrpIds);
            writer.WriteEndElement();

            // Export the tasks

            writer.WriteStartElement(XmlNames.Tasks);

            if (isV2Server)
            {
                //if the server is v2, export all props
                IRowSet rowset = OpenTaskRowSet(RowSetType.Snapshot, TaskRowSetOptions.NoParametricExpansion);

                rowset.SetColumns(TaskPropertyIds.Id);

                foreach (PropertyRow row in rowset)
                {
                    //all props and custom props
                    int taskId = (int)row[TaskPropertyIds.Id].Value;
                    TaskEx taskObj = new TaskEx(_jobid, taskId, _helper);
                    PropertyRow allTaskProps = taskObj.GetAllProps();

                    TaskPropertyBag taskBag = new TaskPropertyBag();
                    taskBag.SetProperties(allTaskProps.Props);

                    //environment variables
                    Dictionary<string, string> taskVars;
                    _helper.ServerWrapper.Task_GetEnvironmentVariables(taskId, out taskVars);
                    taskBag.SetEnvironmentVariables(taskVars);

                    taskBag.WriteXml(writer, flags);
                }
            }
            else
            {

                IRowSet rowset = OpenTaskRowSet(RowSetType.Snapshot, TaskRowSetOptions.NoParametricExpansion);

                rowset.SetColumns(TaskEx._exportV3XmlPids);

                // Find out in advance which tasks have enviroment variables.  For jobs with many tasks, this
                // will much cheaper than trying to load the enviroment variables one-by-one.

                Dictionary<int, bool> taskIdEnvVarMap = new Dictionary<int, bool>();

                int[] taskIdsWithEnvVars;
                _helper.ServerWrapper.Task_FindJobTasksWithEnvVars(this.Id, out taskIdsWithEnvVars);

                if (taskIdsWithEnvVars != null)
                {
                    foreach (int id in taskIdsWithEnvVars)
                    {
                        taskIdEnvVarMap[id] = true;
                    }
                }

                Dictionary<int, StoreProperty[]> customPropsMap = null;

                PropertyRow[] allTaskCustomProperties;
                GetAllTaskCustomProperties(out allTaskCustomProperties);

                if (allTaskCustomProperties != null && allTaskCustomProperties.Length != 0)
                {
                    customPropsMap = new Dictionary<int, StoreProperty[]>(allTaskCustomProperties.Length);
                    foreach (PropertyRow customRow in allTaskCustomProperties)
                    {
                        int taskId = (int)customRow[TaskPropertyIds.Id].Value;

                        List<StoreProperty> taskCustomProps = new List<StoreProperty>(customRow.Props.Length);

                        foreach (StoreProperty prop in customRow.Props)
                        {
                            if (prop.Id != TaskPropertyIds.Id)
                            {
                                taskCustomProps.Add(prop);
                            }
                        }
                        customPropsMap[taskId] = taskCustomProps.ToArray();


                    }
                }


                foreach (PropertyRow row in rowset)
                {
                    TaskPropertyBag taskBag = new TaskPropertyBag();
                    int id = (int)row[TaskPropertyIds.Id].Value;

                    if (customPropsMap == null || !customPropsMap.ContainsKey(id))
                    {
                        taskBag.SetProperties(row.Props);
                    }
                    else
                    {
                        List<StoreProperty> taskProps = new List<StoreProperty>(row.Props);
                        taskProps.AddRange(customPropsMap[id]);
                        taskBag.SetProperties(taskProps.ToArray());
                    }


                    if (taskIdEnvVarMap.ContainsKey(id))
                    {
                        Dictionary<string, string> taskVars;
                        _helper.ServerWrapper.Task_GetEnvironmentVariables(id, out taskVars);
                        taskBag.SetEnvironmentVariables(taskVars);
                    }

                    taskBag.WriteXml(writer, flags);
                }
            }

            writer.WriteEndElement(); // Tasks

            writer.WriteEndElement();  // Job

            writer.Flush();
        }


        /*
         * Schema:
         * <Dependencies>
         *      <Parent GroupId = 8>
         *          <Child GroupId = 9/>
         *          <Child GroupId = 10/>
         *      </Parent>
         *      <Parent GroupId = 9>
         *          <Child GroupId = 10/>
         *          <Child GroupId = 11/>
         *          ...
         *      </Parent>
         *      ...
         * </Dependencies>
         */
        private void ExportTaskGroupDependency(
            XmlWriter writer,
            IClusterTaskGroup parent,
            List<int> visitedGrpIds)
        {
            if (visitedGrpIds.Contains(parent.Id))
            {
                return;
            }

            visitedGrpIds.Add(parent.Id);

            IClusterTaskGroup[] children = parent.GetChildren();

            if (children != null && children.Length > 0)
            {
                writer.WriteStartElement(XmlNames.Parent);
                writer.WriteAttributeString(XmlNames.GroupId, parent.Id.ToString());

                foreach (IClusterTaskGroup child in children)
                {
                    writer.WriteStartElement(XmlNames.Child);
                    writer.WriteAttributeString(XmlNames.GroupId, child.Id.ToString());
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();  // End of Parent

                foreach (IClusterTaskGroup child in children)
                {
                    ExportTaskGroupDependency(writer, child, visitedGrpIds);
                }
            }
        }

        public override void PersistToXml(System.Xml.XmlWriter writer, XmlExportOptions flags)
        {
            if (writer == null)
            {
                throw new SchedulerException(SR.MustProvideXmlWriter);
            }

            if (flags == XmlExportOptions.VersionOneCompatible)
            {
                _ExportV1Xml(writer);
                return;
            }

            _ExportV3Xml(writer, flags);
        }

        public override void RestoreFromXml(System.Xml.XmlReader reader, XmlImportOptions options)
        {
            JobPropertyBag jobbag = new JobPropertyBag();

            jobbag.ReadXML(reader, XmlImportOptions.None);

            jobbag.CommitToJob(_helper, this);
        }

        protected JobState EndAsyncRequest(IAsyncResult result)
        {
            AsyncResult _result = (AsyncResult)result;

            // Wait for it to finish, note that this may
            // be already signalled.

            WaitHandle handle = _result.AsyncWaitHandle;
            if (handle != null)
            {
                handle.WaitOne();
            }

            JobState state = (JobState)_result.ResultState;

            _helper.CloseAsyncResult(_result);

            return state;
        }

        public IClusterTaskGroup GetRootTaskGroup()
        {
            TaskGroupHost host = new TaskGroupHost(_helper, _jobid, _token);

            host.Init();

            return host.GetRootGroup();
        }

        public void DeleteTaskGroup(Int32 groupId)
        {
            _helper.ServerWrapper.TaskGroup_DeleteTaskGroup(_jobid, groupId);
        }

        public void UpdateTaskGroups(IList<int> groupIds)
        {
            _helper.RemoteServer.TaskGroup_UpdateGroupsMaxMin(_token, _jobid, groupIds);
        }

        public void UpdateTaskGroup(Int32 groupId)
        {
            if (_helper.ServerVersion.IsV2)
            {
                throw new NotImplementedException("This is not implemented in V2");
            }
            _helper.RemoteServer.TaskGroup_UpdateGroupMaxMin(_token, _jobid, groupId);
        }


        public IClusterJob CreateChildJob(params StoreProperty[] jobProps)
        {
            StoreProperty[] processedJobProps = PropertyLookup.ProcessSetProps(_helper, ObjectType.Job, jobProps);

            int childJobId = _helper.ServerWrapper.Job_CreateChildJob(_jobid, processedJobProps);

            return new JobEx(childJobId, _token, _helper, null);
        }

        //
        // OLD Task Nice Id methods being deprecated
        //

        public IClusterTask OpenTaskByNiceId(Int32 taskNiceId)
        {
            int taskId = 0;

            _helper.ServerWrapper.Task_FindTaskIdByFriendlyId(_jobid, taskNiceId, ref taskId);

            return new TaskEx(_jobid, taskId, _helper);
        }

        public IClusterTask OpenTaskByNiceId(string taskNiceId)
        {
            int taskId = 0;

            _helper.ServerWrapper.Task_FindTaskIdByFriendlyId(_jobid, taskNiceId, ref taskId);

            return new TaskEx(_jobid, taskId, _helper);
        }

        public IAsyncResult BeginCancelTaskNiceId(Int32 taskNiceId, AsyncCallback callback, object param, string message)
        {
            return BeginCancelTaskNiceId(taskNiceId, CancelRequest.CancelByUser, callback, param, message);
        }

        public IAsyncResult BeginCancelTaskNiceId(Int32 taskNiceId, CancelRequest request, AsyncCallback callback, object param, string message)
        {
            int taskId = 0;

            _helper.ServerWrapper.Task_FindTaskIdByFriendlyId(_jobid, taskNiceId, ref taskId);

            return BeginCancelTask(taskId, request, callback, param, message);
        }

        public void SetEnvironmentVariable(string name, string value)
        {
            _helper.ServerWrapper.SetJobEnvVar(_jobid, name, value);
        }

        public Dictionary<string, string> GetEnvironmentVariables()
        {
            return _helper.ServerWrapper.GetJobEnvVars(_jobid);
        }

        public void GetAllTaskCustomProperties(out PropertyRow[] resultRow)
        {
            _helper.ServerWrapper.Job_GetAllTaskCustomProperties(_jobid, out resultRow);
        }

        public void AddExcludedNodes(string[] nodeNames)
        {
            _helper.ServerWrapper.Job_AddExcludedNodes(_jobid, nodeNames);
        }

        public void RemoveExcludedNodes(string[] nodeNames)
        {
            _helper.ServerWrapper.Job_RemoveExcludedNodes(_jobid, nodeNames);
        }

        public void ClearExcludedNodes()
        {
            _helper.ServerWrapper.Job_ClearExcludedNodes(_jobid);
        }
        /// <summary>
        /// Schedule this job by creating phantom resources on these nodes and set the properties
        /// for the phantom resources on each node
        /// </summary>
        /// <param name="nodeIds">list of nodes to create phantom resources on</param>
        /// <param name="phantomResourceProps">the props common to all the phantom resources of this job</param>
        public void ScheduleOnPhantomResources(int[] nodeIds, StoreProperty[] phantomResourceProps)
        {
            StoreTransaction transaction = _helper.GetTransaction();

            if (transaction == null)
            {
                throw new SchedulerException("ScheduleOnPhantomResources should only be called within a store transaction");
            }




            foreach (int nodeId in nodeIds)
            {
                //create a new array with the phantomresourceprops and fill in the one parameter that 
                //varies from node to node, ie node id
                //fill in the node id for each phantom resource
                //we need to create a new copy for each resource on each node
                StoreProperty[] finalPhantomResourceProps = new StoreProperty[phantomResourceProps.Length + 1];
                phantomResourceProps.CopyTo(finalPhantomResourceProps, 1);
                finalPhantomResourceProps[0] = new StoreProperty(ResourcePropertyIds.NodeId, nodeId);
                transaction.ScheduleJobOnPhantomResource(_jobid, nodeId, finalPhantomResourceProps);
            }

        }


        SchedulerTaskEventDelegate _jobTaskEvent;
        int _subscriptionCnt = 0;
        object _taskEvtLock = new object();

        public event SchedulerTaskEventDelegate TaskEvent
        {
            add
            {
                if (_jobid <= 0)
                {
                    // The job hasn't been initialized
                    throw new NotSupportedException("The job object has not been initialized yet.");
                }

                lock (_taskEvtLock)
                {
                    _jobTaskEvent += value;

                    _subscriptionCnt++;
                    if (_subscriptionCnt == 1)
                    {
                        _helper.RegisterJobTaskEvent(this._jobid, this.JobTaskEventProxy);
                    }
                }
            }

            remove
            {
                lock (_taskEvtLock)
                {
                    _subscriptionCnt--;
                    if (_subscriptionCnt == 0)
                    {
                        _helper.UnregisterJobTaskEvent(this._jobid, this.JobTaskEventProxy);
                    }

                    _jobTaskEvent -= value;
                }
            }
        }

        // This is a wrapper to _jobTaskEvent. We need it because the _jobTaskEvent object changes
        // every time a new handler register to it
        void JobTaskEventProxy(Int32 jobId, Int32 taskSystemId, TaskId taskId, EventType eventType, StoreProperty[] props)
        {
            if (_jobTaskEvent != null)
            {
                _jobTaskEvent(jobId, taskSystemId, taskId, eventType, props);
            }
        }

        public IList<BalanceRequest> GetBalanceRequest()
        {
            IList<BalanceRequest> request;

            _helper.ServerWrapper.Job_GetBalanceRequest(this._jobid, out request);
            return request;
        }
    }

    internal class PropertyXmlMap
    {
        internal abstract class PropertyWriterBase
        {
            internal abstract void WriteProperty(XmlWriter writer, StoreProperty prop);
        }

        internal class DefaultProperty : PropertyWriterBase
        {
            internal override void WriteProperty(XmlWriter writer, StoreProperty prop)
            {
                // The XML parser doesn't recognize the Uppercase True and False
                if (prop.Id.Type == StorePropertyType.Boolean)
                {
                    writer.WriteAttributeString(prop.Id.Name, prop.Value.ToString().ToLowerInvariant());
                }
                else
                {
                    writer.WriteAttributeString(prop.Id.Name, prop.Value.ToString());
                }
            }
        }

        internal class RunTimeProperty : PropertyWriterBase
        {
            internal override void WriteProperty(XmlWriter writer, StoreProperty prop)
            {
                int totalSeconds = (int)prop.Value;

                if (totalSeconds < 60)
                {
                    totalSeconds = 60;
                }

                int days = totalSeconds / 86400;

                totalSeconds -= (days * 86400);

                int hours = totalSeconds / 3600;

                totalSeconds -= (hours * 3600);

                int mins = totalSeconds / 60;

                string val = string.Format("{0}:{1}:{2}", days, hours, mins);

                writer.WriteAttributeString("Runtime", val);
            }
        }

        internal class AllocationProperty : PropertyWriterBase
        {
            static AllocationProperty writer = new AllocationProperty();

            internal static AllocationProperty Writer
            {
                get { return writer; }
            }

            internal override void WriteProperty(XmlWriter writer, StoreProperty prop)
            {
                StringBuilder allocationString = new StringBuilder();
                bool first = true;
                foreach (KeyValuePair<string, int> allocation in (ICollection<KeyValuePair<string, int>>)prop.Value)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        allocationString.Append(",");
                    }

                    allocationString.Append(allocation.Key);
                }

                writer.WriteAttributeString("AllocatedNodes", allocationString.ToString());
            }
        }

        internal class RenameProperty : PropertyWriterBase
        {
            string _name;

            internal RenameProperty(string name)
            {
                _name = name;
            }

            internal override void WriteProperty(XmlWriter writer, StoreProperty prop)
            {
                writer.WriteAttributeString(_name, prop.Value.ToString());
            }
        }

        Dictionary<PropertyId, PropertyWriterBase> _map = new Dictionary<PropertyId, PropertyWriterBase>();

        internal void Add(PropertyId pid)
        {
            _map.Add(pid, new DefaultProperty());
        }

        internal void Add(PropertyId pid, PropertyWriterBase writer)
        {
            _map.Add(pid, writer);
        }

        internal void Add(PropertyId pid, string name)
        {
            _map.Add(pid, new RenameProperty(name));
        }

        internal void SetRowSetColumns(IRowSet rowset)
        {
            List<PropertyId> pids = new List<PropertyId>(_map.Keys);

            rowset.SetColumns(pids.ToArray());
        }

        internal void WriteProps(PropertyRow row, XmlWriter writer)
        {
            foreach (StoreProperty prop in row.Props)
            {
                if (prop.Id != StorePropertyIds.Error)
                {
                    _map[prop.Id].WriteProperty(writer, prop);
                }
            }
        }

        internal void WriteProps(IClusterStoreObject item, XmlWriter writer)
        {
            List<PropertyId> pids = new List<PropertyId>(_map.Keys);

            WriteProps(item.GetProps(pids.ToArray()), writer);
        }
    }

}
