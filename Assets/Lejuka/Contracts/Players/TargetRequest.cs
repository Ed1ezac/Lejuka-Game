namespace Lejuka.Contracts.Players
{
    /// <summary>Asks the Cutter to call the target rank.</summary>
    /// <param name="GameIndex">0-based index of the game within the match.</param>
    /// <param name="Cutter">The seat being asked.</param>
    public sealed record TargetRequest(int GameIndex, Seat Cutter);
}
