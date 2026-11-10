using Lejuka.Contracts.Players;

namespace Lejuka.Contracts.Game.Events
{
    /// <summary>A new game of the match has started.</summary>
    /// <param name="GameIndex">0-based index of the game within the match.</param>
    /// <param name="Dealer">The seat dealing this game.</param>
    /// <param name="Cutter">The seat cutting and calling the target this game.</param>
    public sealed record GameStarted(int GameIndex, Seat Dealer, Seat Cutter) : IGameEvent;
}
