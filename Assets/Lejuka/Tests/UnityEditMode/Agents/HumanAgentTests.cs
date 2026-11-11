using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Lejuka.Contracts.Cards;
using Lejuka.Contracts.Players;
using Lejuka.Tests.UnityEditMode.Support;
using Lejuka.Unity.Agents;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Lejuka.Tests.UnityEditMode.Agents
{
    public sealed class HumanAgentTests
    {
        private static readonly CutRequest Cut = new CutRequest(0, Seat.A, 1, 51);

        [UnityTest]
        public IEnumerator Shuffle_WaitsForInput_ThenCompletes()
        {
            var agent = new HumanAgent();

            var task = agent.RequestShuffleAsync(new ShuffleRequest(0, Seat.A), CancellationToken.None);

            Assert.That(task.IsCompleted, Is.False);
            Assert.That(agent.Pending.Kind, Is.EqualTo(HumanInputKind.Shuffle));
            Assert.That(agent.Pending.Seat, Is.EqualTo(Seat.A));

            agent.SubmitShuffle();

            var pump = new TaskPump();
            while (pump.ShouldKeepWaiting(task))
            {
                yield return null;
            }

            Assert.That(task.IsCompleted && !task.IsFaulted && !task.IsCanceled, Is.True);
            Assert.That(agent.Pending, Is.Null);
        }

        [UnityTest]
        public IEnumerator Cut_ExposesTheRange_AndReturnsTheSubmittedCount()
        {
            var agent = new HumanAgent();

            var task = agent.ChooseCutAsync(Cut, CancellationToken.None);

            Assert.That(task.IsCompleted, Is.False);
            Assert.That(agent.Pending.Kind, Is.EqualTo(HumanInputKind.Cut));
            Assert.That(agent.Pending.MinCut, Is.EqualTo(1));
            Assert.That(agent.Pending.MaxCut, Is.EqualTo(51));

            agent.SubmitCut(17);

            var pump = new TaskPump();
            while (pump.ShouldKeepWaiting(task))
            {
                yield return null;
            }

            Assert.That(task.IsCompleted && !task.IsFaulted && !task.IsCanceled, Is.True);
            Assert.That(task.Result, Is.EqualTo(17));
        }

        [UnityTest]
        public IEnumerator Rank_ReturnsTheSubmittedRank()
        {
            var agent = new HumanAgent();

            var task = agent.ChooseTargetRankAsync(new TargetRequest(0, Seat.B), CancellationToken.None);

            Assert.That(agent.Pending.Kind, Is.EqualTo(HumanInputKind.Rank));
            Assert.That(agent.Pending.Seat, Is.EqualTo(Seat.B));

            agent.SubmitRank(Rank.Queen);

            var pump = new TaskPump();
            while (pump.ShouldKeepWaiting(task))
            {
                yield return null;
            }

            Assert.That(task.IsCompleted && !task.IsFaulted && !task.IsCanceled, Is.True);
            Assert.That(task.Result, Is.EqualTo(Rank.Queen));
        }

        [UnityTest]
        public IEnumerator Answer_IsNotValidatedByTheAgent()
        {
            var agent = new HumanAgent();

            var task = agent.ChooseCutAsync(Cut, CancellationToken.None);
            agent.SubmitCut(999);

            var pump = new TaskPump();
            while (pump.ShouldKeepWaiting(task))
            {
                yield return null;
            }

            // Out of range on purpose: rejecting it is the domain's job, not the agent's.
            Assert.That(task.Result, Is.EqualTo(999));
        }

        [Test]
        public void PendingChanged_ReportsTheRequest_ThenNull()
        {
            var agent = new HumanAgent();
            var seen = new List<HumanInputRequest>();
            agent.PendingChanged += seen.Add;

            agent.ChooseCutAsync(Cut, CancellationToken.None);
            agent.SubmitCut(5);

            Assert.That(seen.Count, Is.EqualTo(2));
            Assert.That(seen[0].Kind, Is.EqualTo(HumanInputKind.Cut));
            Assert.That(seen[1], Is.Null);
        }

        [UnityTest]
        public IEnumerator Cancellation_CancelsTheRequest_AndClearsPending()
        {
            var agent = new HumanAgent();
            using (var cancellation = new CancellationTokenSource())
            {
                var task = agent.ChooseCutAsync(Cut, cancellation.Token);
                Assert.That(agent.Pending, Is.Not.Null);

                cancellation.Cancel();

                var pump = new TaskPump();
                while (pump.ShouldKeepWaiting(task))
                {
                    yield return null;
                }

                Assert.That(task.IsCanceled, Is.True);
                Assert.That(agent.Pending, Is.Null);
            }
        }

        [UnityTest]
        public IEnumerator AlreadyCancelledToken_NeverAsksForInput()
        {
            var agent = new HumanAgent();
            using (var cancellation = new CancellationTokenSource())
            {
                cancellation.Cancel();

                var task = agent.ChooseCutAsync(Cut, cancellation.Token);

                var pump = new TaskPump();
                while (pump.ShouldKeepWaiting(task))
                {
                    yield return null;
                }

                Assert.That(task.IsCanceled, Is.True);
                Assert.That(agent.Pending, Is.Null);
            }
        }

        [UnityTest]
        public IEnumerator SecondConcurrentRequest_IsRejected_AndTheFirstKeepsWaiting()
        {
            var agent = new HumanAgent();

            var first = agent.ChooseCutAsync(Cut, CancellationToken.None);
            var second = agent.ChooseTargetRankAsync(new TargetRequest(0, Seat.A), CancellationToken.None);

            var pump = new TaskPump();
            while (pump.ShouldKeepWaiting(second))
            {
                yield return null;
            }

            Assert.That(second.IsFaulted, Is.True);
            Assert.That(second.Exception.InnerException, Is.InstanceOf<InvalidOperationException>());

            Assert.That(first.IsCompleted, Is.False);
            Assert.That(agent.Pending.Kind, Is.EqualTo(HumanInputKind.Cut));

            agent.SubmitCut(3);

            pump = new TaskPump();
            while (pump.ShouldKeepWaiting(first))
            {
                yield return null;
            }

            Assert.That(first.Result, Is.EqualTo(3));
        }

        [Test]
        public void Submit_WithNothingPending_IsIgnored()
        {
            var agent = new HumanAgent();

            Assert.DoesNotThrow(() => agent.SubmitShuffle());
            Assert.That(agent.Pending, Is.Null);
        }

        [Test]
        public void Submit_OfTheWrongKind_Throws_AndKeepsTheRequest()
        {
            var agent = new HumanAgent();
            agent.ChooseCutAsync(Cut, CancellationToken.None);

            Assert.Throws<InvalidOperationException>(() => agent.SubmitRank(Rank.Ace));
            Assert.That(agent.Pending.Kind, Is.EqualTo(HumanInputKind.Cut));
        }
    }
}
