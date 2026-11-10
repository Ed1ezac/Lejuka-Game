using System;

namespace Lejuka.Domain.Cards
{
    /// <summary>
    /// The valid cut range. A cut must leave at least one card in each stack,
    /// so it runs from 1 to the deck size minus 1.
    /// </summary>
    public static class CutRule
    {
        /// <summary>Smallest valid cut count.</summary>
        public const int MinCut = 1;

        /// <summary>Largest valid cut count for a deck of the given size.</summary>
        public static int MaxCut(int deckCount)
        {
            return deckCount - 1;
        }

        /// <summary>True when the cut count is valid for a deck of the given size.</summary>
        public static bool IsValid(int cutCount, int deckCount)
        {
            return cutCount >= MinCut && cutCount <= MaxCut(deckCount);
        }

        /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> when the cut count is not valid.</summary>
        public static void EnsureValid(int cutCount, int deckCount)
        {
            if (!IsValid(cutCount, deckCount))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cutCount),
                    cutCount,
                    $"Cut count must be between {MinCut} and {MaxCut(deckCount)}.");
            }
        }
    }
}
