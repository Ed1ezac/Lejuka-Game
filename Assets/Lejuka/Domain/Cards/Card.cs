using System;
using Lejuka.Contracts.Cards;

namespace Lejuka.Domain.Cards
{
    /// <summary>A playing card. Immutable, and equal to any card of the same rank and suit.</summary>
    public readonly struct Card : IEquatable<Card>
    {
        /// <summary>Creates a card, rejecting values that are not a real rank or suit.</summary>
        public Card(Rank rank, Suit suit)
        {
            if (!Ranks.IsDefined(rank))
            {
                throw new ArgumentOutOfRangeException(nameof(rank), rank, "Unknown rank.");
            }

            if (suit < Suit.Clubs || suit > Suit.Spades)
            {
                throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown suit.");
            }

            Rank = rank;
            Suit = suit;
        }

        /// <summary>The card's rank.</summary>
        public Rank Rank { get; }

        /// <summary>The card's suit.</summary>
        public Suit Suit { get; }

        /// <summary>True when both cards have the same rank and suit.</summary>
        public static bool operator ==(Card left, Card right)
        {
            return left.Equals(right);
        }

        /// <summary>True when the cards differ in rank or suit.</summary>
        public static bool operator !=(Card left, Card right)
        {
            return !left.Equals(right);
        }

        /// <summary>Converts the card to its boundary representation.</summary>
        public CardData ToData()
        {
            return new CardData(Rank, Suit);
        }

        /// <inheritdoc />
        public bool Equals(Card other)
        {
            return Rank == other.Rank && Suit == other.Suit;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Card other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return ((int)Rank * 4) + (int)Suit;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
    }
}
