using System;
using Lejuka.Contracts.Tables;

namespace Lejuka.Domain.Tables
{
    /// <summary>A table unlocks once the player has won enough matches.</summary>
    public static class UnlockRule
    {
        /// <summary>True when <paramref name="matchesWon"/> meets the table's requirement.</summary>
        public static bool IsUnlocked(int matchesWon, TableDefinition table)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            return matchesWon >= table.MatchWinsRequired;
        }
    }
}
