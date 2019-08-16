using System;
using System.Threading.Tasks;

namespace TelepathyCommon
{
#if net40
    using Microsoft.Diagnostics.Tracing;
#else
    using System.Diagnostics.Tracing;
#endif

    [EventSource(Name = "Microsoft-HpcApplication-HpcCommon")]
    public sealed class CommonEventSource : EventSource
    {
        private const int CommonEventSourceIdStart = 10000;
        public static readonly CommonEventSource Current = new CommonEventSource();

        static CommonEventSource()
        {
            // A workaround for the problem where ETW activities do not get tracked until Tasks infrastructure is initialized.
            // This problem will be fixed in .NET Framework 4.6.2.
#if net40
            TaskEx.Run(() => { }).Wait();
#else
            Task.Run(() => { }).Wait();
#endif
        }

        // Instance constructor is private to enforce singleton semantics
        private CommonEventSource() : base() { }

#region Keywords
        // Event keywords can be used to categorize events. 
        // Each keyword is a bit flag. A single event can be associated with multiple keywords (via EventAttribute.Keywords property).
        // Keywords must be defined as a public class named 'Keywords' inside EventSource that uses them.
        public static class Keywords
        {
            public const EventKeywords General = (EventKeywords)0x1L;
            public const EventKeywords Registry = (EventKeywords)0x2L;
            public const EventKeywords PerformanceCounter = (EventKeywords)0x3L;
        }
#endregion

#region Events
        // Define an instance method for each event you want to record and apply an [Event] attribute to it.
        // The method name is the name of the event.
        // Pass any parameters you want to record with the event (only primitive integer types, DateTime, Guid & string are allowed).
        // Each event method implementation should check whether the event source is enabled, and if it is, call WriteEvent() method to raise the event.
        // The number and types of arguments passed to every event method must exactly match what is passed to WriteEvent().
        // Put [NonEvent] attribute on all methods that do not define an event.
        // For more information see https://msdn.microsoft.com/en-us/library/system.diagnostics.tracing.eventsource.aspx

        [NonEvent]
        public void Message(string message, params object[] args)
        {
            if (this.IsEnabled())
            {
                string finalMessage = string.Format(message, args);
                Message(finalMessage);
            }
        }

        [Event(HpcConstants.HpcCommonEventIdStart + 1, Level = EventLevel.Informational, Message = "{0}", Keywords = Keywords.General)]
        public void Message(string message)
        {
            if (this.IsEnabled())
            {
                WriteEvent(CommonEventSourceIdStart + 1, message);
            }
        }

        [Event(HpcConstants.HpcCommonEventIdStart + 2, Level = EventLevel.Informational, Message = "Registry monitored a non-notification event. cached {0}, oldValue {1}, exist {2} newValue {3}", Keywords = Keywords.Registry)]
        public void RegistryNonNotification<T>(bool cached, T oldValue, bool exist, T newValue)
        {
            WriteEvent(CommonEventSourceIdStart + 2, cached, oldValue, exist, newValue);
        }

        [Event(HpcConstants.HpcCommonEventIdStart + 3, Level = EventLevel.Error, Message = "Exception while setup performance category {0}, ex {1}", Keywords = Keywords.PerformanceCounter)]
        public void ErrorInstallPerformanceCounter(string category, Exception ex)
        {
            WriteEvent(CommonEventSourceIdStart + 3, category, ex);
        }

        [Event(HpcConstants.HpcCommonEventIdStart + 4, Level = EventLevel.Informational, Message = "Installed performance counter category {0}", Keywords = Keywords.PerformanceCounter)]
        public void InstalledPerformanceCounter(string category)
        {
            WriteEvent(CommonEventSourceIdStart + 4, category);
        }

        [Event(HpcConstants.HpcCommonEventIdStart + 5, Level = EventLevel.Critical, Message = "Service aborted", Keywords = Keywords.General)]
        public void ServiceAborted()
        {
            WriteEvent(CommonEventSourceIdStart + 5);
        }

        [Event(HpcConstants.HpcCommonEventIdStart + 6, Level = EventLevel.Critical, Message = "Replica role changed to {0}", Keywords = Keywords.General)]
        public void OnChangeRole(string newRole)
        {
            WriteEvent(CommonEventSourceIdStart + 6, newRole);
        }

        [Event(HpcConstants.HpcCommonEventIdStart + 7, Level = EventLevel.Critical, Message = "Service is closed", Keywords = Keywords.General)]
        public void OnCloseService()
        {
            WriteEvent(CommonEventSourceIdStart + 7);
        }

        [Event(HpcConstants.HpcCommonEventIdStart + 8, Level = EventLevel.Critical, Message = "Service is opened", Keywords = Keywords.General)]
        public void OnOpenService()
        {
            WriteEvent(CommonEventSourceIdStart + 8);
        }

        [Event(HpcConstants.HpcCommonEventIdStart + 9, Level = EventLevel.Critical, Message = "Create listeners", Keywords = Keywords.General)]
        public void OnCreateListeners()
        {
            WriteEvent(CommonEventSourceIdStart + 9);
        }

        [Event(HpcConstants.HpcCommonEventIdStart + 10, Level = EventLevel.Critical, Message = "Cancel triggered", Keywords = Keywords.General)]
        public void OnCancelTriggered()
        {
            WriteEvent(CommonEventSourceIdStart + 10);
        }

#endregion
    }
}
