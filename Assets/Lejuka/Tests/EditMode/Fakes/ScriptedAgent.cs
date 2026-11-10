using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Cards;
using Lejuka.Contracts.Players;

namespace Lejuka.Tests.EditMode.Fakes
{
    /// <summary>An agent that always gives the same preset answers and records what it was asked.</summary>
    public sealed class ScriptedAgent : IPlayerAgent
    {
        private readonly int _cutCount;
        private readonly Rank _targetRank;
        private readonly List<ShuffleRequest> _shuffleRequests = new List<ShuffleRequest>();
        private readonly List<CutRequest> _cutRequests = new List<CutRequest>();
        private readonly List<TargetRequest> _targetRequests = new List<TargetRequest>();

        public ScriptedAgent(int cutCount, Rank targetRank)
        {
            _cutCount = cutCount;
            _targetRank = targetRank;
        }

        public IReadOnlyList<ShuffleRequest> ShuffleRequests => _shuffleRequests;

        public IReadOnlyList<CutRequest> CutRequests => _cutRequests;

        public IReadOnlyList<TargetRequest> TargetRequests => _targetRequests;

        public Task RequestShuffleAsync(ShuffleRequest request, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            _shuffleRequests.Add(request);
            return Task.CompletedTask;
        }

        public Task<int> ChooseCutAsync(CutRequest request, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<int>(cancellationToken);
            }

            _cutRequests.Add(request);
            return Task.FromResult(_cutCount);
        }

        public Task<Rank> ChooseTargetRankAsync(TargetRequest request, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<Rank>(cancellationToken);
            }

            _targetRequests.Add(request);
            return Task.FromResult(_targetRank);
        }
    }
}
