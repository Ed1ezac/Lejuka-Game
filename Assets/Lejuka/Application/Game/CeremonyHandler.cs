using System;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Game.Events;

namespace Lejuka.Application.Game
{
    /// <summary>Announces the winner, then completes the game.</summary>
    public sealed class CeremonyHandler : IStageHandler
    {
        private readonly IPresentationGate _gate;

        /// <summary>Creates the handler.</summary>
        public CeremonyHandler(IPresentationGate gate)
        {
            _gate = gate ?? throw new ArgumentNullException(nameof(gate));
        }

        /// <inheritdoc />
        public GameStage Stage => GameStage.Ceremony;

        /// <inheritdoc />
        public async Task HandleAsync(GameContext context, CancellationToken cancellationToken)
        {
            var round = context.Round;
            if (!round.Winner.HasValue)
            {
                throw new InvalidOperationException("The ceremony started without a winner.");
            }

            await _gate.PresentAsync(new GameWon(round.Index, round.Winner.Value), cancellationToken);
            round.CompleteCeremony();
        }
    }
}
