using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Hurricane.Utilities
{
    static class TaskExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async static void Forget(this Task task)
        {
            await task.ConfigureAwait(false);
        }
    }
}
