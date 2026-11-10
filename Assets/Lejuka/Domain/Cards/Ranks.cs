using Lejuka.Contracts.Cards;

namespace Lejuka.Domain.Cards
{
    /// <summary>The valid range of <see cref="Rank"/> values.</summary>
    public static class Ranks
    {
        /// <summary>Lowest rank.</summary>
        public const Rank Lowest = Rank.Ace;

        /// <summary>Highest rank.</summary>
        public const Rank Highest = Rank.King;

        /// <summary>
        /// True when the value is a real rank. Needed because any integer can be cast to the enum.
        /// </summary>
        public static bool IsDefined(Rank rank)
        {
            return rank >= Lowest && rank <= Highest;
        }
    }
}
