using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Common;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Game.Events;

namespace Lejuka.Application.Game
{
    /// <summary>Runs a single game from its first stage to completion, one stage handler at a time.</summary>
    public sealed class GameRunner
    {
        private readonly Dictionary<GameStage, IStageHandler> _handlers = new Dictionary<GameStage, IStageHandler>();
        private readonly IPresentationGate _gate;

        /// <summary>Creates the runner. Exactly one handler is expected per playable stage.</summary>
        public GameRunner(IEnumerable<IStageHandler> handlers, IPresentationGate gate)
        {
            if (handlers == null)
            {
                throw new ArgumentNullException(nameof(handlers));
            }

            _gate = gate ?? throw new ArgumentNullException(nameof(gate));

            foreach (var handler in handlers)
            {
                if (handler == null)
                {
                    throw new ArgumentException("Stage handlers cannot be null.", nameof(handlers));
                }

                if (_handlers.ContainsKey(handler.Stage))
                {
                    throw new ArgumentException($"Stage {handler.Stage} has more than one handler.", nameof(handlers));
                }

                _handlers.Add(handler.Stage, handler);
            }
        }

        /// <summary>Creates a runner with the four standard stage handlers.</summary>
        public static GameRunner CreateDefault(IPresentationGate gate, IRandomSource random)
        {
            var handlers = new IStageHandler[]
            {
                new ShuffleAndCutHandler(random, gate),
                new AnnounceTargetHandler(gate),
                new DealHandler(gate),
                new CeremonyHandler(gate)
            };

            return new GameRunner(handlers, gate);
        }

        /// <summary>Plays the game in <paramref name="context"/> until it is completed.</summary>
        /// <remarks>
        /// Awaits deliberately keep the caller's synchronization context (no ConfigureAwait(false)),
        /// so under Unity every agent and presentation call stays on the main thread.
        /// </remarks>
        public async Task RunAsync(GameContext context, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var round = context.Round;
            await _gate.PresentAsync(new GameStarted(round.Index, round.Dealer, round.Cutter), cancellationToken);

            while (true)
            {
                var stage = round.Stage;
                await _gate.PresentAsync(new StageChanged(round.Index, stage), cancellationToken);
                if (stage == GameStage.Completed)
                {
                    return;
                }

                if (!_handlers.TryGetValue(stage, out var handler))
                {
                    throw new InvalidOperationException($"No handler is registered for stage {stage}.");
                }

                await handler.HandleAsync(context, cancellationToken);

                // Without this, a handler that forgets to advance the game would spin here forever.
                if (round.Stage == stage)
                {
                    throw new InvalidOperationException($"The handler for stage {stage} did not advance the game.");
                }
            }
        }
    }
}
