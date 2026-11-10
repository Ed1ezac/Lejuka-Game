using Lejuka.Domain.Players;

namespace Lejuka.Domain.Tables
{
    /// <summary>
    /// A player must be able to cover the worst case, losing every game,
    /// so the escrow is the stake times the number of games.
    /// </summary>
    public static class EligibilityRule
    {
        /// <summary>The credits a player must have available, and that are reserved for the match.</summary>
        public static Credits RequiredCredits(int gameCount, Credits stakePerGame)
        {
            return stakePerGame * gameCount;
        }

        /// <summary>True when the available credits cover the full escrow.</summary>
        public static bool IsEligible(Credits available, int gameCount, Credits stakePerGame)
        {
            return available >= RequiredCredits(gameCount, stakePerGame);
        }
    }
}
