namespace Lejuka.Contracts.Players
{
    /// <summary>Asks the Dealer to shuffle. The shuffle itself is done by the game, not the agent.</summary>
    /// <param name="GameIndex">0-based index of the game within the match.</param>
    /// <param name="Dealer">The seat being asked.</param>
    public sealed record ShuffleRequest(int GameIndex, Seat Dealer);
}
