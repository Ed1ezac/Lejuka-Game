using Lejuka.Contracts.Tables;

namespace Lejuka.Domain.Tables
{
    /// <summary>Whether a player may join a table, and if not, why.</summary>
    /// <param name="IsUnlocked">True when the player has won enough matches.</param>
    /// <param name="IsEligible">True when the player can cover the match escrow.</param>
    public sealed record TableAccessResult(bool IsUnlocked, bool IsEligible)
    {
        /// <summary>True when the table is both unlocked and affordable.</summary>
        public bool CanJoin => IsUnlocked && IsEligible;

        /// <summary>
        /// Why access is denied. A locked table reports <see cref="TableDenialReason.Locked"/>
        /// even if it is also unaffordable, because unlocking has to come first.
        /// </summary>
        public TableDenialReason DenialReason
        {
            get
            {
                if (!IsUnlocked)
                {
                    return TableDenialReason.Locked;
                }

                return IsEligible ? TableDenialReason.None : TableDenialReason.InsufficientCredits;
            }
        }
    }
}
