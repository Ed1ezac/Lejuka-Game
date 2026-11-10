namespace Lejuka.Contracts.Tables
{
    /// <summary>Why a player may not join a table.</summary>
    public enum TableDenialReason
    {
        /// <summary>Access is allowed.</summary>
        None = 0,

        /// <summary>The player has not won enough matches to unlock the table.</summary>
        Locked = 1,

        /// <summary>The player cannot cover the full match escrow.</summary>
        InsufficientCredits = 2
    }
}
