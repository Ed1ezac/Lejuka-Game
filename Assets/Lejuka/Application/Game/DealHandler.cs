using System;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Game.Events;

namespace Lejuka.Application.Game
{
    /// <summary>Deals one card at a time until the target card appears.</summary>
    public sealed class DealHandler : IStageHandler
    {
        private readonly IPresentationGate _gate;

        /// <summary>Creates the handler.</summary>
        public DealHandler(IPresentationGate gate)
        {
            _gate = gate ?? throw new ArgumentNullException(nameof(gate));
        }

        /// <inheritdoc />
        public GameStage Stage => GameStage.Dealing;

        /// <inheritdoc />
        public async Task HandleAsync(GameContext context, CancellationToken cancellationToken)
        {
            var round = context.Round;

            while (round.Stage == GameStage.Dealing)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var dealt = round.DealNext();
                await _gate.PresentAsync(
                    new CardDealt(round.Index, dealt.Card.ToData(), dealt.Recipient, dealt.DealIndex, dealt.IsTarget),
                    cancellationToken);
            }
        }
    }
}
