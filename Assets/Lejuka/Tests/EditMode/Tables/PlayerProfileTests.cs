using System;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Game;
using Lejuka.Domain.Players;
using Lejuka.Domain.Tables;
using NUnit.Framework;

namespace Lejuka.Tests.EditMode.Tables
{
    public sealed class PlayerProfileTests
    {
        [Test]
        public void CreateNew_StartsWithTheGivenCreditsAndNoHistory()
        {
            var profile = PlayerProfile.CreateNew("p1", "Player One", new Credits(750));

            Assert.That(profile.Wallet.Balance, Is.EqualTo(new Credits(750)));
            Assert.That(profile.MatchesPlayed, Is.EqualTo(0));
            Assert.That(profile.MatchesWon, Is.EqualTo(0));
            Assert.That(profile.GamesWon, Is.EqualTo(0));
        }

        [Test]
        public void RecordMatch_Won_CountsTheMatchTheWinAndTheGames()
        {
            var profile = PlayerProfile.CreateNew("p1", "Player One", new Credits(750));

            profile.RecordMatch(new MatchScore(3, 1), Seat.A);

            Assert.That(profile.MatchesPlayed, Is.EqualTo(1));
            Assert.That(profile.MatchesWon, Is.EqualTo(1));
            Assert.That(profile.GamesWon, Is.EqualTo(3));
        }

        [Test]
        public void RecordMatch_Tied_CountsTheMatchButNoWin()
        {
            var profile = PlayerProfile.CreateNew("p1", "Player One", new Credits(750));

            profile.RecordMatch(new MatchScore(2, 2), Seat.A);

            Assert.That(profile.MatchesPlayed, Is.EqualTo(1));
            Assert.That(profile.MatchesWon, Is.EqualTo(0));
            Assert.That(profile.GamesWon, Is.EqualTo(2));
        }

        [Test]
        public void RecordMatch_Lost_CountsTheMatchAndTheGamesWon()
        {
            var profile = PlayerProfile.CreateNew("p1", "Player One", new Credits(750));

            profile.RecordMatch(new MatchScore(1, 3), Seat.A);

            Assert.That(profile.MatchesPlayed, Is.EqualTo(1));
            Assert.That(profile.MatchesWon, Is.EqualTo(0));
            Assert.That(profile.GamesWon, Is.EqualTo(1));
        }

        [Test]
        public void RecordMatch_ReadsTheScoreFromThePlayersOwnSeat()
        {
            var profile = PlayerProfile.CreateNew("p1", "Player One", new Credits(750));

            profile.RecordMatch(new MatchScore(1, 3), Seat.B);

            Assert.That(profile.MatchesWon, Is.EqualTo(1));
            Assert.That(profile.GamesWon, Is.EqualTo(3));
        }

        [Test]
        public void RecordMatch_AccumulatesAcrossMatches_AndLeavesTheWalletAlone()
        {
            var profile = PlayerProfile.CreateNew("p1", "Player One", new Credits(750));

            profile.RecordMatch(new MatchScore(4, 0), Seat.A);
            profile.RecordMatch(new MatchScore(0, 2), Seat.A);

            Assert.That(profile.MatchesPlayed, Is.EqualTo(2));
            Assert.That(profile.MatchesWon, Is.EqualTo(1));
            Assert.That(profile.GamesWon, Is.EqualTo(4));
            Assert.That(profile.Wallet.Balance, Is.EqualTo(new Credits(750)));
        }

        [Test]
        public void Constructor_RejectsMoreWinsThanMatchesPlayed()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new PlayerProfile("p1", "Player One", new Wallet(Credits.Zero), 2, 3, 0));
        }

        [Test]
        public void Constructor_RejectsBlankId()
        {
            Assert.Throws<ArgumentException>(
                () => new PlayerProfile(" ", "Player One", new Wallet(Credits.Zero), 0, 0, 0));
        }
    }
}
