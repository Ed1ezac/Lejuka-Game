using System;
using System.Linq;
using Lejuka.Contracts.Game;
using Lejuka.Domain.Game;
using NUnit.Framework;

namespace Lejuka.Tests.EditMode.Game
{
    public sealed class DealOrderTests
    {
        [Test]
        public void Classic_First8Cards_AlternateStartingWithFirstReceiver()
        {
            Assert.That(Sequence(new ClassicDealOrder(), 8), Is.EqualTo("FSFSFSFS"));
        }

        [Test]
        public void Fair_First16Cards_FollowThueMorse()
        {
            Assert.That(Sequence(new FairDealOrder(), 16), Is.EqualTo("FSSFSFFSSFFSFSSF"));
        }

        [Test]
        public void BothOrders_GiveTheFirstCardToTheFirstReceiver()
        {
            Assert.That(new ClassicDealOrder().IsFirstReceiverTurn(0), Is.True);
            Assert.That(new FairDealOrder().IsFirstReceiverTurn(0), Is.True);
        }

        [Test]
        public void BothOrders_RejectNegativeIndex()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ClassicDealOrder().IsFirstReceiverTurn(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FairDealOrder().IsFirstReceiverTurn(-1));
        }

        [Test]
        public void Factory_CreatesTheOrderForEachMode()
        {
            Assert.That(DealOrderFactory.Create(DealMode.Classic), Is.InstanceOf<ClassicDealOrder>());
            Assert.That(DealOrderFactory.Create(DealMode.Fair), Is.InstanceOf<FairDealOrder>());
            Assert.Throws<ArgumentOutOfRangeException>(() => DealOrderFactory.Create((DealMode)99));
        }

        private static string Sequence(IDealOrder order, int count)
        {
            return new string(Enumerable.Range(0, count)
                .Select(index => order.IsFirstReceiverTurn(index) ? 'F' : 'S')
                .ToArray());
        }
    }
}
