using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Hurricane.Utilities
{
    public static class TaskExtensions
    {
        public static readonly Task CompletedTask = Task.FromResult(false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget(this Task task)
        {
            //Nothing here
        }
    }
}