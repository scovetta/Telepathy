namespace Microsoft.Hpc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    public static class TaskExtensions
    {
        /// <summary>
        /// Fires the task and forget it.
        /// </summary>
        /// <param name="t">the task instance</param>
#if !net40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void FireAndForget(this Task t) { }
    }
}
