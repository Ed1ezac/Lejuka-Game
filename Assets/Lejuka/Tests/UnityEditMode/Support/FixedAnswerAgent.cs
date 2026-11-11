using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Cards;
using Lejuka.Contracts.Players;

namespace Lejuka.Tests.UnityEditMode.Support
{
    /// <summary>An agent that answers straight away with preset values and counts what it was asked.</summary>
    public sealed class FixedAnswerAgent : IPlayerAgent
    {
        private readonly int _cutCount;
        private readonly Rank _targetRank;

        public FixedAnswerAgent(int cutCount, Rank targetRank)
        {
            _cutCount = cutCount;
            _targetRank = targetRank;
        }

        public int ShuffleRequestCount { get; private set; }

        public CutRequest LastCutRequest { get; private set; }

        public TargetRequest LastTargetRequest { get; private set; }

        public Task RequestShuffleAsync(ShuffleRequest request, CancellationToken cancellationToken)
        {
            ShuffleRequestCount++;
            return Task.CompletedTask;
        }

        public Task<int> ChooseCutAsync(CutRequest request, CancellationToken cancellationToken)
        {
            LastCutRequest = request;
            return Task.FromResult(_cutCount);
        }

        public Task<Rank> ChooseTargetRankAsync(TargetRequest request, CancellationToken cancellationToken)
        {
            LastTargetRequest = request;
            return Task.FromResult(_targetRank);
        }
    }
}
