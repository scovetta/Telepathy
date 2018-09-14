//-----------------------------------------------------------------------
// <copyright file="RollingListener.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Abstract trace listener which can roll the text log</summary>
//-----------------------------------------------------------------------

using System.IO;
using System.Diagnostics;
using System;

namespace Microsoft.Hpc.Azure.Common
{
    /// <summary>
    /// The abstract listener class to take TextWriterTraceListener, and roll the output file to prevent file overflow disk
    /// </summary>
    /// <remarks>This is reflected source from Microsoft.Diagnostics.RollingListener in Microsoft.WindowsAzure.RoleContainer.dll</remarks>
    public abstract class RollingListener : TraceListener
    {
        // Fields
        private object _currentListenerLock;
        private FileInfo _currentLogFile;
        private TraceListener _currentTraceListener;
        private bool _forceRollover;
        private bool _isInitialized;
        private readonly string _logExtension;
        private readonly string _logFilename;
        private readonly string _logPath;
        private int _maxLogFiles;
        private long _maxSize;
        public const string MaxLogFilesKeyword = "LogFileLimit";
        public const string MaxSizeKeyword = "LogSizeLimit";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initializeData"></param>
        public RollingListener(string initializeData)
            : base(initializeData)
        {
            this._currentListenerLock = new object();
            this._logFilename = Path.GetFileNameWithoutExtension(initializeData);
            this._logPath = Path.GetDirectoryName(Path.GetFullPath(initializeData));
            this._logExtension = Path.GetExtension(initializeData);
        }

        private void ArchiveFile()
        {
            this.CloseFile();
            if (this._currentLogFile != null)
            {
                try
                {
                    FileInfo info = this.FindReplacementFile();
                    info.Delete();
                    this._currentLogFile.MoveTo(info.FullName);
                    this._currentLogFile.Attributes |= FileAttributes.Archive;
                }
                catch (Exception)
                {
                }
                finally
                {
                    this._currentLogFile = null;
                }
            }
        }

        public override void Close()
        {
            this.CloseAndDispose();
            base.Close();
        }

        private void CloseAndDispose()
        {
            lock (this._currentListenerLock)
            {
                this.CloseFile();
            }
        }

        private void CloseFile()
        {
            try
            {
                if (this._currentTraceListener != null)
                {
                    this.CloseTraceListener(this._currentTraceListener);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                this._currentTraceListener = null;
            }
        }

        protected abstract void CloseTraceListener(TraceListener listener);
        protected override void Dispose(bool disposing)
        {
            this.CloseAndDispose();
            base.Dispose(disposing);
        }

        private FileInfo FindReplacementFile()
        {
            FileInfo info = null;
            for (int i = 0; i < this.MaxLogFiles; i++)
            {
                string str = string.Format("{0}.{1:000}{2}", this._logFilename, i, this._logExtension);
                FileInfo info2 = new FileInfo(Path.Combine(this._logPath, str));
                if (!info2.Exists)
                {
                    return info2;
                }
                if ((info == null) || (info2.LastWriteTimeUtc < info.LastWriteTimeUtc))
                {
                    info = info2;
                }
            }
            return info;
        }

        public override void Flush()
        {
            lock (this._currentListenerLock)
            {
                try
                {
                    if (this._currentTraceListener != null)
                    {
                        this._currentTraceListener.Flush();
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        protected override string[] GetSupportedAttributes()
        {
            return new string[] { "LogFileLimit", "LogSizeLimit" };
        }

        private void Init()
        {
            if (!this._isInitialized)
            {
                if (base.Attributes.ContainsKey("LogFileLimit"))
                {
                    this.MaxLogFiles = int.Parse(base.Attributes["LogFileLimit"]);
                    if (this.MaxLogFiles < 1)
                    {
                        throw new ArgumentException("LogFileLimit must be set to at least one");
                    }
                }
                else
                {
                    this.MaxLogFiles = 5;
                }
                if (base.Attributes.ContainsKey("LogSizeLimit"))
                {
                    this.MaxSize = long.Parse(base.Attributes["LogSizeLimit"]);
                    if (this.MaxLogFiles < 1)
                    {
                        throw new ArgumentException("LogSizeLimit must be set to at least one");
                    }
                }
                else
                {
                    this.MaxSize = 10L;
                }
                this._forceRollover = false;
                this.OpenFile();
                this._isInitialized = true;
            }
        }

        private void OpenFile()
        {
            string str = string.Format("{0}{1}", this._logFilename, this._logExtension);
            FileInfo info = new FileInfo(Path.Combine(this._logPath, str));
            if (!info.Exists)
            {
                info.Create().Close();
                info.Refresh();
            }
            info.Attributes = (info.Attributes & ~FileAttributes.Archive) | FileAttributes.NotContentIndexed;
            TraceListener listener = this.OpenTraceListener(info.FullName);
            listener.Filter = base.Filter;
            this._currentLogFile = info;
            this._currentTraceListener = listener;
        }

        protected abstract TraceListener OpenTraceListener(string path);
        private void RollIfNeeded()
        {
            this.Init();
            bool flag = this._forceRollover;
            if (!flag)
            {
                this._currentLogFile.Refresh();
                if (this._currentLogFile.Length >= ((this.MaxSize * 0x400L) * 0x400L))
                {
                    flag = true;
                }
            }
            if (flag)
            {
                this.ArchiveFile();
                try
                {
                    this.OpenFile();
                    this._forceRollover = false;
                }
                catch (Exception)
                {
                    this._forceRollover = true;
                    throw;
                }
            }
        }

        // Override all the tracelistener output interfaces

        public override void Fail(string message, string detailMessage)
        {
            this.WriteInternal(delegate
            {
                this._currentTraceListener.Fail(message, detailMessage);
            });
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            this.WriteInternal(delegate
            {
                this._currentTraceListener.TraceData(eventCache, source, eventType, id, data);
            });
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            this.WriteInternal(delegate
            {
                this._currentTraceListener.TraceData(eventCache, source, eventType, id, data);
            });
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            this.WriteInternal(delegate
            {
                this._currentTraceListener.TraceEvent(eventCache, source, eventType, id);
            });
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            this.WriteInternal(delegate
            {
                this._currentTraceListener.TraceEvent(eventCache, source, eventType, id, message);
            });
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            this.WriteInternal(delegate
            {
                this._currentTraceListener.TraceEvent(eventCache, source, eventType, id, format, args);
            });
        }

        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            this.WriteInternal(delegate
            {
                this._currentTraceListener.TraceTransfer(eventCache, source, id, message, relatedActivityId);
            });
        }

        public override void Write(string message)
        {
            this.WriteInternal(delegate
            {
                this._currentTraceListener.Write(message);
            });
        }

        public override void WriteLine(string message)
        {
            this.WriteInternal(delegate
            {
                this._currentTraceListener.WriteLine(message);
            });
        }

        /// <summary>
        /// Internal implement to make sure the writing output will rollover to new file if exceed the size
        /// </summary>
        /// <param name="a"></param>
        private void WriteInternal(Action a)
        {
            Action action = null;
            lock (this._currentListenerLock)
            {
                this.RollIfNeeded();
                try
                {
                    a();
                }
                catch (IOException)
                {
                    this._forceRollover = true;
                    if (action == null)
                    {
                        action = delegate
                        {
                            try
                            {
                                a();
                            }
                            catch (Exception)
                            {
                            }
                        };
                    }
                    this.WriteInternal(action);
                }
            }
        }

        /// <summary>
        /// This tracelistener should be thread safe
        /// </summary>
        public override bool IsThreadSafe
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Max log file number, will be like 001, 002, etc
        /// </summary>
        public int MaxLogFiles
        {
            get
            {
                return this._maxLogFiles;
            }
            set
            {
                this._maxLogFiles = value;
            }
        }

        /// <summary>
        /// Max file size for each log file
        /// </summary>
        public long MaxSize
        {
            get
            {
                return this._maxSize;
            }
            set
            {
                this._maxSize = value;
            }
        }
    }
}