//------------------------------------------------------------------------------
// <copyright file="SimpleThreadPool.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">sayanch</owner>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.Hpc
{
    internal class SimpleMultiThreadMessageQueue<T>
    {

        internal delegate void ProcessMessageDelegate(T msg);

        /// <summary>
        /// Lock to protect the queue of messages
        /// </summary>
        object _msgHandlerLock = new object();

        /// <summary>
        /// The queue of messages
        /// </summary>
        Queue<T> _msgQueue = new Queue<T>();

        /// <summary>
        /// Threads used to process messages
        /// </summary>
        Thread[] _msgThreads = null;

        /// <summary>
        /// Number of message processing threads
        /// </summary>
        int _numMsgThreads = 1;

        /// <summary>
        /// Event used to signal that a new message has appeared
        /// </summary>
        AutoResetEvent _msgEvent = new AutoResetEvent(false);

        /// <summary>
        /// How long should the message processing thread wait on the event for a new message to appear
        /// </summary>
        int _msgEventTimeout = 1000; //ms 

        /// <summary>
        /// Sleep between one thread processing consecutive message entries     
        /// </summary>
        int _msgSleep = 10; // ms         

        /// <summary>
        /// The handler provided by the user to process a message
        /// </summary>
        ProcessMessageDelegate _msgHandler = null;

        /// <summary>
        /// Has it been stopped ?
        /// </summary>
        bool _isStopped = false;
        
        /// <summary>
        /// Create a simple thread pool with relevant parameters
        /// </summary>
        /// <param name="numMsgThreads"></param>
        /// <param name="msgEventTimeout"></param>
        /// <param name="msgSleep"></param>
        /// <param name="msgHandler"></param>
        public SimpleMultiThreadMessageQueue(int numMsgThreads, int msgEventTimeout, int msgSleep, ProcessMessageDelegate msgHandler)
        {
            _numMsgThreads = numMsgThreads;
            _msgEventTimeout = msgEventTimeout;
            _msgSleep = msgSleep;
            _msgHandler = msgHandler;

            _msgThreads = new Thread[_numMsgThreads];

            for (int i = 0; i < _numMsgThreads; i++)
            {
                _msgThreads[i] = new Thread(new ThreadStart(this.MsgProcess));
                _msgThreads[i].IsBackground = true;
            }

        }

        /// <summary>
        /// Start the threads that will process messages in this thread pool
        /// </summary>
        internal void Start()
        {
            for (int i = 0; i < _numMsgThreads; i++)
            {
                _msgThreads[i].Start();
            }
        }


        /// <summary>
        /// Trigger the stopping of the message processing threads
        /// </summary>
        internal void Stop()
        {
            _isStopped = true;
        }

        /// <summary>
        /// Method used by the threads to process messages in the queue
        /// </summary>
        private void MsgProcess()
        {

            while (!_isStopped)
            {
                try
                {
                    //Wait for the new queue event or wait for it to timeout
                    _msgEvent.WaitOne(_msgEventTimeout);

                    //Irrespective of whether the event was generated or simply timeout 
                    //check for the next event

                    T entry;
                    while (GetNextHeartBeatEntryToProcess(out entry))
                    {
                        if (_msgHandler != null)
                        {
                            _msgHandler(entry);
                            Thread.Sleep(_msgSleep);
                        }
                        
                    }

                }
                catch (Exception e)
                {
                    //We should not let this thread die other than exiting because it stopped.
                    Debug.WriteLine("[{0}] SimpleThread Pool suffered exception {1}", Thread.CurrentThread.ManagedThreadId, e);
                }
            }
        }

        /// <summary>
        /// Get the next  message to be processed from the queue of messages
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>        
        private bool GetNextHeartBeatEntryToProcess(out T entry)
        {
            entry = default(T);
            lock (_msgHandlerLock)
            {
                if (_msgQueue.Count == 0)
                {
                    //if the queue is empty return false;
                    return false;
                }
                entry = _msgQueue.Dequeue();
            }

            return true;
        }

        /// <summary>
        /// Add the message to the queue 
        /// </summary>
        /// <param name="entry"></param>
        internal void AddMessageToPool(T entry)
        {
            lock (_msgHandlerLock)
            {
                _msgQueue.Enqueue(entry);
                _msgEvent.Set();
            }
        }

    }
}