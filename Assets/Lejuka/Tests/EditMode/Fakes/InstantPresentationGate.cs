using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Game;

namespace Lejuka.Tests.EditMode.Fakes
{
    /// <summary>A gate with no presentation: every event completes at once.</summary>
    public sealed class InstantPresentationGate : IPresentationGate
    {
        public Task PresentAsync(IGameEvent gameEvent, CancellationToken cancellationToken)
        {
            return cancellationToken.IsCancellationRequested
                ? Task.FromCanceled(cancellationToken)
                : Task.CompletedTask;
        }
    }
}
