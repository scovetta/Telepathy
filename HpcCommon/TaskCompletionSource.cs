namespace Microsoft.Hpc
{
    using System.Threading.Tasks;

    public class TaskCompletionSource : TaskCompletionSource<object>
    {
        public void SetResult() { this.SetResult(null); }

        public bool TrySetResult() { return this.TrySetResult(null); }
    }
}
