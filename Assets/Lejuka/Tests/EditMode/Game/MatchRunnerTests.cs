using System;
using System.Linq;
using System.Threading;
using Lejuka.Application.Game;
using Lejuka.Contracts.Cards;
using Lejuka.Contracts.Common;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Game.Events;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Players;
using Lejuka.Tests.EditMode.Fakes;
using Lejuka.Tests.EditMode.Support;
using NUnit.Framework;

namespace Lejuka.Tests.EditMode.Game
{
    public sealed class MatchRunnerTests
    {
        private const int Seed = 9001;
        private const int Stake = 25;
        private const int GameCount = 4;
        private const int StartingBalance = 500;

        private RecordingPresentationGate _gate;
        private MatchRunner _runner;
        private ScriptedAgent _agentA;
        private ScriptedAgent _agentB;
        private Wallet _walletA;
        private Wallet _walletB;

        [SetUp]
        public void SetUp()
        {
            _gate = new RecordingPresentationGate();
            _runner = new MatchRunner(GameRunner.CreateDefault(_gate, new SystemRandomSource(Seed)), _gate);
            _agentA = new ScriptedAgent(20, Rank.Queen);
            _agentB = new ScriptedAgent(33, Rank.Four);
            _walletA = new Wallet(new Credits(StartingBalance));
            _walletB = new Wallet(new Credits(StartingBalance));
        }

        [Test]
        public void Run_PlaysEveryGame_AndSettlesByGamesWon()
        {
            var result = _runner.RunAsync(NewSetup(), CancellationToken.None).Await();

            var wonA = _gate.EventsOf<GameWon>().Count(won => won.Winner == Seat.A);
            var wonB = _gate.EventsOf<GameWon>().Count(won => won.Winner == Seat.B);
            Assert.That(wonA + wonB, Is.EqualTo(GameCount));
            Assert.That(result.Games.Count, Is.EqualTo(GameCount));
            Assert.That(result.Score.GamesWonA, Is.EqualTo(wonA));
            Assert.That(result.Score.GamesWonB, Is.EqualTo(wonB));

            var expectedDeltaA = Stake * (wonA - wonB);
            Assert.That(_walletA.Balance.Amount, Is.EqualTo(StartingBalance + expectedDeltaA));
            Assert.That(_walletB.Balance.Amount, Is.EqualTo(StartingBalance - expectedDeltaA));
        }

        [Test]
        public void Run_ReservesEscrowDuringTheMatch_AndReleasesItAfterSettlement()
        {
            var reservedA = Credits.Zero;
            var reservedB = Credits.Zero;
            _gate.OnPresent = gameEvent =>
            {
                if (gameEvent is MatchStarted)
                {
                    reservedA = _walletA.Reserved;
                    reservedB = _walletB.Reserved;
                }
            };

            _runner.RunAsync(NewSetup(), CancellationToken.None).Await();

            Assert.That(reservedA, Is.EqualTo(new Credits(GameCount * Stake)));
            Assert.That(reservedB, Is.EqualTo(new Credits(GameCount * Stake)));
            Assert.That(_walletA.Reserved, Is.EqualTo(Credits.Zero));
            Assert.That(_walletB.Reserved, Is.EqualTo(Credits.Zero));
            Assert.That(_walletA.Available, Is.EqualTo(_walletA.Balance));
        }

        [Test]
        public void Run_PublishesMatchStartedFirstAndMatchSettledLast()
        {
            var result = _runner.RunAsync(NewSetup(), CancellationToken.None).Await();

            Assert.That(_gate.Events[0], Is.InstanceOf<MatchStarted>());
            var settled = _gate.Events[_gate.Events.Count - 1] as MatchSettled;
            Assert.That(settled, Is.Not.Null);
            Assert.That(settled.GamesWonA, Is.EqualTo(result.Score.GamesWonA));
            Assert.That(settled.GamesWonB, Is.EqualTo(result.Score.GamesWonB));
            Assert.That(settled.NetTransfer, Is.EqualTo(result.Settlement.Amount.Amount));
            Assert.That(settled.Payer, Is.EqualTo(result.Settlement.Payer));
            Assert.That(settled.Payee, Is.EqualTo(result.Settlement.Payee));
        }

        [Test]
        public void Run_SwapsRolesEveryGame()
        {
            _runner.RunAsync(NewSetup(), CancellationToken.None).Await();

            var dealers = _gate.EventsOf<GameStarted>().Select(started => started.Dealer).ToList();
            var cutters = _gate.EventsOf<GameStarted>().Select(started => started.Cutter).ToList();
            Assert.That(dealers, Is.EqualTo(new[] { Seat.A, Seat.B, Seat.A, Seat.B }));
            Assert.That(cutters, Is.EqualTo(new[] { Seat.B, Seat.A, Seat.B, Seat.A }));
        }

        [Test]
        public void Run_AsksTheDealerToShuffle_AndTheCutterToCutAndCall()
        {
            _runner.RunAsync(NewSetup(), CancellationToken.None).Await();

            // Seat A deals games 0 and 2; seat B deals games 1 and 3.
            Assert.That(_agentA.ShuffleRequests.Select(request => request.GameIndex), Is.EqualTo(new[] { 0, 2 }));
            Assert.That(_agentB.ShuffleRequests.Select(request => request.GameIndex), Is.EqualTo(new[] { 1, 3 }));
            Assert.That(_agentB.CutRequests.Select(request => request.GameIndex), Is.EqualTo(new[] { 0, 2 }));
            Assert.That(_agentA.CutRequests.Select(request => request.GameIndex), Is.EqualTo(new[] { 1, 3 }));
            Assert.That(_agentB.TargetRequests.Select(request => request.GameIndex), Is.EqualTo(new[] { 0, 2 }));
            Assert.That(_agentA.TargetRequests.Select(request => request.GameIndex), Is.EqualTo(new[] { 1, 3 }));

            foreach (var request in _agentA.CutRequests.Concat(_agentB.CutRequests))
            {
                Assert.That(request.MinCut, Is.EqualTo(1));
                Assert.That(request.MaxCut, Is.EqualTo(51));
            }
        }

        [Test]
        public void Run_AppliesEachAgentsDecisions()
        {
            _runner.RunAsync(NewSetup(), CancellationToken.None).Await();

            var cuts = _gate.EventsOf<DeckCut>();
            var targets = _gate.EventsOf<TargetAnnounced>();
            Assert.That(cuts.Select(cut => cut.CutCount), Is.EqualTo(new[] { 33, 20, 33, 20 }));
            Assert.That(cuts.Select(cut => cut.Cutter), Is.EqualTo(new[] { Seat.B, Seat.A, Seat.B, Seat.A }));
            Assert.That(
                targets.Select(target => target.TargetRank),
                Is.EqualTo(new[] { Rank.Four, Rank.Queen, Rank.Four, Rank.Queen }));
        }

        [Test]
        public void Run_DealsTheFirstCardOfEveryGameToTheCutter_AndStopsOnTheTarget()
        {
            _runner.RunAsync(NewSetup(), CancellationToken.None).Await();

            foreach (var started in _gate.EventsOf<GameStarted>())
            {
                var dealt = _gate.EventsOf<CardDealt>().Where(card => card.GameIndex == started.GameIndex).ToList();
                var target = _gate.EventsOf<TargetAnnounced>().Single(e => e.GameIndex == started.GameIndex);
                var won = _gate.EventsOf<GameWon>().Single(e => e.GameIndex == started.GameIndex);

                Assert.That(dealt[0].DealIndex, Is.EqualTo(0));
                Assert.That(dealt[0].Recipient, Is.EqualTo(started.Cutter));
                Assert.That(dealt.Select(card => card.DealIndex), Is.EqualTo(Enumerable.Range(0, dealt.Count)));

                var last = dealt[dealt.Count - 1];
                Assert.That(dealt.Count(card => card.IsTarget), Is.EqualTo(1));
                Assert.That(last.IsTarget, Is.True);
                Assert.That(last.Card.Rank, Is.EqualTo(target.TargetRank));
                Assert.That(dealt.Take(dealt.Count - 1).All(card => card.Card.Rank != target.TargetRank), Is.True);
                Assert.That(won.Winner, Is.EqualTo(last.Recipient));
            }
        }

        [Test]
        public void Run_WalksEveryGameThroughTheStagesInOrder()
        {
            _runner.RunAsync(NewSetup(), CancellationToken.None).Await();

            var expected = new[]
            {
                GameStage.ShuffleAndCut,
                GameStage.AnnounceTarget,
                GameStage.Dealing,
                GameStage.Ceremony,
                GameStage.Completed
            };
            for (var gameIndex = 0; gameIndex < GameCount; gameIndex++)
            {
                var stages = _gate.EventsOf<StageChanged>()
                    .Where(changed => changed.GameIndex == gameIndex)
                    .Select(changed => changed.Stage)
                    .ToList();

                Assert.That(stages, Is.EqualTo(expected));
            }
        }

        [Test]
        public void Run_SameSeedAndDecisions_GivesTheSameMatch()
        {
            var first = _runner.RunAsync(NewSetup(), CancellationToken.None).Await();

            SetUp();
            var second = _runner.RunAsync(NewSetup(), CancellationToken.None).Await();

            Assert.That(second.Games, Is.EqualTo(first.Games));
        }

        [Test]
        public void Run_WhenASeatCannotCoverTheEscrow_ThrowsAndReservesNothing()
        {
            _walletB = new Wallet(new Credits((GameCount * Stake) - 1));

            Assert.Throws<DomainRuleException>(() => _runner.RunAsync(NewSetup(), CancellationToken.None).Await());

            Assert.That(_walletA.Reserved, Is.EqualTo(Credits.Zero));
            Assert.That(_walletB.Reserved, Is.EqualTo(Credits.Zero));
            Assert.That(_gate.Events, Is.Empty);
        }

        [Test]
        public void Run_WithExactlyTheEscrow_IsAllowed()
        {
            _walletA = new Wallet(new Credits(GameCount * Stake));
            _walletB = new Wallet(new Credits(GameCount * Stake));

            var result = _runner.RunAsync(NewSetup(), CancellationToken.None).Await();

            Assert.That(result.Games.Count, Is.EqualTo(GameCount));
            Assert.That(_walletA.Balance.Amount + _walletB.Balance.Amount, Is.EqualTo(2 * GameCount * Stake));
        }

        [TestCase(0)]
        [TestCase(52)]
        public void Run_WhenAnAgentReturnsAnInvalidCut_ThrowsAndMovesNoCredits(int cutCount)
        {
            _agentB = new ScriptedAgent(cutCount, Rank.Four);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => _runner.RunAsync(NewSetup(), CancellationToken.None).Await());

            AssertWalletsUntouched();
        }

        [Test]
        public void Run_WhenAnAgentReturnsAnUnknownRank_ThrowsAndMovesNoCredits()
        {
            _agentB = new ScriptedAgent(33, (Rank)0);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => _runner.RunAsync(NewSetup(), CancellationToken.None).Await());

            AssertWalletsUntouched();
        }

        [Test]
        public void Run_WhenCancelledMidMatch_ThrowsAndMovesNoCredits()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                _gate.OnPresent = gameEvent =>
                {
                    if (gameEvent is GameWon)
                    {
                        cancellation.Cancel();
                    }
                };

                Assert.Catch<OperationCanceledException>(
                    () => _runner.RunAsync(NewSetup(), cancellation.Token).Await());
            }

            Assert.That(_gate.EventsOf<GameWon>().Count, Is.EqualTo(1));
            Assert.That(_gate.EventsOf<MatchSettled>(), Is.Empty);
            AssertWalletsUntouched();
        }

        [Test]
        public void Setup_RejectsOneWalletInBothSeats()
        {
            var settings = new MatchSettings(GameCount, Stake, DealMode.Classic);
            var seatA = new MatchParticipant(new Player("a", "Player A", PlayerKind.Human), _agentA, _walletA);
            var seatB = new MatchParticipant(new Player("b", "Player B", PlayerKind.Ai), _agentB, _walletA);

            Assert.Throws<ArgumentException>(() => new MatchSetup(settings, seatA, seatB));
        }

        private MatchSetup NewSetup()
        {
            return new MatchSetup(
                new MatchSettings(GameCount, Stake, DealMode.Classic),
                new MatchParticipant(new Player("a", "Player A", PlayerKind.Human), _agentA, _walletA),
                new MatchParticipant(new Player("b", "Player B", PlayerKind.Ai), _agentB, _walletB));
        }

        private void AssertWalletsUntouched()
        {
            Assert.That(_walletA.Balance, Is.EqualTo(new Credits(StartingBalance)));
            Assert.That(_walletB.Balance, Is.EqualTo(new Credits(StartingBalance)));
            Assert.That(_walletA.Reserved, Is.EqualTo(Credits.Zero));
            Assert.That(_walletB.Reserved, Is.EqualTo(Credits.Zero));
        }
    }
}
