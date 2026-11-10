using System.Threading.Tasks;

namespace Lejuka.Tests.EditMode.Support
{
    /// <summary>
    /// Lets tests stay synchronous. Every fake completes its tasks immediately, so waiting never blocks,
    /// and the tests do not depend on the test runner's support for async test methods.
    /// </summary>
    public static class TaskTestExtensions
    {
        public static void Await(this Task task)
        {
            task.GetAwaiter().GetResult();
        }

        public static T Await<T>(this Task<T> task)
        {
            return task.GetAwaiter().GetResult();
        }
    }
}
