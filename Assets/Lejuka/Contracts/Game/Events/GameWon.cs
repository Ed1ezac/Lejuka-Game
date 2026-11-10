using Lejuka.Contracts.Players;

namespace Lejuka.Contracts.Game.Events
{
    /// <summary>A game has been won by the player who received the target card.</summary>
    /// <param name="GameIndex">0-based index of the game within the match.</param>
    /// <param name="Winner">The winning seat.</param>
    public sealed record GameWon(int GameIndex, Seat Winner) : IGameEvent;
}
