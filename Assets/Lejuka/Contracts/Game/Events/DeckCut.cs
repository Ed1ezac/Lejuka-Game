using Lejuka.Contracts.Players;

namespace Lejuka.Contracts.Game.Events
{
    /// <summary>The Cutter has cut the shuffled deck.</summary>
    /// <param name="GameIndex">0-based index of the game within the match.</param>
    /// <param name="Cutter">The seat that cut.</param>
    /// <param name="CutCount">How many cards were lifted off the top and moved underneath.</param>
    public sealed record DeckCut(int GameIndex, Seat Cutter, int CutCount) : IGameEvent;
}
