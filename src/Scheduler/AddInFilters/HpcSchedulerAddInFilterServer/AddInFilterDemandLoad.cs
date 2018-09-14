using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;


namespace Microsoft.Hpc.Scheduler.AddInFilter.HpcServer
{
    /// <summary>
    /// This class provides demand-load functionality to the System.AddIn based filters.
    /// </summary>
    public class AddInFilterDemandLoad
    {
        private static IAddInFilterFactory _filterFactory = null;
        private static object _initLock = new object();

        public static bool FilterStackIsLoaded { get { return null != _filterFactory;}}

        public static IAddInFilterFactory GetAddInFilterFactory()
        {
            if (null == _filterFactory)
            {
                string home = Environment.GetEnvironmentVariable("CCP_HOME");
                string bin = Path.Combine(home, @"Bin\AddInPipeline\AddIns\SchedulerAddInFilters");

                string defaultAddInAssembly = Path.Combine(bin, "HpcSchedulerFilterServer.dll");

                GetAddInFilterFactory(defaultAddInAssembly);
            }

            return _filterFactory;
        }

        public static IAddInFilterFactory GetAddInFilterFactory(string explicitLocation)
        {
            if (null == _filterFactory)
            {
                lock(_initLock) // only one init call should win
                {
                    if (null == _filterFactory) // did this thread lose or win?
                    {
                        Assembly addInFilterAssembly = Assembly.LoadFile(explicitLocation); 

                        IAddInFilterFactory newFactory = ReflectionHelper.LoadSingleInterface<IAddInFilterFactory>(addInFilterAssembly, explicitLocation) as IAddInFilterFactory;

                        if (null == newFactory)
                        {
                            throw new ArgumentNullException(explicitLocation, "Null factory found.");
                        }

                        // one time init of the Pipeline
                        newFactory.InitAddInPipeline();

                        Interlocked.Exchange<IAddInFilterFactory>(ref _filterFactory, newFactory);
                    }
                }
            }

            return _filterFactory;
        }
    }

    public interface IAddInFilterFactory
    {
        /// <summary>
        /// Returns any pipeline "warnings" found during
        /// (re)construction of the System.AddIn pipeline.
        /// </summary>
        /// <returns></returns>
        string[] GetPipelineWarnings();

        /// <summary>
        /// Constructs a well formed/complete filter name (think primary key)
        /// from the fragment entered by user.
        /// Bob.dll becomes @"c:\Program Files\Hpc\...\Filters\bob.dll".
        /// MyDir\Bob.dll becomes @"c:\Program Files\Hpc\...\Filters\MyDir\Bob.dll"
        /// </summary>
        /// <param name="filterNameFragment"></param>
        /// <returns></returns>
        string MakeCanonicalFilterName(string filterNameFragment);

        /// <summary>
        /// Creates a factory assuming hard-coded pipeline "addinRoot".
        /// </summary>
        /// <returns></returns>
        void InitAddInPipeline();

        /// <summary>
        /// Creates a factory from the given pipeline root.
        /// </summary>
        /// <param name="addinRoot"></param>
        void InitAddInPipelineWithRoot(string addinRoot);

        /// <summary>
        /// Queues up shutdown notifications to each AppDomain/Filter.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Retrieves cached AddIn Filter.  If non exists one is created and initialized.
        /// </summary>
        /// <param name="filterFileName"></param>
        /// <param name="executionTimeoutMillSecs"></param>
        /// <returns></returns>
        IAddInFilter GetAddInFilter(string filterFileName, int executionTimeoutMillSecs);

        /// <summary>
        /// Will retire any filter AppDomains that are not listed in "knownFilters".
        /// IFilterLifespan will be honored before any retirements.
        /// </summary>
        /// <param name="filtersInUse"></param>
        /// <param name="executionTimeoutMillSecs"></param>
        void RetireUnusedFilters(SortedList<string, object> filtersInUse, int executionTimeoutMillSecs);
    }

    public interface IAddInFilter
    {
        IActivationFilterInternal GetActivationFilter();
        ISubmissionFilterInternal GetSubmissionFilter();

        void ReloadFilterAndAppDomain();
        void UnloadFilterAndAppDomain();
    } 

    public enum ActivationFilterResponseInternal
    {
        StartJob = 0, DoNotRunHoldQueue = 1, DoNotRunKeepResourcesAllowOtherJobsToSchedule = 2,
        HoldJobReleaseResourcesAllowOtherJobsToSchedule = 3, FailJob = 4,
        AddResourcesToRunningJob = 0, RejectAdditionofResources = 1
    };

    public interface IActivationFilterInternal
    {
        ActivationFilterResponseInternal FilterActivation(Stream jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount, int executionTimeoutMillSecs);

        void RevertActivation(Stream jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount, int executionTimeoutMillSecs);
    }

    public enum SubmissionFilterResponseInternal { FailJob = -1, SuccessNoJobChange = 0, SuccessJobChanged = 1 };

    public interface ISubmissionFilterInternal
    {
        SubmissionFilterResponseInternal FilterSubmission(Stream jobXml, out Stream jobXmlModified, int executionTimeoutMillSecs);
        void RevertSubmission(Stream jobXml, int executionTimeoutMillSecs);
    }

    public class AddInFilterTimeBoundedCallException : Exception
    {
        public AddInFilterTimeBoundedCallException(Exception inner) : base("Exception during time bounded call.  See Inner Exception.", inner)
        {
        }
    }

    public class ReflectionHelper
    {
        public static object LoadSingleInterface<T>(Assembly filterAssembly, string filename)
        {
            object instancedInterface = null;

            List<Type> customerFilters = FindAllInterfaces(filterAssembly, typeof(T));

            if (null != customerFilters)  // did we find any at all?
            {
                if (customerFilters.Count > 1)    // there can be only one!
                { 
                    throw new ArgumentOutOfRangeException(String.Format("Only one class implementing type {0} must exist in file: {1}", typeof(T).ToString(), filename));
                }

                Type subFilterType = customerFilters[0];  // the one!

                instancedInterface = Activator.CreateInstance(subFilterType);  // instantiate the class that implements the interface

                if (null == instancedInterface)
                {
                    throw new ArgumentNullException(String.Format("Unable to CreateInstance of type {0} from file: {1}", typeof(T).ToString(), filename));
                }
            }

            return instancedInterface;
        }

        /// <summary>
        /// Find all the types in the given assembly that implement the desired interface.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="desiredInterface"></param>
        /// <returns></returns>
        public static List<Type> FindAllInterfaces(Assembly assembly, Type desiredInterface)
        {
            List<Type> returnInterfaces = null;
            Type[] allTypes = assembly.GetTypes();

            if (null != allTypes)
            {
                foreach (Type curType in allTypes)
                {
                    Type[] allInterfaces = curType.GetInterfaces();

                    if (null != allInterfaces)
                    {
                        for (int j = 0; j < allInterfaces.Length; j++)
                        {
                            if (allInterfaces[j] == desiredInterface)
                            {
                                if (null == returnInterfaces)
                                    returnInterfaces = new List<Type>();

                                returnInterfaces.Add(curType);
                                break;
                            }
                        }
                    }
                }
            }

            return returnInterfaces;
        }
    }
}
