using Lejuka.Contracts.Tables;
using Lejuka.Domain.Players;
using Lejuka.Domain.Tables;
using NUnit.Framework;

namespace Lejuka.Tests.EditMode.Tables
{
    public sealed class TableAccessTests
    {
        // Stake 50 over 4 games needs 200 credits, and the table unlocks at 3 match wins.
        private static readonly TableDefinition Table = new TableDefinition("silver", "Silver", 50, 4, 3);

        [TestCase(0, false)]
        [TestCase(2, false)]
        [TestCase(3, true)]
        [TestCase(4, true)]
        public void UnlockRule_UnlocksAtTheRequiredMatchWins(int matchesWon, bool expected)
        {
            Assert.That(UnlockRule.IsUnlocked(matchesWon, Table), Is.EqualTo(expected));
        }

        [Test]
        public void EligibilityRule_RequiresStakeTimesGames()
        {
            Assert.That(EligibilityRule.RequiredCredits(4, new Credits(50)), Is.EqualTo(new Credits(200)));
        }

        [TestCase(199, false)]
        [TestCase(200, true)]
        [TestCase(201, true)]
        public void EligibilityRule_TurnsExactlyAtTheRequiredCredits(int available, bool expected)
        {
            Assert.That(EligibilityRule.IsEligible(new Credits(available), 4, new Credits(50)), Is.EqualTo(expected));
        }

        [Test]
        public void UnlockedAndEligible_CanJoin()
        {
            var access = TableAccess.Evaluate(Profile(credits: 200, matchesWon: 3), Table);

            Assert.That(access.IsUnlocked, Is.True);
            Assert.That(access.IsEligible, Is.True);
            Assert.That(access.CanJoin, Is.True);
            Assert.That(access.DenialReason, Is.EqualTo(TableDenialReason.None));
        }

        [Test]
        public void UnlockedButIneligible_IsDeniedForInsufficientCredits()
        {
            var access = TableAccess.Evaluate(Profile(credits: 199, matchesWon: 3), Table);

            Assert.That(access.IsUnlocked, Is.True);
            Assert.That(access.IsEligible, Is.False);
            Assert.That(access.CanJoin, Is.False);
            Assert.That(access.DenialReason, Is.EqualTo(TableDenialReason.InsufficientCredits));
        }

        [Test]
        public void EligibleButLocked_IsDeniedAsLocked()
        {
            var access = TableAccess.Evaluate(Profile(credits: 5000, matchesWon: 2), Table);

            Assert.That(access.IsUnlocked, Is.False);
            Assert.That(access.IsEligible, Is.True);
            Assert.That(access.CanJoin, Is.False);
            Assert.That(access.DenialReason, Is.EqualTo(TableDenialReason.Locked));
        }

        [Test]
        public void LockedAndIneligible_ReportsLockedFirst()
        {
            var access = TableAccess.Evaluate(Profile(credits: 0, matchesWon: 0), Table);

            Assert.That(access.CanJoin, Is.False);
            Assert.That(access.DenialReason, Is.EqualTo(TableDenialReason.Locked));
        }

        [Test]
        public void ReservedCredits_DoNotCountTowardsEligibility()
        {
            var profile = Profile(credits: 200, matchesWon: 3);
            profile.Wallet.Reserve(new Credits(1));

            var access = TableAccess.Evaluate(profile, Table);

            Assert.That(access.IsEligible, Is.False);
        }

        private static PlayerProfile Profile(int credits, int matchesWon)
        {
            return new PlayerProfile("p1", "Player One", new Wallet(new Credits(credits)), matchesWon, matchesWon, 0);
        }
    }
}
