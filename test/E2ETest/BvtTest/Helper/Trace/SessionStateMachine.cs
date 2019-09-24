// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Test.E2E.Bvt.Helper.Trace
{
    using System;
    using System.Collections.Generic;

    class SessionStateMachine
    {
        private SessionState state = SessionState.BeforeCreateSession;
        private Dictionary<SessionState, Dictionary<SessionAction, Action>> rule;

        private void InitRule()
        {
            this.rule = new Dictionary<SessionState, Dictionary<SessionAction, Action>>();

            foreach (var e in Enum.GetValues(typeof(SessionState)))
            {
                this.rule[(SessionState)e] = new Dictionary<SessionAction, Action>();
            }

            this.rule[SessionState.BeforeCreateSession][SessionAction.BeginCreateSession] = () => { this.state = SessionState.SessionCreating; };

            this.rule[SessionState.SessionCreating][SessionAction.CreatedSession] = () => { this.state = SessionState.SessionRunning; };

            this.rule[SessionState.SessionSuspended][SessionAction.RasieSession] = () => { this.state = SessionState.SessionRunning; };
            this.rule[SessionState.SessionSuspended][SessionAction.FinishSession] = () => { this.state = SessionState.SessionClosed; };

            this.rule[SessionState.SessionRunning][SessionAction.SuspendSession] = () => { this.state = SessionState.SessionSuspended; };
            this.rule[SessionState.SessionRunning][SessionAction.SuspendSession] = () => { this.state = SessionState.SessionSuspended; };
            this.rule[SessionState.SessionRunning][SessionAction.FinishSession] = () => { this.state = SessionState.SessionClosed; };
            this.rule[SessionState.SessionRunning][SessionAction.ProcessRequests] = () => { this.state = SessionState.SessionRunning; };
        }

        public SessionStateMachine()
        {
            this.InitRule();
        }

        public void Process(SessionAction action)
        {
            try
            {
                this.rule[this.state][action]();
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidOperationException(string.Format("Invalid state transistion from {0} with action {1}", this.state, action));
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
