using System;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Cards;
using Lejuka.Contracts.Common;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Cards;

namespace Lejuka.Application.Players
{
    /// <summary>
    /// An AI that decides uniformly at random and answers immediately.
    /// Any "thinking" pause is a presentation concern and is added by a decorator in the Unity layer.
    /// </summary>
    public sealed class RandomAiAgent : IPlayerAgent
    {
        private readonly IRandomSource _random;

        /// <summary>Creates the agent with the randomness it draws its decisions from.</summary>
        public RandomAiAgent(IRandomSource random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <inheritdoc />
        public Task RequestShuffleAsync(ShuffleRequest request, CancellationToken cancellationToken)
        {
            return cancellationToken.IsCancellationRequested
                ? Task.FromCanceled(cancellationToken)
                : Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<int> ChooseCutAsync(CutRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<int>(cancellationToken);
            }

            return Task.FromResult(_random.Next(request.MinCut, request.MaxCut + 1));
        }

        /// <inheritdoc />
        public Task<Rank> ChooseTargetRankAsync(TargetRequest request, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<Rank>(cancellationToken);
            }

            return Task.FromResult((Rank)_random.Next((int)Ranks.Lowest, (int)Ranks.Highest + 1));
        }
    }
}
