using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

using Microsoft.Hpc.Scheduler.AddInFilter.HpcClient;

namespace Microsoft.Hpc.Scheduler.AddInFilter.HpcServer
{
    /// <summary>
    /// Here we transition into a tighter access model.  The intent is that customers will be restricted in
    /// their ability to reflect and/or execute this internal code (business logic)
    /// </summary>
    public sealed class FilterInternal
    {
        private IActivationFilter _actvFilter = null;  // holds instance of activation filter used by this instance
        private ISubmissionFilter _subfilter = null;   // holds instance of submission filter used by this instance
        private IFilterLifespan _filterLifespan = null;  // holds interface to call if optional lifespan features are desired by filter author
        private AdminUserGateKeeper _gatekeeper = new AdminUserGateKeeper();

        /// <summary>
        /// Here we look at the current object to see if it implements any of our interesting interfaces.
        /// </summary>
        /// <param name="possible"></param>
        private void FindInterestingInterfaces(object possible)
        {
            if (null == _actvFilter)
            {
                _actvFilter = possible as IActivationFilter;
            }

            if (null == _subfilter)
            {
                _subfilter = possible as ISubmissionFilter;
            }

            if (null == _filterLifespan)
            {
                _filterLifespan = possible as IFilterLifespan;
            }
        }

        /// <summary>
        /// This will crawl the given assembly and determine which, if any, filter interfaces are implmented.
        /// </summary>
        /// <param name="filterFile"></param>
        /// <param name="loadDataBytes"></param>
        public void LoadFilter(string filterFile, out byte[] loadDataBytes)
        {
            using (DisposableLock adminLock = _gatekeeper.GetAdminLock())
            {
                if ((null != _actvFilter) || (null != _subfilter) || (null != _filterLifespan))
                {
                    throw new ArgumentException("Current filter has already been loaded.", filterFile);
                }

                // ok load the customer's filter assembly and start reflecting into it
                Assembly filterAssembly = Assembly.LoadFile(filterFile);
                
                // remember that any single class can implement any of our interfaces...
                // so we poke around looking but avoid making multiple instances of a class that implementents
                // multiple interfaces.
                object candidat = ReflectionHelper.LoadSingleInterface<IActivationFilter>(filterAssembly, filterFile) as IActivationFilter;
                FindInterestingInterfaces(candidat);

                if (null == _subfilter)
                {
                    candidat = ReflectionHelper.LoadSingleInterface<ISubmissionFilter>(filterAssembly, filterFile) as ISubmissionFilter;
                    FindInterestingInterfaces(candidat);
                }

                if (null == _filterLifespan)
                {
                    candidat = ReflectionHelper.LoadSingleInterface<IFilterLifespan>(filterAssembly, filterFile) as IFilterLifespan;
                    FindInterestingInterfaces(candidat);
                }
            }

            // now that the filter is loaded, create and populate a summary
            // of the load experience to be returned to caller

            AddInLoadData loadData = new AddInLoadData();

            loadData._activationFilterFound = (null != _actvFilter);
            loadData._submissionFilterFound = (null != _subfilter);
            loadData._filterLifespanFound = (null != _filterLifespan);

            loadDataBytes = loadData.Serialize();
        }

        public void OnFilterLoad()
        {
            using (DisposableLock adminLock = _gatekeeper.GetAdminLock())
            {
                ValideteHasLifespanInterface();

                if (null != _filterLifespan)
                {
                    _filterLifespan.OnFilterLoad();
                }
            }
        }

        public void OnFilterUnload()
        {
            using (DisposableLock adminLock = _gatekeeper.GetAdminLock())
            {
                ValideteHasLifespanInterface();

                if (null != _filterLifespan)
                {
                    _filterLifespan.OnFilterUnload();
                }
            }
        }

        public int FilterActivation(byte[] jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount)
        {
            using (DisposableLock userLock = _gatekeeper.GetUserLock())
            {
                ValidateIsActivationFilter();

                using (MemoryStream xmlStream = new MemoryStream(jobXml))
                {
                    ActivationFilterResponse retVal = _actvFilter.FilterActivation(xmlStream, schedulerPass, jobIndex, backfill, resourceCount);

                    return (int)retVal;
                }
            }
        }

        public void RevertActivation(byte[] jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount)
        {
            using (DisposableLock userLock = _gatekeeper.GetUserLock())
            {
                ValidateIsActivationFilter();

                using (MemoryStream xmlStream = new MemoryStream(jobXml))
                {
                    if (null != _filterLifespan)
                    {
                        _actvFilter.RevertActivation(xmlStream, schedulerPass, jobIndex, backfill, resourceCount);
                    }
                }
            }
        }

        public int FilterSubmission(ref byte[] jobXml)
        {
            using (DisposableLock userLock = _gatekeeper.GetUserLock())
            {
                ValidateIsSubmissionFilter();

                using (MemoryStream xmlInStream = new MemoryStream(jobXml))
                {
                    Stream xmlOutStream;

                        // we now call the filter providing a stream based on the job xml and get an output stream
                        // for any modifications.
                    SubmissionFilterResponse retVal = _subfilter.FilterSubmission(xmlInStream, out xmlOutStream);

                    if (SubmissionFilterResponse.SuccessJobChanged == retVal) // need to push modified data back up call chain
                    {
                        if (null == xmlOutStream)
                        {
                            throw new ArgumentNullException("jobXmlModified", "You must create and return a modified job XML stream if the job is signaled as modified.");
                        }

                        xmlOutStream.Flush();

                        byte[] outXml = new byte[xmlOutStream.Length];

                        xmlOutStream.Position = 0; // rewind

                        xmlOutStream.Read(outXml, 0, (int)xmlOutStream.Length);

                        jobXml = outXml; // return modified job xml to call stack
                    }

                    return (int)retVal;
                }
            }
        }

        public void RevertSubmission(byte[] jobXml)
        {
            using (DisposableLock userLock = _gatekeeper.GetUserLock())
            {
                ValidateIsSubmissionFilter();

                using (Stream xmlStream = new MemoryStream(jobXml))
                {
                    if (null != _filterLifespan)
                    {
                        _subfilter.RevertSubmission(xmlStream);
                    }
                }
            }
        }

        private void ValidateIsActivationFilter()
        {
            if (null == _actvFilter)
            {
                throw new ArgumentNullException("ActivationFilterInterface", "Attempt to call an Activation Filter where none was found.");
            }
        }

        private void ValidateIsSubmissionFilter()
        {
            if (null == _subfilter)
            {
                throw new ArgumentNullException("SubmissionFilterInterface", "Attempt to call a Submission Filter where none was found.");
            }
        }

        private void ValideteHasLifespanInterface()
        {
            if (null == _filterLifespan)
            {
                throw new ArgumentNullException("FilterLifespanInterface", "Attempt to call Filter Lifepan methods where none were found.");
            }
        }

        public FilterInternal()
        {
        }
    }
}
