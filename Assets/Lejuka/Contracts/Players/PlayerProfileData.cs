namespace Lejuka.Contracts.Players
{
    /// <summary>Persisted state of a player profile.</summary>
    /// <param name="Id">Stable profile id.</param>
    /// <param name="DisplayName">Name shown to the player.</param>
    /// <param name="Credits">Wallet balance. Reservations are never persisted.</param>
    /// <param name="MatchesPlayed">Total completed matches.</param>
    /// <param name="MatchesWon">Matches won outright (ties do not count).</param>
    /// <param name="GamesWon">Total single games won.</param>
    public sealed record PlayerProfileData(
        string Id,
        string DisplayName,
        int Credits,
        int MatchesPlayed,
        int MatchesWon,
        int GamesWon);
}
