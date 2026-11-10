using System;
using Lejuka.Contracts.Common;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Game;
using Lejuka.Domain.Players;
using Lejuka.Tests.EditMode.Support;
using NUnit.Framework;

namespace Lejuka.Tests.EditMode.Game
{
    public sealed class MatchTests
    {
        private const int Seed = 4242;
        private const int Stake = 10;

        [TestCase(-2)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(7)]
        public void Constructor_RejectsGameCountThatIsNotEvenAndAtLeastTwo(int gameCount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Match(new MatchSettings(gameCount, Stake, DealMode.Classic)));
        }

        [TestCase(2)]
        [TestCase(4)]
        [TestCase(6)]
        public void Constructor_AcceptsEvenGameCount(int gameCount)
        {
            var match = new Match(new MatchSettings(gameCount, Stake, DealMode.Classic));

            Assert.That(match.GameCount, Is.EqualTo(gameCount));
            Assert.That(match.StakePerGame, Is.EqualTo(new Credits(Stake)));
            Assert.That(match.IsComplete, Is.False);
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void Constructor_RejectsStakeThatIsNotPositive(int stake)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Match(new MatchSettings(2, stake, DealMode.Classic)));
        }

        [Test]
        public void FirstDealer_DefaultsToSeatA()
        {
            var match = NewMatch(2);

            var first = match.StartNextGame();

            Assert.That(first.Dealer, Is.EqualTo(Seat.A));
            Assert.That(first.Cutter, Is.EqualTo(Seat.B));
        }

        [Test]
        public void Roles_SwapEveryGame()
        {
            var match = new Match(new MatchSettings(4, Stake, DealMode.Classic, Seat.B));
            var expectedDealers = new[] { Seat.B, Seat.A, Seat.B, Seat.A };

            for (var i = 0; i < expectedDealers.Length; i++)
            {
                var round = match.StartNextGame();

                Assert.That(round.Index, Is.EqualTo(i));
                Assert.That(round.Dealer, Is.EqualTo(expectedDealers[i]));
                Assert.That(round.Cutter, Is.EqualTo(expectedDealers[i].Opponent()));

                GameRoundDriver.PlayToWin(round, Seat.A, new ClassicDealOrder(), Seed + i);
                match.RecordResult(round);
            }

            Assert.That(match.IsComplete, Is.True);
        }

        [Test]
        public void Settlement_FourNil_LoserPaysFourStakes()
        {
            var settlement = PlayAndSettle(Seat.A, Seat.A, Seat.A, Seat.A);

            Assert.That(settlement.Score, Is.EqualTo(new MatchScore(4, 0)));
            Assert.That(settlement.Amount, Is.EqualTo(new Credits(4 * Stake)));
            Assert.That(settlement.Payer, Is.EqualTo(Seat.B));
            Assert.That(settlement.Payee, Is.EqualTo(Seat.A));
            Assert.That(settlement.SignedDeltaFor(Seat.A), Is.EqualTo(4 * Stake));
            Assert.That(settlement.SignedDeltaFor(Seat.B), Is.EqualTo(-4 * Stake));
        }

        [Test]
        public void Settlement_ThreeOne_LoserPaysTwoStakes()
        {
            var settlement = PlayAndSettle(Seat.A, Seat.B, Seat.A, Seat.A);

            Assert.That(settlement.Score, Is.EqualTo(new MatchScore(3, 1)));
            Assert.That(settlement.Amount, Is.EqualTo(new Credits(2 * Stake)));
            Assert.That(settlement.Payer, Is.EqualTo(Seat.B));
            Assert.That(settlement.Payee, Is.EqualTo(Seat.A));
        }

        [Test]
        public void Settlement_TwoAll_MovesNothing()
        {
            var settlement = PlayAndSettle(Seat.A, Seat.B, Seat.B, Seat.A);

            Assert.That(settlement.Score, Is.EqualTo(new MatchScore(2, 2)));
            Assert.That(settlement.Amount, Is.EqualTo(Credits.Zero));
            Assert.That(settlement.Payer, Is.Null);
            Assert.That(settlement.Payee, Is.Null);
            Assert.That(settlement.SignedDeltaFor(Seat.A), Is.EqualTo(0));
            Assert.That(settlement.SignedDeltaFor(Seat.B), Is.EqualTo(0));
        }

        [Test]
        public void Settlement_OneThree_SeatAPaysTwoStakes()
        {
            var settlement = PlayAndSettle(Seat.B, Seat.A, Seat.B, Seat.B);

            Assert.That(settlement.Score, Is.EqualTo(new MatchScore(1, 3)));
            Assert.That(settlement.Amount, Is.EqualTo(new Credits(2 * Stake)));
            Assert.That(settlement.Payer, Is.EqualTo(Seat.A));
            Assert.That(settlement.Payee, Is.EqualTo(Seat.B));
            Assert.That(settlement.SignedDeltaFor(Seat.A), Is.EqualTo(-2 * Stake));
            Assert.That(settlement.SignedDeltaFor(Seat.B), Is.EqualTo(2 * Stake));
        }

        [Test]
        public void Results_AreRecordedInPlayOrder()
        {
            var match = NewMatch(2);
            Play(match, Seat.B, Seat.A);

            Assert.That(match.Results.Count, Is.EqualTo(2));
            Assert.That(match.Results[0].GameIndex, Is.EqualTo(0));
            Assert.That(match.Results[0].Winner, Is.EqualTo(Seat.B));
            Assert.That(match.Results[1].GameIndex, Is.EqualTo(1));
            Assert.That(match.Results[1].Winner, Is.EqualTo(Seat.A));
            Assert.That(match.Score, Is.EqualTo(new MatchScore(1, 1)));
        }

        [Test]
        public void CalculateSettlement_BeforeEveryGameIsPlayed_Throws()
        {
            var match = NewMatch(4);
            Play(match, Seat.A, Seat.A, Seat.A);

            Assert.Throws<DomainRuleException>(() => match.CalculateSettlement());
        }

        [Test]
        public void StartNextGame_AfterTheLastGame_Throws()
        {
            var match = NewMatch(2);
            Play(match, Seat.A, Seat.B);

            Assert.Throws<DomainRuleException>(() => match.StartNextGame());
        }

        [Test]
        public void StartNextGame_WhileAGameIsUnrecorded_Throws()
        {
            var match = NewMatch(2);
            match.StartNextGame();

            Assert.Throws<DomainRuleException>(() => match.StartNextGame());
        }

        [Test]
        public void RecordResult_ForAnUnfinishedGame_Throws()
        {
            var match = NewMatch(2);
            var round = match.StartNextGame();

            Assert.Throws<DomainRuleException>(() => match.RecordResult(round));
        }

        [Test]
        public void RecordResult_ForAGameFromElsewhere_Throws()
        {
            var match = NewMatch(2);
            match.StartNextGame();
            var stranger = new GameRound(0, new GameRoles(Seat.A, Seat.B), new ClassicDealOrder());
            GameRoundDriver.PlayToWin(stranger, Seat.A, new ClassicDealOrder(), Seed);

            Assert.Throws<DomainRuleException>(() => match.RecordResult(stranger));
        }

        private static Match NewMatch(int gameCount)
        {
            return new Match(new MatchSettings(gameCount, Stake, DealMode.Classic));
        }

        private static Settlement PlayAndSettle(params Seat[] winners)
        {
            var match = NewMatch(winners.Length);
            Play(match, winners);
            return match.CalculateSettlement();
        }

        private static void Play(Match match, params Seat[] winners)
        {
            var dealOrder = new ClassicDealOrder();
            for (var i = 0; i < winners.Length; i++)
            {
                var round = match.StartNextGame();
                GameRoundDriver.PlayToWin(round, winners[i], dealOrder, Seed + i);
                match.RecordResult(round);
            }
        }
    }
}
