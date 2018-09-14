//------------------------------------------------------------------------------
// <copyright file="ComplexDataContainer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Complex data container that has more than 1 data store path
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.DataContainer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using Microsoft.Hpc.Scheduler.Session.Data.Internal;

    /// <summary>
    /// Complex data container that has more than 1 data store paths
    /// </summary>
    internal class ComplexDataContainer : IDataContainer
    {
#region private fields
        /// <summary>
        /// primary data container
        /// </summary>
        private IDataContainer primaryContainer;

        /// <summary>
        /// secondary data containers
        /// </summary>
        private List<IDataContainer> secondaryContainers = new List<IDataContainer>();

#endregion

        /// <summary>
        /// Initializes a new instance of the ComplexDataContainer class
        /// </summary>
        /// <param name="primaryContainerPath">primary data container path</param>
        /// <param name="secondaryContainerPaths">secondary data container paths</param>
        public ComplexDataContainer(string primaryContainerPath, List<string> secondaryContainerPaths)
        {
            TraceHelper.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[ComplexDataContainer].Constructor: primary path={0}, secondary path count={1}", primaryContainerPath, secondaryContainerPaths.Count);

            this.primaryContainer = DataContainerHelper.GetDataContainer(primaryContainerPath);
            foreach (string secondaryContainerPath in secondaryContainerPaths)
            {
                this.secondaryContainers.Add(DataContainerHelper.GetDataContainer(secondaryContainerPath));
            }
        }

        /// <summary>
        /// Gets data container id
        /// </summary>
        public string Id
        {
            get
            {
                return this.primaryContainer.Id;
            }
        }

        /// <summary>
        /// Returns a path that tells where the data is stored
        /// </summary>
        /// <returns>path telling where the data is stored</returns>
        public string GetStorePath()
        {
            return this.primaryContainer.GetStorePath();
        }

        /// <summary>
        /// Get the content Md5
        /// </summary>
        /// <returns>The base64 md5 string</returns>
        public string GetContentMd5()
        {
            return this.primaryContainer.GetContentMd5();
        }

        /// <summary>
        /// Write a data item into data container and flush
        /// </summary>
        /// <param name="data">data content to be written</param>
        public void AddDataAndFlush(DataContent data)
        {
            TraceHelper.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[ComplexDataContainer] .AddDataAndFlush");

            CountdownEvent doneEvent = new CountdownEvent(this.secondaryContainers.Count);
            foreach (IDataContainer secondaryContainer in this.secondaryContainers)
            {
                ThreadPool.QueueUserWorkItem(
                    delegate 
                    {
                        try
                        {
                            secondaryContainer.AddDataAndFlush(data);
                        }
                        catch (Exception ex)
                        {
                            // do not throw exception for secondary containers
                            TraceHelper.TraceSource.TraceEvent(TraceEventType.Error, 0, "[ComplexDataContainer] .AddDataAndFlush: container {0} receives exception = {1}", secondaryContainer.Id, ex);
                        }
                        finally
                        {
                            doneEvent.Signal();
                        }
                    });
            }

            Exception exception = null;
            try
            {
                this.primaryContainer.AddDataAndFlush(data);
            }
            catch(DataException ex)
            {
                exception = ex;
            }

            doneEvent.Wait();

            // if the primary container is in problem, or is deleted, delete the secondary containers
            if (exception != null || !this.primaryContainer.Exists())
            {
                foreach (IDataContainer secondaryContainer in this.secondaryContainers)
                {
                    try
                    {
                        secondaryContainer.DeleteIfExists();
                    }
                    catch (Exception ex)
                    {
                        TraceHelper.TraceSource.TraceEvent(TraceEventType.Error, 0, "[ComplexDataContainer] .AddDataAndFlush: delete container {0} receives exception = {1}", secondaryContainer.Id, ex);
                    }
                }
            }
        }

        /// <summary>
        /// Get data content from the data container
        /// </summary>
        /// <returns>data content in the data container</returns>
        public byte[] GetData()
        {
            TraceHelper.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[ComplexDataContainer] .GetData");

            return this.primaryContainer.GetData();
        }

        /// <summary>
        /// Delete the data container.
        /// </summary>
        public void DeleteIfExists()
        {
            foreach (IDataContainer secondaryContainer in this.secondaryContainers)
            {
                secondaryContainer.DeleteIfExists();
            }

            this.primaryContainer.DeleteIfExists();
        }

        /// <summary>
        /// Check if the data container exists on data server or not
        /// </summary>
        /// <returns>true if the data container exists, false otherwise</returns>
        public bool Exists()
        {
            return this.primaryContainer.Exists();
        }
    }
}
