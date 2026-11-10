namespace Lejuka.Contracts.Tables
{
    /// <summary>One table in the free-play catalog. Validated by the domain catalog, not here.</summary>
    /// <param name="Id">Unique table id.</param>
    /// <param name="DisplayName">Name shown to the player.</param>
    /// <param name="StakePerGame">Credits each game is worth at this table.</param>
    /// <param name="GamesPerMatch">Number of games in a match at this table.</param>
    /// <param name="MatchWinsRequired">Match wins a player needs before the table unlocks.</param>
    public sealed record TableDefinition(
        string Id,
        string DisplayName,
        int StakePerGame,
        int GamesPerMatch,
        int MatchWinsRequired);
}
