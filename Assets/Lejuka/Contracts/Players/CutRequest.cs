namespace Lejuka.Contracts.Players
{
    /// <summary>Asks the Cutter how many cards to lift off the top of the deck.</summary>
    /// <param name="GameIndex">0-based index of the game within the match.</param>
    /// <param name="Cutter">The seat being asked.</param>
    /// <param name="MinCut">Smallest valid cut count (inclusive).</param>
    /// <param name="MaxCut">Largest valid cut count (inclusive).</param>
    public sealed record CutRequest(int GameIndex, Seat Cutter, int MinCut, int MaxCut);
}
