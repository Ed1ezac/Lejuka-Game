using System;
using System.Globalization;
using Lejuka.Contracts.Cards;
using Lejuka.Unity.Presentation;

namespace Lejuka.Unity.Config
{
    /// <summary>
    /// Reads and writes the card sprite naming scheme:
    /// faces are <c>Card_{Suit}_{Rank}</c> (rank A, 2–10, J, Q, K) and backs are <c>Back_{Colour}_{n}</c>.
    /// Parsing ignores case and accepts a singular suit ("Heart") as well as the plural.
    /// </summary>
    public static class CardSpriteNames
    {
        private const string FacePrefix = "Card";
        private const string BackPrefix = "Back";
        private const char Separator = '_';
        private const int PartCount = 3;

        /// <summary>Returns the sprite name for a card face, e.g. <c>Card_Hearts_A</c>.</summary>
        public static string FaceName(CardData card)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            return FacePrefix + Separator + card.Suit + Separator + RankLabel.Short(card.Rank);
        }

        /// <summary>Returns the sprite name for a card back, e.g. <c>Back_Blue_1</c>.</summary>
        public static string BackName(string colour, int index)
        {
            return BackPrefix + Separator + colour + Separator + index.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Reads a face sprite name. Returns false when the name is not a card face.</summary>
        public static bool TryParseFace(string spriteName, out CardData card)
        {
            card = null;

            var parts = Split(spriteName, FacePrefix);
            if (parts == null || !TryParseSuit(parts[1], out var suit) || !TryParseRank(parts[2], out var rank))
            {
                return false;
            }

            card = new CardData(rank, suit);
            return true;
        }

        /// <summary>Reads a back sprite name. Returns false when the name is not a card back.</summary>
        public static bool TryParseBack(string spriteName, out string colour, out int index)
        {
            colour = null;
            index = 0;

            var parts = Split(spriteName, BackPrefix);
            if (parts == null || parts[1].Length == 0)
            {
                return false;
            }

            if (!int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out index))
            {
                return false;
            }

            colour = parts[1];
            return true;
        }

        private static string[] Split(string spriteName, string prefix)
        {
            if (string.IsNullOrWhiteSpace(spriteName))
            {
                return null;
            }

            var parts = spriteName.Trim().Split(Separator);
            if (parts.Length != PartCount || !string.Equals(parts[0], prefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return parts;
        }

        private static bool TryParseSuit(string text, out Suit suit)
        {
            for (var candidate = Suit.Clubs; candidate <= Suit.Spades; candidate++)
            {
                var plural = candidate.ToString();
                var singular = plural.Substring(0, plural.Length - 1);
                if (string.Equals(text, plural, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(text, singular, StringComparison.OrdinalIgnoreCase))
                {
                    suit = candidate;
                    return true;
                }
            }

            suit = default;
            return false;
        }

        private static bool TryParseRank(string text, out Rank rank)
        {
            for (var candidate = Rank.Ace; candidate <= Rank.King; candidate++)
            {
                if (string.Equals(text, RankLabel.Short(candidate), StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(text, candidate.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    rank = candidate;
                    return true;
                }
            }

            rank = default;
            return false;
        }
    }
}
