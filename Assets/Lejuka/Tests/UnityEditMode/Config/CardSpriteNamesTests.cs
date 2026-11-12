using System.Collections.Generic;
using Lejuka.Contracts.Cards;
using Lejuka.Unity.Config;
using NUnit.Framework;

namespace Lejuka.Tests.UnityEditMode.Config
{
    public sealed class CardSpriteNamesTests
    {
        private static readonly string[] RankTokens =
        {
            "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K"
        };

        private static readonly string[] SuitTokens = { "Clubs", "Diamonds", "Hearts", "Spades" };

        [Test]
        public void TryParseFace_ReadsAll52Names_EachToADifferentCard()
        {
            var seen = new HashSet<CardData>();

            for (var suitIndex = 0; suitIndex < SuitTokens.Length; suitIndex++)
            {
                for (var rankIndex = 0; rankIndex < RankTokens.Length; rankIndex++)
                {
                    var name = "Card_" + SuitTokens[suitIndex] + "_" + RankTokens[rankIndex];

                    Assert.That(CardSpriteNames.TryParseFace(name, out var card), Is.True, name);
                    Assert.That(card.Suit, Is.EqualTo((Suit)suitIndex), name);
                    Assert.That(card.Rank, Is.EqualTo((Rank)(rankIndex + 1)), name);
                    seen.Add(card);
                }
            }

            Assert.That(seen.Count, Is.EqualTo(52));
        }

        [Test]
        public void FaceName_RoundTripsThroughTheParser()
        {
            for (var suit = Suit.Clubs; suit <= Suit.Spades; suit++)
            {
                for (var rank = Rank.Ace; rank <= Rank.King; rank++)
                {
                    var card = new CardData(rank, suit);

                    Assert.That(CardSpriteNames.TryParseFace(CardSpriteNames.FaceName(card), out var parsed), Is.True);
                    Assert.That(parsed, Is.EqualTo(card));
                }
            }
        }

        [TestCase("Card_Hearts_A", Suit.Hearts, Rank.Ace)]
        [TestCase("Card_Spades_10", Suit.Spades, Rank.Ten)]
        [TestCase("Card_Clubs_K", Suit.Clubs, Rank.King)]
        [TestCase("card_diamonds_q", Suit.Diamonds, Rank.Queen)]
        [TestCase("Card_Heart_J", Suit.Hearts, Rank.Jack)]
        [TestCase("Card_Spades_Ace", Suit.Spades, Rank.Ace)]
        public void TryParseFace_AcceptsTheNamingScheme(string name, Suit suit, Rank rank)
        {
            Assert.That(CardSpriteNames.TryParseFace(name, out var card), Is.True);
            Assert.That(card, Is.EqualTo(new CardData(rank, suit)));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("Back_Blue_1")]
        [TestCase("Card_Hearts")]
        [TestCase("Card_Hearts_1")]
        [TestCase("Card_Hearts_11")]
        [TestCase("Card_Hearts_0")]
        [TestCase("Card_Stars_A")]
        [TestCase("Card_Hearts_A_Extra")]
        [TestCase("Table_Felt")]
        public void TryParseFace_RejectsOtherNames(string name)
        {
            Assert.That(CardSpriteNames.TryParseFace(name, out var card), Is.False);
            Assert.That(card, Is.Null);
        }

        [TestCase("Back_Blue_1", "Blue", 1)]
        [TestCase("Back_Red_2", "Red", 2)]
        [TestCase("back_green_12", "green", 12)]
        public void TryParseBack_ReadsColourAndIndex(string name, string colour, int index)
        {
            Assert.That(CardSpriteNames.TryParseBack(name, out var parsedColour, out var parsedIndex), Is.True);
            Assert.That(parsedColour, Is.EqualTo(colour));
            Assert.That(parsedIndex, Is.EqualTo(index));
        }

        [TestCase(null)]
        [TestCase("Back_Blue")]
        [TestCase("Back_Blue_One")]
        [TestCase("Back__1")]
        [TestCase("Card_Hearts_A")]
        public void TryParseBack_RejectsOtherNames(string name)
        {
            Assert.That(CardSpriteNames.TryParseBack(name, out _, out _), Is.False);
        }

        [Test]
        public void BackName_RoundTripsThroughTheParser()
        {
            Assert.That(CardSpriteNames.BackName("Blue", 1), Is.EqualTo(CardArtConfig.PreferredBackName));
            Assert.That(CardSpriteNames.TryParseBack(CardSpriteNames.BackName("Red", 3), out var colour, out var index), Is.True);
            Assert.That(colour, Is.EqualTo("Red"));
            Assert.That(index, Is.EqualTo(3));
        }
    }
}
