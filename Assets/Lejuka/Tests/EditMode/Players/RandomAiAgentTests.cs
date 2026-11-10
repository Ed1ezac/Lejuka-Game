using System.Collections.Generic;
using System.Threading;
using Lejuka.Application.Game;
using Lejuka.Application.Players;
using Lejuka.Contracts.Cards;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Cards;
using Lejuka.Tests.EditMode.Support;
using NUnit.Framework;

namespace Lejuka.Tests.EditMode.Players
{
    public sealed class RandomAiAgentTests
    {
        private const int Seed = 77;
        private const int Samples = 5000;

        [Test]
        public void ChooseCut_StaysInsideTheRequestedRange_AndReachesBothEnds()
        {
            var agent = new RandomAiAgent(new SystemRandomSource(Seed));
            var request = new CutRequest(0, Seat.B, 1, 51);
            var seen = new HashSet<int>();

            for (var i = 0; i < Samples; i++)
            {
                seen.Add(agent.ChooseCutAsync(request, CancellationToken.None).Await());
            }

            Assert.That(seen, Is.All.InRange(1, 51));
            Assert.That(seen, Has.Member(1));
            Assert.That(seen, Has.Member(51));
        }

        [Test]
        public void ChooseTargetRank_ReturnsOnlyRealRanks_AndReachesAllOfThem()
        {
            var agent = new RandomAiAgent(new SystemRandomSource(Seed));
            var request = new TargetRequest(0, Seat.B);
            var seen = new HashSet<Rank>();

            for (var i = 0; i < Samples; i++)
            {
                seen.Add(agent.ChooseTargetRankAsync(request, CancellationToken.None).Await());
            }

            Assert.That(seen.Count, Is.EqualTo(13));
            foreach (var rank in seen)
            {
                Assert.That(Ranks.IsDefined(rank), Is.True);
            }
        }

        [Test]
        public void SameSeed_GivesSameDecisions()
        {
            var first = new RandomAiAgent(new SystemRandomSource(Seed));
            var second = new RandomAiAgent(new SystemRandomSource(Seed));
            var request = new CutRequest(0, Seat.B, 1, 51);

            for (var i = 0; i < 20; i++)
            {
                Assert.That(
                    first.ChooseCutAsync(request, CancellationToken.None).Await(),
                    Is.EqualTo(second.ChooseCutAsync(request, CancellationToken.None).Await()));
            }
        }

        [Test]
        public void RequestShuffle_CompletesImmediately()
        {
            var agent = new RandomAiAgent(new SystemRandomSource(Seed));

            var task = agent.RequestShuffleAsync(new ShuffleRequest(0, Seat.A), CancellationToken.None);

            Assert.That(task.IsCompleted, Is.True);
        }
    }
}
