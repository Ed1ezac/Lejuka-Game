using System;
using System.Linq;
using Lejuka.Application.Game;
using Lejuka.Contracts.Cards;
using Lejuka.Contracts.Common;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Cards;
using Lejuka.Domain.Game;
using Lejuka.Domain.Players;
using Lejuka.Tests.EditMode.Support;
using NUnit.Framework;

namespace Lejuka.Tests.EditMode.Game
{
    public sealed class GameRoundTests
    {
        private const int Seed = 20240611;
        private const int CutCount = 17;

        [Test]
        public void NewGame_StartsInShuffleAndCut_WithNoTargetOrWinner()
        {
            var round = NewRound();

            Assert.That(round.Stage, Is.EqualTo(GameStage.ShuffleAndCut));
            Assert.That(round.Dealer, Is.EqualTo(Seat.A));
            Assert.That(round.Cutter, Is.EqualTo(Seat.B));
            Assert.That(round.TargetRank, Is.Null);
            Assert.That(round.Winner, Is.Null);
            Assert.That(round.DeckCount, Is.EqualTo(52));
            Assert.That(round.DealtCards, Is.Empty);
        }

        [Test]
        public void Constructor_RejectsSameSeatInBothRoles()
        {
            Assert.Throws<ArgumentException>(
                () => new GameRound(0, new GameRoles(Seat.A, Seat.A), new ClassicDealOrder()));
        }

        [Test]
        public void Cut_BeforeShuffle_Throws()
        {
            var round = NewRound();

            Assert.Throws<DomainRuleException>(() => round.Cut(CutCount));
        }

        [Test]
        public void Shuffle_Twice_Throws()
        {
            var round = NewRound();
            round.Shuffle(new SystemRandomSource(Seed));

            Assert.Throws<DomainRuleException>(() => round.Shuffle(new SystemRandomSource(Seed)));
        }

        [Test]
        public void InShuffleAndCut_AnnounceDealAndCeremony_Throw()
        {
            var round = NewRound();
            round.Shuffle(new SystemRandomSource(Seed));

            Assert.Throws<DomainRuleException>(() => round.Announce(Rank.Ace));
            Assert.Throws<DomainRuleException>(() => round.DealNext());
            Assert.Throws<DomainRuleException>(() => round.CompleteCeremony());
        }

        [Test]
        public void InAnnounceTarget_ShuffleCutDealAndCeremony_Throw()
        {
            var round = NewRound();
            round.Shuffle(new SystemRandomSource(Seed));
            round.Cut(CutCount);

            Assert.That(round.Stage, Is.EqualTo(GameStage.AnnounceTarget));
            Assert.Throws<DomainRuleException>(() => round.Shuffle(new SystemRandomSource(Seed)));
            Assert.Throws<DomainRuleException>(() => round.Cut(CutCount));
            Assert.Throws<DomainRuleException>(() => round.DealNext());
            Assert.Throws<DomainRuleException>(() => round.CompleteCeremony());
        }

        [Test]
        public void InDealing_ShuffleCutAnnounceAndCeremony_Throw()
        {
            var round = NewRound();
            round.Shuffle(new SystemRandomSource(Seed));
            round.Cut(CutCount);
            round.Announce(Rank.Nine);

            Assert.That(round.Stage, Is.EqualTo(GameStage.Dealing));
            Assert.That(round.TargetRank, Is.EqualTo(Rank.Nine));
            Assert.Throws<DomainRuleException>(() => round.Shuffle(new SystemRandomSource(Seed)));
            Assert.Throws<DomainRuleException>(() => round.Cut(CutCount));
            Assert.Throws<DomainRuleException>(() => round.Announce(Rank.Ten));
            Assert.Throws<DomainRuleException>(() => round.CompleteCeremony());
        }

        [Test]
        public void InCeremony_DealingFurther_Throws()
        {
            var round = NewRound();
            GameRoundDriver.PlayToCeremony(round, Seed, CutCount, Rank.Nine);

            Assert.That(round.Stage, Is.EqualTo(GameStage.Ceremony));
            Assert.Throws<DomainRuleException>(() => round.DealNext());
            Assert.Throws<DomainRuleException>(() => round.Announce(Rank.Ten));
        }

        [Test]
        public void OnceCompleted_EveryActionThrows()
        {
            var round = NewRound();
            GameRoundDriver.PlayToCeremony(round, Seed, CutCount, Rank.Nine);
            round.CompleteCeremony();

            Assert.That(round.Stage, Is.EqualTo(GameStage.Completed));
            Assert.Throws<DomainRuleException>(() => round.Shuffle(new SystemRandomSource(Seed)));
            Assert.Throws<DomainRuleException>(() => round.Cut(CutCount));
            Assert.Throws<DomainRuleException>(() => round.Announce(Rank.Ten));
            Assert.Throws<DomainRuleException>(() => round.DealNext());
            Assert.Throws<DomainRuleException>(() => round.CompleteCeremony());
        }

        [TestCase(0)]
        [TestCase(52)]
        public void Cut_OutsideValidRange_IsRejectedAndTheGameStaysInItsStage(int cutCount)
        {
            var round = NewRound();
            round.Shuffle(new SystemRandomSource(Seed));

            Assert.Throws<ArgumentOutOfRangeException>(() => round.Cut(cutCount));
            Assert.That(round.Stage, Is.EqualTo(GameStage.ShuffleAndCut));
        }

        [TestCase(0)]
        [TestCase(14)]
        public void Announce_UnknownRank_IsRejectedAndTheGameStaysInItsStage(int rankValue)
        {
            var round = NewRound();
            round.Shuffle(new SystemRandomSource(Seed));
            round.Cut(CutCount);

            Assert.Throws<ArgumentOutOfRangeException>(() => round.Announce((Rank)rankValue));
            Assert.That(round.Stage, Is.EqualTo(GameStage.AnnounceTarget));
            Assert.That(round.TargetRank, Is.Null);
        }

        [TestCase(DealMode.Classic, Seat.A)]
        [TestCase(DealMode.Classic, Seat.B)]
        [TestCase(DealMode.Fair, Seat.A)]
        [TestCase(DealMode.Fair, Seat.B)]
        public void FirstCard_GoesToTheCutter(DealMode mode, Seat dealer)
        {
            var round = NewRound(mode, dealer);
            round.Shuffle(new SystemRandomSource(Seed));
            round.Cut(CutCount);
            round.Announce(Rank.Nine);

            var first = round.DealNext();

            Assert.That(first.DealIndex, Is.EqualTo(0));
            Assert.That(first.Recipient, Is.EqualTo(round.Cutter));
            Assert.That(first.Recipient, Is.EqualTo(dealer.Opponent()));
        }

        [Test]
        public void Deal_FollowsTheShuffledAndCutDeckFromTheTop()
        {
            var replica = GameRoundDriver.ReplicaDeck(Seed, CutCount);
            var round = NewRound();

            GameRoundDriver.PlayToCeremony(round, Seed, CutCount, Rank.Nine);

            var dealt = round.DealtCards.Select(result => result.Card).ToList();
            Assert.That(dealt, Is.EqualTo(replica.Cards.Take(dealt.Count).ToList()));
            Assert.That(round.DeckCount, Is.EqualTo(52 - dealt.Count));
        }

        [TestCase(DealMode.Classic)]
        [TestCase(DealMode.Fair)]
        public void Winner_IsTheReceiverOfTheFirstTargetRankCard(DealMode mode)
        {
            var order = DealOrderFactory.Create(mode);
            var replica = GameRoundDriver.ReplicaDeck(Seed, CutCount);

            for (var targetRank = Ranks.Lowest; targetRank <= Ranks.Highest; targetRank++)
            {
                var targetIndex = GameRoundDriver.FirstIndexOf(replica, targetRank);
                var round = NewRound(mode);
                var expectedWinner = order.IsFirstReceiverTurn(targetIndex) ? round.Cutter : round.Dealer;

                GameRoundDriver.PlayToCeremony(round, Seed, CutCount, targetRank);

                // Dealing stopped on the first card of the target rank, and nothing before it matched.
                Assert.That(round.DealtCards.Count, Is.EqualTo(targetIndex + 1), targetRank.ToString());
                Assert.That(round.DealtCards.Count(result => result.IsTarget), Is.EqualTo(1), targetRank.ToString());
                var last = round.DealtCards[round.DealtCards.Count - 1];
                Assert.That(last.IsTarget, Is.True, targetRank.ToString());
                Assert.That(last.Card.Rank, Is.EqualTo(targetRank));

                Assert.That(round.Winner, Is.EqualTo(expectedWinner), targetRank.ToString());
                Assert.That(round.Winner, Is.EqualTo(last.Recipient), targetRank.ToString());
                Assert.That(round.Stage, Is.EqualTo(GameStage.Ceremony), targetRank.ToString());
            }
        }

        private static GameRound NewRound(DealMode mode = DealMode.Classic, Seat dealer = Seat.A)
        {
            return new GameRound(0, new GameRoles(dealer, dealer.Opponent()), DealOrderFactory.Create(mode));
        }
    }
}
