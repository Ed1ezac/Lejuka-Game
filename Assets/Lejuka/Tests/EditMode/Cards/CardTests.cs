using System;
using Lejuka.Contracts.Cards;
using Lejuka.Domain.Cards;
using NUnit.Framework;

namespace Lejuka.Tests.EditMode.Cards
{
    public sealed class CardTests
    {
        [Test]
        public void Cards_WithSameRankAndSuit_AreEqual()
        {
            var first = new Card(Rank.Queen, Suit.Hearts);
            var second = new Card(Rank.Queen, Suit.Hearts);

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first == second, Is.True);
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        }

        [Test]
        public void Cards_WithDifferentRankOrSuit_AreNotEqual()
        {
            var card = new Card(Rank.Queen, Suit.Hearts);

            Assert.That(card != new Card(Rank.King, Suit.Hearts), Is.True);
            Assert.That(card != new Card(Rank.Queen, Suit.Spades), Is.True);
        }

        [Test]
        public void Constructor_RejectsUnknownRankAndSuit()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Card((Rank)0, Suit.Hearts));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Card((Rank)14, Suit.Hearts));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Card(Rank.Ace, (Suit)4));
        }

        [Test]
        public void ToData_CarriesRankAndSuit()
        {
            var data = new Card(Rank.Seven, Suit.Clubs).ToData();

            Assert.That(data, Is.EqualTo(new CardData(Rank.Seven, Suit.Clubs)));
        }
    }
}
