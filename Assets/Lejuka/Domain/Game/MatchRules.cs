using System;

namespace Lejuka.Domain.Game
{
    /// <summary>
    /// The limits on a match's size and stake. Shared so a table definition is checked
    /// against exactly the rules the match itself enforces.
    /// </summary>
    public static class MatchRules
    {
        /// <summary>Fewest games a match can have.</summary>
        public const int MinGameCount = 2;

        /// <summary>
        /// Throws unless the game count is even and at least <see cref="MinGameCount"/>.
        /// It must be even so both seats deal the same number of games.
        /// </summary>
        public static void EnsureValidGameCount(int gameCount, string paramName)
        {
            if (gameCount < MinGameCount || gameCount % 2 != 0)
            {
                throw new ArgumentOutOfRangeException(
                    paramName,
                    gameCount,
                    $"A match must have an even number of games, at least {MinGameCount}.");
            }
        }

        /// <summary>Throws unless the stake per game is positive.</summary>
        public static void EnsureValidStake(int stakePerGame, string paramName)
        {
            if (stakePerGame <= 0)
            {
                throw new ArgumentOutOfRangeException(paramName, stakePerGame, "The stake per game must be positive.");
            }
        }
    }
}
