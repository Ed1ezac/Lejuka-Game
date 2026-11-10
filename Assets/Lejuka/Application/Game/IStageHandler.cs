using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Game;

namespace Lejuka.Application.Game
{
    /// <summary>
    /// Plays one stage of a game: asks the right agent, applies the answer to the domain
    /// and publishes what happened. A handler must leave the game in a later stage.
    /// </summary>
    public interface IStageHandler
    {
        /// <summary>The stage this handler plays.</summary>
        GameStage Stage { get; }

        /// <summary>Plays the stage to its end.</summary>
        Task HandleAsync(GameContext context, CancellationToken cancellationToken);
    }
}
