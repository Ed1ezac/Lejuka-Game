using System;
using Lejuka.Contracts.Common;
using Lejuka.Domain.Players;
using NUnit.Framework;

namespace Lejuka.Tests.EditMode.Players
{
    public sealed class CreditsTests
    {
        [Test]
        public void Constructor_RejectsNegativeAmount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Credits(-1));
        }

        [Test]
        public void Zero_IsTheDefaultValue()
        {
            Assert.That(Credits.Zero, Is.EqualTo(new Credits(0)));
            Assert.That(default(Credits).Amount, Is.EqualTo(0));
        }

        [Test]
        public void Arithmetic_AddsSubtractsAndMultiplies()
        {
            Assert.That(new Credits(30) + new Credits(12), Is.EqualTo(new Credits(42)));
            Assert.That(new Credits(30) - new Credits(12), Is.EqualTo(new Credits(18)));
            Assert.That(new Credits(30) - new Credits(30), Is.EqualTo(Credits.Zero));
            Assert.That(new Credits(30) * 4, Is.EqualTo(new Credits(120)));
            Assert.That(new Credits(30) * 0, Is.EqualTo(Credits.Zero));
        }

        [Test]
        public void Subtraction_BelowZero_Throws()
        {
            Assert.Throws<DomainRuleException>(() => { var unused = new Credits(5) - new Credits(6); });
        }

        [Test]
        public void Multiplication_ByNegativeCount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { var unused = new Credits(5) * -1; });
        }

        [Test]
        public void Arithmetic_Overflow_ThrowsInsteadOfWrapping()
        {
            Assert.Throws<OverflowException>(() => { var unused = new Credits(int.MaxValue) + new Credits(1); });
            Assert.Throws<OverflowException>(() => { var unused = new Credits(int.MaxValue) * 2; });
        }

        [Test]
        public void Comparison_OrdersByAmount()
        {
            var small = new Credits(10);
            var large = new Credits(20);

            Assert.That(small < large, Is.True);
            Assert.That(large > small, Is.True);
            Assert.That(small <= new Credits(10), Is.True);
            Assert.That(small >= new Credits(10), Is.True);
            Assert.That(small == new Credits(10), Is.True);
            Assert.That(small != large, Is.True);
            Assert.That(small.CompareTo(large), Is.LessThan(0));
        }
    }
}
