namespace Microsoft.Hpc.Scheduler.Session.SchedulerAdapter.HpcPack
{
    using System.Collections.Generic;

    internal static class NameValueCollectionToDictionary
    {
        internal static Dictionary<string, string> Convert(INameValueCollection collection)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            foreach (INameValue nvp in collection)
            {
                res[nvp.Name] = nvp.Value;
            }

            return res;
        }
    }
}
