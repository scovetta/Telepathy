namespace Microsoft.Hpc.Scheduler.Session
{
    using System.Collections.Generic;

    public static class TaskInInterestUtil
    {
        public static bool IsTaskInInterest(HashSet<int> tasksInInterestList, int taskId)
        {
            if (tasksInInterestList == null)
            {
                // null means all tasks
                return true;
            }
            else
            {
                return tasksInInterestList.Contains(taskId);
            }
        }
    }
}
