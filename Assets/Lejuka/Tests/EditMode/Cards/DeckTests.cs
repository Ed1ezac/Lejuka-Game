using System;
using System.Collections.Generic;
using System.Linq;
using Lejuka.Application.Game;
using Lejuka.Contracts.Common;
using Lejuka.Domain.Cards;
using NUnit.Framework;

namespace Lejuka.Tests.EditMode.Cards
{
    public sealed class DeckTests
    {
        private const int Seed = 1234;

        [Test]
        public void CreateStandard_Has52UniqueCards()
        {
            var deck = Deck.CreateStandard();

            Assert.That(deck.Count, Is.EqualTo(52));
            Assert.That(new HashSet<Card>(deck.Cards).Count, Is.EqualTo(52));
        }

        [Test]
        public void CreateStandard_HasFourCardsOfEveryRank()
        {
            var deck = Deck.CreateStandard();

            var countsByRank = deck.Cards.GroupBy(card => card.Rank).Select(group => group.Count()).ToList();

            Assert.That(countsByRank.Count, Is.EqualTo(13));
            Assert.That(countsByRank, Is.All.EqualTo(4));
        }

        [Test]
        public void Shuffle_SameSeed_GivesSameOrder()
        {
            var first = Deck.CreateStandard();
            var second = Deck.CreateStandard();

            first.Shuffle(new SystemRandomSource(Seed));
            second.Shuffle(new SystemRandomSource(Seed));

            Assert.That(first.Cards, Is.EqualTo(second.Cards));
        }

        [Test]
        public void Shuffle_DifferentSeed_GivesDifferentOrder()
        {
            var first = Deck.CreateStandard();
            var second = Deck.CreateStandard();

            first.Shuffle(new SystemRandomSource(Seed));
            second.Shuffle(new SystemRandomSource(Seed + 1));

            Assert.That(first.Cards, Is.Not.EqualTo(second.Cards));
        }

        [Test]
        public void Shuffle_ReordersButKeepsEveryCard()
        {
            var unshuffled = Deck.CreateStandard().Cards.ToList();
            var deck = Deck.CreateStandard();

            deck.Shuffle(new SystemRandomSource(Seed));

            Assert.That(deck.Cards, Is.Not.EqualTo(unshuffled));
            Assert.That(deck.Cards, Is.EquivalentTo(unshuffled));
        }

        [TestCase(1)]
        [TestCase(26)]
        [TestCase(51)]
        public void Cut_PlacesBottomStackOnTopOfTopStack(int cutCount)
        {
            var deck = Deck.CreateStandard();
            deck.Shuffle(new SystemRandomSource(Seed));
            var before = deck.Cards.ToList();

            deck.Cut(cutCount);

            var expected = before.Skip(cutCount).Concat(before.Take(cutCount)).ToList();
            Assert.That(deck.Cards, Is.EqualTo(expected));
        }

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(52)]
        [TestCase(53)]
        public void Cut_OutsideValidRange_IsRejectedAndLeavesDeckUntouched(int cutCount)
        {
            var deck = Deck.CreateStandard();
            var before = deck.Cards.ToList();

            Assert.Throws<ArgumentOutOfRangeException>(() => deck.Cut(cutCount));
            Assert.That(deck.Cards, Is.EqualTo(before));
        }

        [Test]
        public void CutRule_ForFullDeck_AllowsOneToFiftyOne()
        {
            Assert.That(CutRule.MinCut, Is.EqualTo(1));
            Assert.That(CutRule.MaxCut(Deck.StandardSize), Is.EqualTo(51));
        }

        [Test]
        public void Draw_TakesTheTopCard()
        {
            var deck = Deck.CreateStandard();
            var top = deck.Cards[0];
            var next = deck.Cards[1];

            var drawn = deck.Draw();

            Assert.That(drawn, Is.EqualTo(top));
            Assert.That(deck.Count, Is.EqualTo(51));
            Assert.That(deck.Cards[0], Is.EqualTo(next));
        }

        [Test]
        public void Draw_FromEmptyDeck_Throws()
        {
            var deck = Deck.CreateStandard();
            for (var i = 0; i < Deck.StandardSize; i++)
            {
                deck.Draw();
            }

            Assert.Throws<DomainRuleException>(() => deck.Draw());
        }
    }
}
