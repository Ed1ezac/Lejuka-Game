using System;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Common;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Game.Events;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Cards;

namespace Lejuka.Application.Game
{
    /// <summary>The Dealer shuffles, then the Cutter cuts.</summary>
    public sealed class ShuffleAndCutHandler : IStageHandler
    {
        private readonly IRandomSource _random;
        private readonly IPresentationGate _gate;

        /// <summary>Creates the handler with the randomness used for the shuffle.</summary>
        public ShuffleAndCutHandler(IRandomSource random, IPresentationGate gate)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _gate = gate ?? throw new ArgumentNullException(nameof(gate));
        }

        /// <inheritdoc />
        public GameStage Stage => GameStage.ShuffleAndCut;

        /// <inheritdoc />
        public async Task HandleAsync(GameContext context, CancellationToken cancellationToken)
        {
            var round = context.Round;

            // The agent only signals readiness. The game does the shuffle, so no agent can stack the deck.
            await context.DealerAgent.RequestShuffleAsync(
                new ShuffleRequest(round.Index, round.Dealer),
                cancellationToken);
            round.Shuffle(_random);

            var cutRequest = new CutRequest(
                round.Index,
                round.Cutter,
                CutRule.MinCut,
                CutRule.MaxCut(round.DeckCount));
            var cutCount = await context.CutterAgent.ChooseCutAsync(cutRequest, cancellationToken);
            round.Cut(cutCount);

            await _gate.PresentAsync(new DeckCut(round.Index, round.Cutter, cutCount), cancellationToken);
        }
    }
}
