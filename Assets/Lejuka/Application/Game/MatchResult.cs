using System.Collections.Generic;
using Lejuka.Contracts.Game;
using Lejuka.Domain.Game;

namespace Lejuka.Application.Game
{
    /// <summary>The outcome of a match that was played to the end and settled.</summary>
    /// <param name="Settings">The settings the match was played under.</param>
    /// <param name="Score">The final score.</param>
    /// <param name="Settlement">The net transfer that was applied to the wallets.</param>
    /// <param name="Games">The result of every game, in play order.</param>
    public sealed record MatchResult(
        MatchSettings Settings,
        MatchScore Score,
        Settlement Settlement,
        IReadOnlyList<GameResult> Games);
}
