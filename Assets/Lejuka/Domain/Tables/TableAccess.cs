using System;
using Lejuka.Contracts.Tables;
using Lejuka.Domain.Players;

namespace Lejuka.Domain.Tables
{
    /// <summary>Decides whether a player may join a table: it must be unlocked and affordable.</summary>
    public static class TableAccess
    {
        /// <summary>Checks both rules for the player against the table.</summary>
        public static TableAccessResult Evaluate(PlayerProfile profile, TableDefinition table)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            var isUnlocked = UnlockRule.IsUnlocked(profile.MatchesWon, table);
            var isEligible = EligibilityRule.IsEligible(
                profile.Wallet.Available,
                table.GamesPerMatch,
                new Credits(table.StakePerGame));

            return new TableAccessResult(isUnlocked, isEligible);
        }
    }
}
