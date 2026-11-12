using System;
using System.Threading;
using Lejuka.Contracts.Cards;
using Lejuka.Contracts.Players;
using Lejuka.Tests.UnityEditMode.Support;
using Lejuka.Unity.Agents;
using NUnit.Framework;

namespace Lejuka.Tests.UnityEditMode.Agents
{
    public sealed class ThinkingDelayAgentTests
    {
        private const int CutCount = 23;
        private const Rank TargetRank = Rank.Seven;

        [Test]
        public void ZeroDelay_PassesTheCutThroughUnchanged()
        {
            var inner = new FixedAnswerAgent(CutCount, TargetRank);
            var agent = new ThinkingDelayAgent(inner, 0f, 0f);
            var request = new CutRequest(2, Seat.B, 1, 51);

            var task = agent.ChooseCutAsync(request, CancellationToken.None);

            Assert.That(task.IsCompleted, Is.True);
            Assert.That(task.Result, Is.EqualTo(CutCount));
            Assert.That(inner.LastCutRequest, Is.SameAs(request));
        }

        [Test]
        public void ZeroDelay_PassesTheTargetRankThroughUnchanged()
        {
            var inner = new FixedAnswerAgent(CutCount, TargetRank);
            var agent = new ThinkingDelayAgent(inner, 0f, 0f);
            var request = new TargetRequest(2, Seat.B);

            var task = agent.ChooseTargetRankAsync(request, CancellationToken.None);

            Assert.That(task.IsCompleted, Is.True);
            Assert.That(task.Result, Is.EqualTo(TargetRank));
            Assert.That(inner.LastTargetRequest, Is.SameAs(request));
        }

        [Test]
        public void ZeroDelay_PassesTheShuffleRequestThrough()
        {
            var inner = new FixedAnswerAgent(CutCount, TargetRank);
            var agent = new ThinkingDelayAgent(inner, 0f, 0f);

            var task = agent.RequestShuffleAsync(new ShuffleRequest(0, Seat.B), CancellationToken.None);

            Assert.That(task.IsCompleted, Is.True);
            Assert.That(inner.ShuffleRequestCount, Is.EqualTo(1));
        }

        [Test]
        public void CancelledToken_StopsBeforeTheInnerAgentIsAsked()
        {
            var inner = new FixedAnswerAgent(CutCount, TargetRank);
            var agent = new ThinkingDelayAgent(inner, 0f, 0f);
            using (var cancellation = new CancellationTokenSource())
            {
                cancellation.Cancel();

                var task = agent.ChooseCutAsync(new CutRequest(0, Seat.B, 1, 51), cancellation.Token);

                Assert.That(task.IsCanceled, Is.True);
                Assert.That(inner.LastCutRequest, Is.Null);
            }
        }

        [Test]
        public void Constructor_RejectsABadDelayRange()
        {
            var inner = new FixedAnswerAgent(CutCount, TargetRank);

            Assert.Throws<ArgumentOutOfRangeException>(() => new ThinkingDelayAgent(inner, -0.1f, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ThinkingDelayAgent(inner, 1f, 0.5f));
            Assert.Throws<ArgumentNullException>(() => new ThinkingDelayAgent(null, 0f, 0f));
        }
    }
}
