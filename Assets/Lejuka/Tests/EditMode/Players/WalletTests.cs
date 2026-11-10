using Lejuka.Contracts.Common;
using Lejuka.Domain.Players;
using NUnit.Framework;

namespace Lejuka.Tests.EditMode.Players
{
    public sealed class WalletTests
    {
        [Test]
        public void NewWallet_HasNothingReserved()
        {
            var wallet = new Wallet(new Credits(100));

            Assert.That(wallet.Balance, Is.EqualTo(new Credits(100)));
            Assert.That(wallet.Reserved, Is.EqualTo(Credits.Zero));
            Assert.That(wallet.Available, Is.EqualTo(new Credits(100)));
        }

        [Test]
        public void Reserve_LowersAvailableButNotBalance()
        {
            var wallet = new Wallet(new Credits(100));

            wallet.Reserve(new Credits(40));

            Assert.That(wallet.Balance, Is.EqualTo(new Credits(100)));
            Assert.That(wallet.Reserved, Is.EqualTo(new Credits(40)));
            Assert.That(wallet.Available, Is.EqualTo(new Credits(60)));
        }

        [Test]
        public void Reserve_TheWholeBalance_IsAllowed()
        {
            var wallet = new Wallet(new Credits(100));

            wallet.Reserve(new Credits(100));

            Assert.That(wallet.Available, Is.EqualTo(Credits.Zero));
        }

        [Test]
        public void Reserve_MoreThanAvailable_ThrowsAndChangesNothing()
        {
            var wallet = new Wallet(new Credits(100));
            wallet.Reserve(new Credits(40));

            Assert.Throws<DomainRuleException>(() => wallet.Reserve(new Credits(61)));
            Assert.That(wallet.Reserved, Is.EqualTo(new Credits(40)));
            Assert.That(wallet.Balance, Is.EqualTo(new Credits(100)));
        }

        [Test]
        public void Settle_Win_AddsToBalanceAndReleasesReservation()
        {
            var wallet = new Wallet(new Credits(100));
            wallet.Reserve(new Credits(40));

            wallet.Settle(30);

            Assert.That(wallet.Balance, Is.EqualTo(new Credits(130)));
            Assert.That(wallet.Reserved, Is.EqualTo(Credits.Zero));
            Assert.That(wallet.Available, Is.EqualTo(new Credits(130)));
        }

        [Test]
        public void Settle_Loss_TakesFromBalanceAndReleasesReservation()
        {
            var wallet = new Wallet(new Credits(100));
            wallet.Reserve(new Credits(40));

            wallet.Settle(-40);

            Assert.That(wallet.Balance, Is.EqualTo(new Credits(60)));
            Assert.That(wallet.Reserved, Is.EqualTo(Credits.Zero));
        }

        [Test]
        public void Settle_Zero_OnlyReleasesReservation()
        {
            var wallet = new Wallet(new Credits(100));
            wallet.Reserve(new Credits(40));

            wallet.Settle(0);

            Assert.That(wallet.Balance, Is.EqualTo(new Credits(100)));
            Assert.That(wallet.Reserved, Is.EqualTo(Credits.Zero));
        }

        [Test]
        public void Settle_DownToExactlyZero_IsAllowed()
        {
            var wallet = new Wallet(new Credits(100));
            wallet.Reserve(new Credits(100));

            wallet.Settle(-100);

            Assert.That(wallet.Balance, Is.EqualTo(Credits.Zero));
        }

        [Test]
        public void Settle_BelowZero_ThrowsAndChangesNothing()
        {
            var wallet = new Wallet(new Credits(100));
            wallet.Reserve(new Credits(40));

            Assert.Throws<DomainRuleException>(() => wallet.Settle(-101));
            Assert.That(wallet.Balance, Is.EqualTo(new Credits(100)));
            Assert.That(wallet.Reserved, Is.EqualTo(new Credits(40)));
        }
    }
}
