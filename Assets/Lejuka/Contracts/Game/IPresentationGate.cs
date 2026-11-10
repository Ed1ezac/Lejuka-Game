using System.Threading;
using System.Threading.Tasks;

namespace Lejuka.Contracts.Game
{
    /// <summary>
    /// Shows game events to the player. The game flow awaits each call, so an animation
    /// can finish before the next step begins.
    /// </summary>
    public interface IPresentationGate
    {
        /// <summary>Presents the event and completes once the game may continue.</summary>
        Task PresentAsync(IGameEvent gameEvent, CancellationToken cancellationToken);
    }
}
