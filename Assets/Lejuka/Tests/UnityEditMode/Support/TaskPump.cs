using System.Threading.Tasks;

namespace Lejuka.Tests.UnityEditMode.Support
{
    /// <summary>
    /// Limits how long a test waits for a task. Tests yield one Editor frame at a time while
    /// <see cref="ShouldKeepWaiting"/> is true, so a continuation that Unity posts to the next frame
    /// still gets its chance to run, and a task that never completes fails the test instead of hanging it.
    /// </summary>
    public sealed class TaskPump
    {
        private const int MaxFrames = 120;

        private int _frames;

        /// <summary>True while the task is still running and the frame budget is not used up.</summary>
        public bool ShouldKeepWaiting(Task task)
        {
            if (task.IsCompleted)
            {
                return false;
            }

            _frames++;
            return _frames <= MaxFrames;
        }
    }
}
