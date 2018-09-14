//-----------------------------------------------------------------------------
// <copyright file="BlobTransferTraceSource.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
// BlobTransferTraceSource class, 
// </summary>
//-----------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement
{
    using System;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Threading;

    internal class BlobTransferTraceSource
    {
        /// <summary>
        /// Initializes a static BlobTransferTraceSource object, 
        /// we can only have one BlobTransferTraceSource in a process.
        /// </summary>
        private static BlobTransferTraceSource blobTransferTraceSource = new BlobTransferTraceSource();

        /// <summary>
        /// TraceSource object.
        /// </summary>
        private TraceSource traceSource = new TraceSource("DataMovement", SourceLevels.Off);
        
        /// <summary>
        /// Prevents a default instance of the BlobTransferTraceSource class from being created.
        /// Initializes a new instance of the <see cref="BlobTransferTraceSource"/> class.
        /// </summary>
        private BlobTransferTraceSource()
        {
        }

        /// <summary>
        /// Gets the custom switch attributes defined in the application configuration
        /// file.
        /// </summary>
        /// <value>
        /// A System.Collections.Specialized.StringDictionary object containing 
        /// the custom attributes for the trace switch.
        /// </value>
        public StringDictionary Attributes 
        { 
            get
            {
                return this.traceSource.Attributes;
            }
        }

        /// <summary>
        /// Gets the collection of trace listeners for the trace source.
        /// </summary>
        /// <value>
        /// A System.Diagnostics.TraceListenerCollection that contains the active trace
        /// listeners associated with the source.
        /// </value>
        public TraceListenerCollection Listeners 
        {
            get 
            {
                return this.traceSource.Listeners;
            }
        }

        /// <summary>
        /// Gets or sets the source switch value.
        /// </summary>
        /// <value>
        /// A System.Diagnostics.SourceSwitch object representing the source switch value.
        /// </value>
        public SourceSwitch Switch 
        {
            get
            {
                return this.traceSource.Switch;
            }

            set
            {
                this.traceSource.Switch = value;
            }
        }

        /// <summary>
        /// Gets an Instance of BlobTransferTraceSource.
        /// </summary>
        /// <returns>Instance of BlobTransferTraceSource.</returns>
        public static BlobTransferTraceSource GetInstance()
        {
            return blobTransferTraceSource;
        }

        /// <summary>
        /// Closes all the trace listeners in the trace listener collection.
        /// </summary>
        public void Close()
        {
            this.traceSource.Close();
        }

        /// <summary>
        /// Flushes all the trace listeners in the trace listener collection.
        /// </summary>
        public void Flush()
        {
            this.traceSource.Flush();
        }
        
        /// <summary>
        /// Writes trace data to the trace listeners in the System.Diagnostics.TraceSource.Listeners
        /// collection using the specified event type, event identifier, and trace data.
        /// </summary>
        /// <param name="eventType">One of the enumeration values that specifies 
        /// the event type of the trace data.</param>
        /// <param name="id">A numeric identifier to identify a specific event.</param>
        /// <param name="data">The trace data.</param>
        [Conditional("TRACE")]
        public void TraceData(TraceEventType eventType, int id, object data)
        {
            this.TraceHelper(
                delegate
                {
                    this.traceSource.TraceData(eventType, id, data);
                });
        }

        /// <summary>
        /// Writes trace data to the trace listeners in the System.Diagnostics.TraceSource.Listeners
        /// collection using the specified event type, and trace data
        /// array.
        /// </summary>
        /// <param name="eventType">One of the enumeration values that specifies 
        /// the event type of the trace data.
        /// </param>
        /// <param name="id">A numeric identifier to identify a specific event.</param>
        /// <param name="data">An object array containing the trace data.</param>
        [Conditional("TRACE")]
        public void TraceData(TraceEventType eventType, int id, params object[] data)
        {
            this.TraceHelper(
                delegate
                {
                    this.traceSource.TraceData(eventType, id, data);
                });
        }

        /// <summary>
        /// Writes a trace event message to the trace listeners in the System.Diagnostics.TraceSource.Listeners
        /// collection using the specified event type and event identifier.
        /// </summary>
        /// <param name="eventType">
        /// One of the enumeration values that specifies the event type of the trace
        /// data.</param>
        /// <param name="id">A numeric identifier to identify a specific event.</param>
        [Conditional("TRACE")]
        public void TraceEvent(TraceEventType eventType, int id)
        {
            this.TraceHelper(
                delegate
                {
                    this.traceSource.TraceEvent(eventType, id);
                });
        }

        /// <summary>
        /// Writes a trace event message to the trace listeners in the System.Diagnostics.TraceSource.Listeners
        /// collection using the specified event type, event identifier, and message.
        /// </summary>
        /// <param name="eventType">One of the enumeration values that specifies 
        /// the event type of the trace data.</param>
        /// <param name="id">A numeric identifier to identify a specific event.</param>
        /// <param name="message">The trace message to write.</param>
        [Conditional("TRACE")]
        public void TraceEvent(TraceEventType eventType, int id, string message)
        {
            this.TraceHelper(
                delegate
                {
                    this.traceSource.TraceEvent(eventType, id, message);
                });
        }

        /// <summary>
        /// Writes a trace event to the trace listeners in the System.Diagnostics.TraceSource.Listeners
        /// collection using the specified event type, event identifier, and argument
        /// array and format. 
        /// </summary>
        /// <param name="eventType">One of the enumeration values that specifies 
        /// the event type of the trace data.</param>
        /// <param name="id">A numeric identifier to identify a specific event.</param>
        /// <param name="format">A composite format string that contains text intermixed with
        ///  zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        [Conditional("TRACE")]
        public void TraceEvent(TraceEventType eventType, int id, string format, params object[] args)
        {
            this.TraceHelper(
                delegate
                {
                    this.traceSource.TraceEvent(eventType, id, format, args);
                });
        }

        /// <summary>
        /// Writes an informational message to the trace listeners in the System.Diagnostics.TraceSource.Listeners
        /// collection using the specified message.
        /// </summary>
        /// <param name="message">The informative message to write.</param>
        [Conditional("TRACE")]
        public void TraceInformation(string message)
        {
            this.TraceHelper(
                delegate
                {
                    this.traceSource.TraceInformation(message);
                });
        }

        /// <summary>
        /// Writes an informational message to the trace listeners in the System.Diagnostics.TraceSource.Listeners
        /// collection using the specified object array and formatting information.
        /// </summary>
        /// <param name="format">A composite format string (see Remarks) that contains text intermixed with
        /// zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An array containing zero or more objects to format.</param>
        [Conditional("TRACE")]
        public void TraceInformation(string format, params object[] args)
        {
            this.TraceHelper(
                delegate
                {
                    this.traceSource.TraceInformation(format, args);
                });
        }

        private void TraceHelper(Action trace)
        {
            try
            {
                trace();
            }
            catch (Exception)
            {
                // Caught an exception when try to trace, ignore it.
            }
        }
    }
}
