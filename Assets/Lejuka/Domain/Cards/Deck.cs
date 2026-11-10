using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Lejuka.Contracts.Cards;
using Lejuka.Contracts.Common;

namespace Lejuka.Domain.Cards
{
    /// <summary>An ordered stack of cards. Index 0 is the top of the deck.</summary>
    public sealed class Deck
    {
        /// <summary>Number of cards in a standard deck.</summary>
        public const int StandardSize = 52;

        private readonly List<Card> _cards;
        private readonly ReadOnlyCollection<Card> _cardsView;

        private Deck(List<Card> cards)
        {
            _cards = cards;
            _cardsView = cards.AsReadOnly();
        }

        /// <summary>Cards left in the deck.</summary>
        public int Count => _cards.Count;

        /// <summary>The cards left in the deck, top first.</summary>
        public IReadOnlyList<Card> Cards => _cardsView;

        /// <summary>Creates a fresh, unshuffled 52-card deck.</summary>
        public static Deck CreateStandard()
        {
            var cards = new List<Card>(StandardSize);
            for (var suit = Suit.Clubs; suit <= Suit.Spades; suit++)
            {
                for (var rank = Ranks.Lowest; rank <= Ranks.Highest; rank++)
                {
                    cards.Add(new Card(rank, suit));
                }
            }

            return new Deck(cards);
        }

        /// <summary>Shuffles the deck in place.</summary>
        public void Shuffle(IRandomSource random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            // Fisher–Yates: the only swap scheme where every ordering is equally likely.
            for (var i = _cards.Count - 1; i > 0; i--)
            {
                var j = random.Next(0, i + 1);
                var swapped = _cards[i];
                _cards[i] = _cards[j];
                _cards[j] = swapped;
            }
        }

        /// <summary>
        /// Lifts the top <paramref name="cutCount"/> cards and places the rest on top of them.
        /// </summary>
        public void Cut(int cutCount)
        {
            CutRule.EnsureValid(cutCount, _cards.Count);

            var topStack = _cards.GetRange(0, cutCount);
            _cards.RemoveRange(0, cutCount);
            _cards.AddRange(topStack);
        }

        /// <summary>Removes and returns the top card.</summary>
        public Card Draw()
        {
            if (_cards.Count == 0)
            {
                throw new DomainRuleException("Cannot draw from an empty deck.");
            }

            var top = _cards[0];
            _cards.RemoveAt(0);
            return top;
        }
    }
}
