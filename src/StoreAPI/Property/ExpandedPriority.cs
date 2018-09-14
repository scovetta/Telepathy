using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Hpc.Scheduler.Properties;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Provides methods and fields for working with the expanded scale of job priority values introduced in Windows HPC Server 2008 R2.</para>
    /// </summary>
    /// <remarks>
    ///   <para>The following table shows the ranges of expanded priority values that are equivalent to the values of the 
    /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobPriority" /> enumeration.</para>
    ///   <para>Range of expanded priority values</para>
    ///   <para>Equivalent <see cref="Microsoft.Hpc.Scheduler.Properties.JobPriority" /> member</para>
    ///   <list type="table">
    ///     <item>
    ///       <term>
    ///         <para>0 - 999</para>
    ///       </term>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPriority.Lowest" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <term>
    ///         <para>1000 - 1999</para>
    ///       </term>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPriority.BelowNormal" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <term>
    ///         <para>2000 - 2999</para>
    ///       </term>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPriority.Normal" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <term>
    ///         <para>3000 - 3999</para>
    ///       </term>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPriority.AboveNormal" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <term>
    ///         <para>4000</para>
    ///       </term>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPriority.Highest" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///   </list>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExpandedPriority" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.SessionPriority" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.JobPriority" />
    public static class ExpandedPriority
    {
        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="expandedPriority">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public static JobPriority ExpandedPriorityToJobPriority(int expandedPriority)
        {
            int pri = expandedPriority / LevelsPerPriorityBucket;
            if (pri > (int)JobPriority.Highest)
            {
                return JobPriority.Highest;
            }
            else if (pri < 0)
            {
                return JobPriority.Lowest;
            }
            return (JobPriority)pri;
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="jobPriority">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public static int JobPriorityToExpandedPriority(int jobPriority)
        {
            return jobPriority * LevelsPerPriorityBucket;
        }

        private static List<KeyValuePair<int, JobPriority>> Ranges;
        static ExpandedPriority()
        {
            Ranges = new List<KeyValuePair<int, JobPriority>>();
            Ranges.Add(new KeyValuePair<int, JobPriority>(Highest, JobPriority.Highest));
            Ranges.Add(new KeyValuePair<int, JobPriority>(AboveNormal, JobPriority.AboveNormal));
            Ranges.Add(new KeyValuePair<int, JobPriority>(Normal, JobPriority.Normal));
            Ranges.Add(new KeyValuePair<int, JobPriority>(BelowNormal, JobPriority.BelowNormal));
            Ranges.Add(new KeyValuePair<int, JobPriority>(Lowest, JobPriority.Lowest));
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="expandedPriority">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public static string ToString(int expandedPriority)
        {
            if (expandedPriority > Highest || expandedPriority < Lowest)
            {
                throw new ArgumentException(string.Format("The ExpandedPriority's value must be between {0} and {1}.", Lowest, Highest));
            }



            JobPriority? priLevel = null;
            string offsetString = string.Empty;
            GetJobPriorityAndOffsetString(expandedPriority, out priLevel, out offsetString);

            if (priLevel != null)
            {
                return string.Format("{0}{1}", priLevel.ToString(), offsetString);
            }


            Debug.Assert(false);
            throw new InvalidProgramException();
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="expandedPriority">
        ///   <para />
        /// </param>
        /// <param name="jobPriority">
        ///   <para />
        /// </param>
        /// <param name="offsetString">
        ///   <para />
        /// </param>
        public static void GetJobPriorityAndOffsetString(int expandedPriority, out JobPriority? jobPriority, out string offsetString)
        {
            jobPriority = null;
            offsetString = string.Empty;

            // Save the previous (higher) level
            JobPriority? lastItem = null;
            foreach (KeyValuePair<int, JobPriority> item in Ranges)
            {
                if (expandedPriority > item.Key)
                {
                    int offset = expandedPriority - item.Key;
                    Debug.Assert(offset < ExpandedPriority.LevelsPerPriorityBucket);

                    if (offset <= LevelsPerPriorityBucket / 2)
                    {
                        // Level + offset
                        jobPriority = item.Value;
                        offsetString = string.Format("+{0}", offset);
                        break;
                    }
                    else
                    {
                        // HigherLevel - offset
                        Debug.Assert(lastItem != null);
                        jobPriority = (JobPriority)lastItem;
                        offsetString = string.Format("-{0}", LevelsPerPriorityBucket - offset);
                        break;
                    }
                }
                if (expandedPriority == item.Key)
                {
                    jobPriority = item.Value;
                    break;
                }

                lastItem = item.Value;
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="value">
        ///   <para />
        /// </param>
        /// <param name="expandedPriority">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public static bool TryParse(string value, out int expandedPriority)
        {
            if (Regex.IsMatch(value, @"^[A-Za-z]+\+[0-9]+$"))
            {
                // Normal + 5
                string[] items = value.Split('+');
                Debug.Assert(items != null && items.Length == 2);
                expandedPriority = ExpandedPriority.JobPriorityToExpandedPriority((int)Enum.Parse(typeof(JobPriority), items[0], true)) + int.Parse(items[1]);
            }
            else if (Regex.IsMatch(value, @"^[A-Za-z]+\-[0-9]+$"))
            {
                // Normal - 5
                string[] items = value.Split('-');
                Debug.Assert(items != null && items.Length == 2);
                expandedPriority = ExpandedPriority.JobPriorityToExpandedPriority((int)Enum.Parse(typeof(JobPriority), items[0], true)) - int.Parse(items[1]);
            }
            else if (Regex.IsMatch(value, @"^[0-9]+$"))
            {
                if (int.TryParse(value, out expandedPriority))
                {
                    // 322
                }
                else
                {
                    return false;
                }
            }
            else
            {
                try
                {
                    // Normal
                    int intval = (int)Enum.Parse(typeof(JobPriority), value, true);
                    if (intval < (int)JobPriority.Lowest || intval > (int)JobPriority.Highest)
                    {
                        throw new ArgumentException();
                    }
                    expandedPriority = ExpandedPriority.JobPriorityToExpandedPriority(intval);
                }
                catch (ArgumentException)
                {
                    // We have to use try/catch here because the Enum doesn't have a TryParse
                    // and the IsDefined method doesn't allow ignoring case :(
                    expandedPriority = -1;
                    return false;
                }
            }

            if (expandedPriority > ExpandedPriority.Highest || expandedPriority < ExpandedPriority.Lowest)
            {
                expandedPriority = -1;
                return false;
            }

            return true;
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="expandedPriority">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public static int CeilingOfPriorityBucket(int expandedPriority)
        {
            if (expandedPriority < Lowest)
            {
                expandedPriority = Lowest;
            }
            if (expandedPriority > Highest)
            {
                expandedPriority = Highest;
            }
            if (expandedPriority == Highest)
            {
                return Highest;
            }
            int pri = expandedPriority / LevelsPerPriorityBucket;
            int ceiling = (pri + 1) * LevelsPerPriorityBucket - 1;
            return ceiling;
        }

        /// <summary>
        ///   <para>Represents the number of values in the expanded priority scale that 
        /// are in the range of values that are associated with each named priority value. </para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>This field has a value of 1000.</para>
        ///   <para>You can group expanded priority values into ranges of values that have the same equivalent value in the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobPriority" /> enumeration when you call the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.ExpandedPriorityToJobPriority(System.Int32)" /> method. This field indicates the number of values that those ranges include, except for the range of expanded priorities that corresponds to the  
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobPriority.Highest" /> value, which only includes one value.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.ExpandedPriorityToJobPriority(System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.CeilingOfPriorityBucket(System.Int32)" />
        public const int LevelsPerPriorityBucket = 1000;

        /// <summary>
        ///   <para>Represents the Highest priority and has a value of 4000.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.JobPriority.Highest" />
        public const int Highest = 4000;
        /// <summary>
        ///   <para>Represents the minimum expanded priority value that corresponds to an AboveNormal priority and has a value of 3000.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.JobPriority.AboveNormal" />
        public const int AboveNormal = 3000;
        /// <summary>
        ///   <para>Represents the minimum expanded priority value that corresponds to a Normal priority and has a value of 2000.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.JobPriority.Normal" />
        public const int Normal = 2000;
        /// <summary>
        ///   <para>Represents the minimum expanded priority value that corresponds to a BelowNormal priority and has a value of 1000.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.JobPriority.BelowNormal" />
        public const int BelowNormal = 1000;
        /// <summary>
        ///   <para>Represents the minimum expanded priority value that corresponds to a Lowest priority and has a value of 0.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.JobPriority.Lowest" />
        public const int Lowest = 0;
    }
}
