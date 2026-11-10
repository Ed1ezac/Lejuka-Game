using System;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Game.Events;
using Lejuka.Contracts.Players;

namespace Lejuka.Application.Game
{
    /// <summary>The Cutter calls the target rank.</summary>
    public sealed class AnnounceTargetHandler : IStageHandler
    {
        private readonly IPresentationGate _gate;

        /// <summary>Creates the handler.</summary>
        public AnnounceTargetHandler(IPresentationGate gate)
        {
            _gate = gate ?? throw new ArgumentNullException(nameof(gate));
        }

        /// <inheritdoc />
        public GameStage Stage => GameStage.AnnounceTarget;

        /// <inheritdoc />
        public async Task HandleAsync(GameContext context, CancellationToken cancellationToken)
        {
            var round = context.Round;

            var targetRank = await context.CutterAgent.ChooseTargetRankAsync(
                new TargetRequest(round.Index, round.Cutter),
                cancellationToken);
            round.Announce(targetRank);

            await _gate.PresentAsync(
                new TargetAnnounced(round.Index, round.Cutter, targetRank),
                cancellationToken);
        }
    }
}
