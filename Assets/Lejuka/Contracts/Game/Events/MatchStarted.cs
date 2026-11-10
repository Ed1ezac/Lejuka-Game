namespace Lejuka.Contracts.Game.Events
{
    /// <summary>A match has started and both escrows are reserved.</summary>
    /// <param name="Settings">The settings the match is played under.</param>
    public sealed record MatchStarted(MatchSettings Settings) : IGameEvent;
}
