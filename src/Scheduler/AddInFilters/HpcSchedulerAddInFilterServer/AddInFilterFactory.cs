using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading;

using System.AddIn.Hosting;
using System.AddIn.Pipeline;

using Microsoft.Hpc.Scheduler.AddInFilter.HpcClient;

using System.Reflection;

namespace Microsoft.Hpc.Scheduler.AddInFilter.HpcServer
{

    internal class AddInFactoryConstants
    {
        internal const int MaxSecondsForBulkRetirements = 15;
        internal const int DefaultMaxCallTimeoutMillSecs = 20000;
    }

    /// <summary>
    /// Manages lifespan of AddIn filters (load, use, unload).
    /// </summary>
	public class AddInFilterFactory : IAddInFilterFactory
	{
        private string _addinRoot = null;        // holds root of our addin tree
        private string[] _addinWarnings = null;  // holds any warnings from System.AddIn pipeline infrastructure
        private SortedList<string, AddInFilter> _loadedAddIns = new SortedList<string, AddInFilter>(StringComparer.InvariantCultureIgnoreCase);
        private object _loadedAddInsLock = new object();

        public string[] GetPipelineWarnings()
        { 
            return _addinWarnings;
        }

        public string MakeCanonicalFilterName(string filterNameFragment)
        {
            string baseLocation = Environment.GetEnvironmentVariable("CCP_DATA");
            string filterDir = Path.Combine(baseLocation, "Filters");

            string filterName = Path.Combine(filterDir, filterNameFragment);

            return filterName;
        }

        public void InitAddInPipeline()
        {
            string hpcHome = Environment.GetEnvironmentVariable("CCP_HOME");
            string bin = Path.Combine(hpcHome, "Bin");
            string defaultAddInRoot = Path.Combine(bin, "AddInPipeline");

            InitAddInPipelineWithRoot(defaultAddInRoot);
        }

        public void InitAddInPipelineWithRoot(string addinRoot)
        {
            ValidateInternalAgreesWithClient();

            AddInFilterFactory factoryRet = new AddInFilterFactory();
            
            _addinRoot = addinRoot;

            //Check to see if new AddIns have been installed
            _addinWarnings = AddInStore.Rebuild(addinRoot);
        }

        private class CountdownHelper
        {
            public int InflightCount = 0;
        }

        private class ShutdownHelper
        {
            public AddInFilter Filter = null;
            public CountdownHelper Countdown = null;
        }
        
        private void UnloadFilterThreadProc(object arg)
        {
            ShutdownHelper helperArgs = arg as ShutdownHelper;

            if (null != helperArgs)
            {
                try
                {
                    if (null != helperArgs.Filter)
                    {
                        helperArgs.Filter.UnloadFilterAndAppDomain();
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (null != helperArgs.Countdown)
                    {
                        Interlocked.Decrement(ref helperArgs.Countdown.InflightCount);
                    }
                }
            }
        }

        /// <summary>
        /// This method retires a collection of filters.  Optional "threadExitCounter" allows
        /// calling thread to monitor progress.  Caller can pass in NULL if it does not need
        /// to monitor progress.
        /// </summary>
        /// <param name="filtersToGo"></param>
        /// <param name="threadExitCounter"></param>
        private void SpawnAddInShutdownThreads(IList<AddInFilter> filtersToGo, CountdownHelper threadExitCounter)
        {
            if ((null != filtersToGo) && (filtersToGo.Count > 0))
            {
                foreach (AddInFilter curfilter in filtersToGo)
                {
                    ShutdownHelper args = new ShutdownHelper() {Countdown = threadExitCounter, Filter=curfilter};

                    ThreadPool.QueueUserWorkItem(new WaitCallback(UnloadFilterThreadProc), args);
                }

                if (null != threadExitCounter)  // do we care about thread exits?
                {
                    for (int i = AddInFactoryConstants.MaxSecondsForBulkRetirements; i > 0; i--)
                    {
                        Thread.Sleep(1000);

                        if (threadExitCounter.InflightCount <= 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Call this and all loaded filters will be informed that they are to tear-down and prepare for AppDomain unload
        /// (ie: process is going away).
        /// </summary>
        public void Shutdown()
        {
            CountdownHelper countdown = new CountdownHelper();
            AddInFilter[] filters = null;

            lock (_loadedAddInsLock)
            {
                if (_loadedAddIns.Count > 0)
                {
                    countdown.InflightCount = _loadedAddIns.Count;  // we will have this many threads

                    filters = new AddInFilter[_loadedAddIns.Count];

                    _loadedAddIns.Values.CopyTo(filters, 0);

                    _loadedAddIns.Clear();
                }
            }

            if (null != filters)
            {
                SpawnAddInShutdownThreads(filters, countdown);
            }
        }

        /// <summary>
        /// Will fetch the desired Filter from the loaded filter cache... or will
        /// proceed to load the filter for the first time.
        /// </summary>
        /// <param name="filterFileName"></param>
        /// <param name="executionTimeoutMillSecs"></param>
        /// <returns></returns>
        public IAddInFilter GetAddInFilter(string filterFileName, int executionTimeoutMillSecs)
        {
            AddInFilter filter = null;

            lock(_loadedAddInsLock)
            {
                int iAddIn = _loadedAddIns.IndexOfKey(filterFileName);

                if (iAddIn >= 0)  // found?
                {
                    filter = _loadedAddIns.Values[iAddIn];  // desired Filter is all ready loaded

                    return filter;  // early exit avoids (re-)load of filter
                }
                else  // not found, create it.
                {
                    //Look for AddIns in our root directory and store the results
                    Collection<AddInToken> tokens = AddInStore.FindAddIns(typeof(AddInFilterBase), _addinRoot);

                    if ((null == tokens) || (0 == tokens.Count))
                    {
                        throw new ArgumentOutOfRangeException("Number of found AddIn Filters", "No AddIn Filters found.  Check warnings[] for pipeline health.");
                    }

                    if (1 != tokens.Count)
                    {
                        throw new ArgumentOutOfRangeException("Number of found AddIn Filters", "Multiple AddIns found, there must be only one.");
                    }

                    filter = new AddInFilter(tokens[0], this);  // create a new addin based on this token

                        // here we keep track of the Filter even if it gets into a fails OnLoad
                        // thus the AppDomain can be cleand up by normal mechanism
                    _loadedAddIns.Add(filterFileName, filter);  // this filter now exists and is remembered for cleanup/shutdown

                    // here we load the filter
                    filter.LoadFilter(filterFileName, executionTimeoutMillSecs);
                }
            }
            
            return filter;
        }

        /// <summary>
        /// Creates an empty running cache and
        /// populates new cache with INTERSECTION of two lists:
        ///     previous running cache and filters actually referenced by templates
        /// 
        /// The DIFFERENCE between the lists are (abondoned and) known to NO LONGER be referenced and can
        /// be decomissioned (AppDomain destoryed).
        /// The difference is generated by taking the original running cache and removing the intersection
        /// detected above.
        /// </summary>
        /// <param name="filtersInTemplates"></param>
        /// <param name="executionTimeoutMillSecs"></param>
        public void RetireUnusedFilters(SortedList<string, object> filtersInTemplates, int executionTimeoutMillSecs)
        {
                // after removal of actually referenced filters, this contains list of abandonded filters.
            SortedList<string, AddInFilter> abandonedAddIns = null;

            lock (_loadedAddInsLock)
            {
                    // new empty running cache.  we build up with intersection in loop
                SortedList<string, AddInFilter> newLoadedAddIns = new SortedList<string, AddInFilter>(StringComparer.InvariantCultureIgnoreCase);

                abandonedAddIns = Interlocked.Exchange<SortedList<string, AddInFilter>>(ref _loadedAddIns, newLoadedAddIns); // make authoritative

                foreach (string curTemplateFilter in filtersInTemplates.Keys)
                {
                    int iFilter = abandonedAddIns.IndexOfKey(curTemplateFilter);

                    if (iFilter >= 0) // if found, move to new list
                    {
                            // here we rebuild the running cache of filters/AppDomains
                        _loadedAddIns.Add(curTemplateFilter, abandonedAddIns.Values[iFilter]);

                            // here we remove the keeper.  this leaves us with the unused filters
                        abandonedAddIns.RemoveAt(iFilter);
                    }
                }
            }

            // here we are left with unused AddIns, we can retire them all
            if (null != abandonedAddIns)
            {
                SpawnAddInShutdownThreads(abandonedAddIns.Values, null);
            }
        }

        internal void RemoveAddInFilterEntry(string filterFileName)
        {
            lock(_loadedAddInsLock)
            {
                int iAddIn = _loadedAddIns.IndexOfKey(filterFileName);

                if (iAddIn >= 0)  // found?
                {
                    _loadedAddIns.RemoveAt(iAddIn);  // remove it
                }
            }
        }

        private void ValidateEnumsAgree<T1, T2>()
        {
            string failedOn = null;
            string[] n1 = Enum.GetNames(typeof(T1));
            Array v1 = Enum.GetValues(typeof(T1));
            string[] n2 = Enum.GetNames(typeof(T2));
            Array v2 = Enum.GetValues(typeof(T2));

            if (n1.Length != n2.Length)
            {
                failedOn = "Name array lengths.";
            }
            else
            {
                for (int i = 0; i < n1.Length; i++)
                {
                    if (!n1[i].Equals(n2[i], StringComparison.InvariantCultureIgnoreCase))
                    {
                        failedOn = "Name mismatch on " + i.ToString();
                        break;
                    }
                }

                for (int i = 0; i < v1.Length; i++)
                {
                    if ((int)v1.GetValue(i) != (int)v2.GetValue(i))
                    {
                        failedOn = "Value failed on index " + i.ToString();
                        break;
                    }
                }
            }

            if (null != failedOn)
            {
                throw new Exception(String.Format("{0} != {1}.  Failed On: {2}", typeof(T1).ToString(), typeof(T2).ToString(), failedOn));
            }
        }

        /// <summary>
        /// Confirms that the internal version of the enums matches exactly with public versions.
        /// </summary>
        private void ValidateInternalAgreesWithClient()
        {
            ValidateEnumsAgree<SubmissionFilterResponseInternal, SubmissionFilterResponse>();
            ValidateEnumsAgree<ActivationFilterResponseInternal, ActivationFilterResponse>();
        }

        public AddInFilterFactory()  // only instantiate via Create static method
        {
        }
	}

    internal class AddInFilterState
    {
        internal AddInFilterBase _actualAddIn = null;
        internal readonly string _filterFileName = null;
        internal readonly int _maxExecutionTime = AddInFactoryConstants.DefaultMaxCallTimeoutMillSecs;
        internal AddInLoadData _loadData = null;

        internal IActivationFilterInternal _actvFilter = null;
        internal ISubmissionFilterInternal _subFilter = null;

        internal AddInFilterState(string filterFileName, int maxExecutionTime)
        {
            _filterFileName = filterFileName;
            _maxExecutionTime = maxExecutionTime;
        }

        internal AddInFilterState()
        {
            _filterFileName = "Uninitialized";
            _maxExecutionTime = AddInFactoryConstants.DefaultMaxCallTimeoutMillSecs;
        }
    }

    /// <summary>
    /// Holds all data about a filter including load-time warnings and errors.
    /// Interface "getters" will return NULL if the relevent interface is not available (load error or not implemented)
    /// </summary>
    public class AddInFilter : IAddInFilter
    {
        private readonly AdminUserGateKeeper _adminUserLock = new AdminUserGateKeeper();
        private readonly AddInToken _addinToken;  // which addin to use
        private readonly AddInFilterFactory _parentFactory;

        private AddInFilterState _filterState = new AddInFilterState();

        internal string FilterFilename { get { return _filterState._filterFileName; } }

        public IActivationFilterInternal GetActivationFilter() { return _filterState._actvFilter; }
        public ISubmissionFilterInternal GetSubmissionFilter() { return _filterState._subFilter; }

        public void ReloadFilterAndAppDomain()
        {
            LoadFilter(_filterState._filterFileName, _filterState._maxExecutionTime);
        }

        internal void LoadFilter(string filterFileName, int maxExecutionTime)
        {
            using (DisposableLock adminLock = _adminUserLock.GetAdminLock())
            {
                if (null != _filterState._actualAddIn)  // is there an existing filter?  Dump it
                {
                    DumpFilter();
                }

                _filterState = new AddInFilterState(filterFileName, maxExecutionTime);

                //Activate the selected AddInToken in a new AppDomain set sandboxed security
                //as soon as this is set, DumpFilter can unload the AppDomain
                _filterState._actualAddIn = _addinToken.Activate<AddInFilterBase>(AddInSecurityLevel.FullTrust);

                byte[] loadDataBytes = null;

                    // now we ask the addin to load the filter .dll
                MaxTimeCall.MakeACall(maxExecutionTime, 
                                        String.Format("Timeout loading filter: {0}", filterFileName),
                                        x => _filterState._actualAddIn.LoadFilter(filterFileName, out loadDataBytes));

                // now we reconstitute the load data
                _filterState._loadData = AddInLoadData.Deserialize(loadDataBytes);

                if (_filterState._loadData._filterLifespanFound)
                {
                    MaxTimeCall.MakeACall(maxExecutionTime,
                                            String.Format("Calling OnFilterLoad for filter: {0}", filterFileName),
                                            x => _filterState._actualAddIn.OnFilterLoad());
                }

                // note that If OnFilterLoad() throws, the following code will
                // not be executed and the filter will be safely unusable.

                if (_filterState._loadData._activationFilterFound)
                {
                    _filterState._actvFilter = new ActivationFilter(this);
                }

                if (_filterState._loadData._submissionFilterFound)
                {
                    _filterState._subFilter = new SubmissionFilter(this);
                }
            }
        }

        /// <summary>
        /// Manages the end of life of an AddIn Filter.  If the filter author
        /// implemented the appropriate interface, the filter will be called before
        /// the AppDomain is shut down.
        /// </summary>
        /// <param name="deadFilter"></param>
        private void DumpFilter()
        {
            AddInFilterState oldState = _filterState;  // cache the current values so we can reset them

            _filterState = new AddInFilterState();  // restore to default values

            if (null != oldState._actualAddIn)  // if we created an AppDomain, be sure to destroy it
            {
                if (null != oldState._loadData)
                {
                    if (oldState._loadData._filterLifespanFound) // if filter implemented lifespan interface, call it
                    {
                        try
                        {
                            MaxTimeCall.MakeACall(oldState._maxExecutionTime,
                                            String.Format("Timeout on UnloadFilter in filter: {0}", oldState._filterFileName),
                                            x => oldState._actualAddIn.OnFilterUnload());
                        }
                        catch
                        {
                            // TODO: log UnLoad exception here
                        }
                    }
                }

                AddInController.GetAddInController(oldState._actualAddIn).Shutdown();
            }
        }

        public void UnloadFilterAndAppDomain()
        {
            using (DisposableLock adminLock = _adminUserLock.GetAdminLock())
            {
                _parentFactory.RemoveAddInFilterEntry(_filterState._filterFileName);

                DumpFilter();  // dump the app domain
            }
        }

        // here we replicate the interfaces so that callers are protected from
        // "reloading" filters.
#region Filter Interfaces

        private void ValidateFilterLoad()
        {
            if (null == _filterState._loadData)
            {
                throw new Exception(String.Format("Filter load not yet completed: {0}", _filterState._filterFileName));
            }
        }

        private void ValidateActivationFilterExists()
        {
            if (!_filterState._loadData._activationFilterFound)
            {
                throw new Exception(String.Format("No Activation filter in: {0}", _filterState._filterFileName));
            }
        }

        private void ValidateSubmissionFilterExists()
        {
            if (!_filterState._loadData._submissionFilterFound)
            {
                throw new Exception(String.Format("No Submission filter in: {0}", _filterState._filterFileName));
            }
        }

        internal ActivationFilterResponse FilterActivation(Stream jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount)
        {
            using (DisposableLock userLock = _adminUserLock.GetUserLock())
            {
                ValidateFilterLoad();
                ValidateActivationFilterExists();

                int retVal = _filterState._actualAddIn.FilterActivation(jobXml, schedulerPass, jobIndex, backfill, resourceCount);
                
                return (ActivationFilterResponse)retVal;
            }
        }

        internal void RevertActivation(Stream jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount)
        {
            using (DisposableLock userLock = _adminUserLock.GetUserLock())
            {
                ValidateFilterLoad();
                ValidateActivationFilterExists();

                _filterState._actualAddIn.RevertActivation(jobXml, schedulerPass, jobIndex, backfill, resourceCount);
            }
        }

        internal SubmissionFilterResponse FilterSubmission(Stream jobXml, SubmissionArgs args)
        {
            using (DisposableLock userLock = _adminUserLock.GetUserLock())
            {
                ValidateFilterLoad();
                ValidateSubmissionFilterExists();

                int retVal = _filterState._actualAddIn.FilterSubmission(jobXml, out args.jobXmlModified);

                return (SubmissionFilterResponse)retVal;
            }
        }

        internal void RevertSubmission(Stream jobXml)
        {
            using (DisposableLock userLock = _adminUserLock.GetUserLock())
            {
                ValidateFilterLoad();
                ValidateSubmissionFilterExists();

                _filterState._actualAddIn.RevertSubmission(jobXml);
            }
        }

#endregion        

        internal AddInFilter(AddInToken addinToken, AddInFilterFactory parentFactor)
        {
            _addinToken = addinToken;
            _parentFactory = parentFactor;
        }

        private AddInFilter()
        {
        }
    }

    internal class ActivationFilter : IActivationFilterInternal
    {
        private readonly AddInFilter _addinFilter = null;
        private readonly string _filterCallExceptionText =  null;
        private readonly string _cancelCallExceptionText = null;

        public ActivationFilterResponseInternal FilterActivation(Stream jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount, int executionTimeoutMillSecs)
        {
            int retVal = (int)ActivationFilterResponse.FailJob;
            
            MaxTimeCall.MakeACall(executionTimeoutMillSecs, _filterCallExceptionText,
                x => retVal = (int)_addinFilter.FilterActivation(jobXml, schedulerPass, jobIndex, backfill, resourceCount));

            ActivationFilterResponseInternal retValInternal = (ActivationFilterResponseInternal)retVal;

            return retValInternal;
        }

        public void RevertActivation(Stream jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount, int executionTimeoutMillSecs)
        {
            MaxTimeCall.MakeACall(executionTimeoutMillSecs, _cancelCallExceptionText, 
                            x => _addinFilter.RevertActivation(jobXml, schedulerPass, jobIndex, backfill, resourceCount));
        }

        internal ActivationFilter(AddInFilter addinFilter)
        {
            _addinFilter = addinFilter;
            _filterCallExceptionText = String.Format("Timeout invoking activation filter in file: {0}", _addinFilter.FilterFilename);
            _cancelCallExceptionText = String.Format("Timeout canceling previous activation in file: {0}", _addinFilter.FilterFilename);
        }

        private ActivationFilter()
        {
        }
    }

    /// <summary>
    /// all this because "out" can't be used in lamdas
    /// </summary>
    internal class SubmissionArgs
    {
        public Stream jobXmlModified = null;
    }

    internal class SubmissionFilter : ISubmissionFilterInternal
    {
        private readonly AddInFilter _addinFilter = null;
        private readonly string _filterCallExceptionText = null;
        private readonly string _cancelCallExceptionText = null;

        public SubmissionFilterResponseInternal FilterSubmission(Stream jobXmlIn, out Stream jobXmlModified, int executionTimeoutMillSecs)
        {
            int retVal = (int)SubmissionFilterResponse.FailJob;

            SubmissionArgs args = new SubmissionArgs();

            MaxTimeCall.MakeACall(executionTimeoutMillSecs, _filterCallExceptionText,
                x => retVal = (int)_addinFilter.FilterSubmission(jobXmlIn, args));

            jobXmlModified = args.jobXmlModified; // return any stream created/modified

            SubmissionFilterResponseInternal reValInternal = (SubmissionFilterResponseInternal)retVal;

            return reValInternal;
        }

        public void RevertSubmission(Stream jobXml, int executionTimeoutMillSecs)
        {
            MaxTimeCall.MakeACall(executionTimeoutMillSecs, _cancelCallExceptionText, x => _addinFilter.RevertSubmission(jobXml));
        }

        internal SubmissionFilter(AddInFilter addinFilter)
        {
            _addinFilter = addinFilter;
            _filterCallExceptionText = String.Format("Timeout invoking submission filter in file: {0}", _addinFilter.FilterFilename);
            _cancelCallExceptionText = String.Format("Timeout canceling submission in file: {0}", _addinFilter.FilterFilename);
        }

        private SubmissionFilter()
        {
        }
    }
}
