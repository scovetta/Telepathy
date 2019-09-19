// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AITestLib.Helper.Trace
{
    class SessionStateMachine
    {
        private SessionState state = SessionState.BeforeCreateSession;
        private Dictionary<SessionState, Dictionary<SessionAction, Action>> rule;

        private void InitRule()
        {
            rule = new Dictionary<SessionState, Dictionary<SessionAction, Action>>();

            foreach (var e in Enum.GetValues(typeof(SessionState)))
            {
                rule[(SessionState)e] = new Dictionary<SessionAction, Action>();
            }

            rule[SessionState.BeforeCreateSession][SessionAction.BeginCreateSession] = () => { state = SessionState.SessionCreating; };

            rule[SessionState.SessionCreating][SessionAction.CreatedSession] = () => { state = SessionState.SessionRunning; };

            rule[SessionState.SessionSuspended][SessionAction.RasieSession] = () => { state = SessionState.SessionRunning; };
            rule[SessionState.SessionSuspended][SessionAction.FinishSession] = () => { state = SessionState.SessionClosed; };

            rule[SessionState.SessionRunning][SessionAction.SuspendSession] = () => { state = SessionState.SessionSuspended; };
            rule[SessionState.SessionRunning][SessionAction.SuspendSession] = () => { state = SessionState.SessionSuspended; };
            rule[SessionState.SessionRunning][SessionAction.FinishSession] = () => { state = SessionState.SessionClosed; };
            rule[SessionState.SessionRunning][SessionAction.ProcessRequests] = () => { state = SessionState.SessionRunning; };
        }

        public SessionStateMachine()
        {
            InitRule();
        }

        public void Process(SessionAction action)
        {
            try
            {
                rule[state][action]();
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidOperationException(string.Format("Invalid state transistion from {0} with action {1}", state, action));
            }
        }
    }

    enum SessionAction
    {
        BeginCreateSession,
        CreatedSession,
        FinishSession,
        SuspendSession,
        RasieSession,
        ProcessRequests
    }

    enum SessionState
    {
        BeforeCreateSession,
        SessionCreating,
        SessionRunning,
        SessionClosed,
        SessionSuspended
    }
}
