// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session
{
    using System.Collections.Generic;

    public static class TaskInInterestUtil
    {
        public static bool IsTaskInInterest(HashSet<string> tasksInInterestList, string taskId)
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
