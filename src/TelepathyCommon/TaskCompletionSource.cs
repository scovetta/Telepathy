using System.Threading.Tasks;

namespace TelepathyCommon
{
    public class TaskCompletionSource : TaskCompletionSource<object>
    {
        public void SetResult() { this.SetResult(null); }

        public bool TrySetResult() { return this.TrySetResult(null); }
    }
}
