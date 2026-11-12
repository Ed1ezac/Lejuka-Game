using System;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Cards;
using Lejuka.Contracts.Players;
using UnityEngine;

namespace Lejuka.Unity.Agents
{
    /// <summary>
    /// Wraps another agent and plays the shuffle animation once that agent is ready to shuffle.
    /// The core publishes no "deck shuffled" event, and the shuffle happens right after
    /// <see cref="RequestShuffleAsync"/> returns, so this is the one place the animation can go.
    /// Cut and target decisions pass straight through.
    /// </summary>
    public sealed class ShuffleAnimatingAgent : IPlayerAgent
    {
        private readonly IPlayerAgent _inner;
        private readonly IShuffleAnimation _animation;

        /// <summary>Creates the decorator.</summary>
        public ShuffleAnimatingAgent(IPlayerAgent inner, IShuffleAnimation animation)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _animation = animation ?? throw new ArgumentNullException(nameof(animation));
        }

        /// <inheritdoc />
        public async Task RequestShuffleAsync(ShuffleRequest request, CancellationToken cancellationToken)
        {
            await _inner.RequestShuffleAsync(request, cancellationToken);

            await Awaitable.MainThreadAsync();
            await _animation.ShuffleAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task<int> ChooseCutAsync(CutRequest request, CancellationToken cancellationToken)
        {
            return _inner.ChooseCutAsync(request, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Rank> ChooseTargetRankAsync(TargetRequest request, CancellationToken cancellationToken)
        {
            return _inner.ChooseTargetRankAsync(request, cancellationToken);
        }
    }
}
