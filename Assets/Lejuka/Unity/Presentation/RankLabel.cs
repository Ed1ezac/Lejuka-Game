using System;
using Lejuka.Contracts.Cards;

namespace Lejuka.Unity.Presentation
{
    /// <summary>The short text for a rank: A, 2–10, J, Q, K. Used by buttons, the HUD and sprite names.</summary>
    public static class RankLabel
    {
        /// <summary>Returns the short label for a rank.</summary>
        public static string Short(Rank rank)
        {
            switch (rank)
            {
                case Rank.Ace:
                    return "A";
                case Rank.Jack:
                    return "J";
                case Rank.Queen:
                    return "Q";
                case Rank.King:
                    return "K";
                default:
                    if (rank >= Rank.Two && rank <= Rank.Ten)
                    {
                        return ((int)rank).ToString();
                    }

                    throw new ArgumentOutOfRangeException(nameof(rank), rank, "Unknown rank.");
            }
        }
    }
}
