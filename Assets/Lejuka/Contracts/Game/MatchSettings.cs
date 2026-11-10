using Lejuka.Contracts.Players;

namespace Lejuka.Contracts.Game
{
    /// <summary>Settings fixed when a match starts. Validated by the domain, not here.</summary>
    /// <param name="GameCount">Number of games in the match (even, at least 2).</param>
    /// <param name="StakePerGame">Credits each game is worth.</param>
    /// <param name="DealMode">How dealt cards alternate between the players.</param>
    /// <param name="FirstDealer">The seat that deals the first game.</param>
    public sealed record MatchSettings(
        int GameCount,
        int StakePerGame,
        DealMode DealMode,
        Seat FirstDealer = Seat.A);
}
