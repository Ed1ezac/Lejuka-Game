using System;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Cards;
using Lejuka.Contracts.Players;
using UnityEngine;

namespace Lejuka.Unity.Agents
{
    /// <summary>
    /// Wraps another agent and pauses for a random "thinking" time before each decision.
    /// The decision itself is the wrapped agent's, unchanged.
    /// </summary>
    public sealed class ThinkingDelayAgent : IPlayerAgent
    {
        private readonly IPlayerAgent _inner;
        private readonly float _minSeconds;
        private readonly float _maxSeconds;

        /// <summary>Creates the decorator. A maximum of zero disables the pause.</summary>
        public ThinkingDelayAgent(IPlayerAgent inner, float minSeconds, float maxSeconds)
        {
            if (minSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(minSeconds), minSeconds, "Cannot be negative.");
            }

            if (maxSeconds < minSeconds)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxSeconds),
                    maxSeconds,
                    $"Must be at least {nameof(minSeconds)} ({minSeconds}).");
            }

            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _minSeconds = minSeconds;
            _maxSeconds = maxSeconds;
        }

        /// <inheritdoc />
        public async Task RequestShuffleAsync(ShuffleRequest request, CancellationToken cancellationToken)
        {
            await ThinkAsync(cancellationToken);
            await _inner.RequestShuffleAsync(request, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> ChooseCutAsync(CutRequest request, CancellationToken cancellationToken)
        {
            await ThinkAsync(cancellationToken);
            return await _inner.ChooseCutAsync(request, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Rank> ChooseTargetRankAsync(TargetRequest request, CancellationToken cancellationToken)
        {
            await ThinkAsync(cancellationToken);
            return await _inner.ChooseTargetRankAsync(request, cancellationToken);
        }

        private async Task ThinkAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_maxSeconds <= 0f)
            {
                return;
            }

            await Awaitable.MainThreadAsync();

            // Cosmetic only, so it does not go through the game's seeded IRandomSource.
            var seconds = UnityEngine.Random.Range(_minSeconds, _maxSeconds);
            if (seconds > 0f)
            {
                await Awaitable.WaitForSecondsAsync(seconds, cancellationToken);
            }
        }
    }
}
