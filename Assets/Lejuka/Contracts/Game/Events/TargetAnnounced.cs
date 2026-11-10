using Lejuka.Contracts.Cards;
using Lejuka.Contracts.Players;

namespace Lejuka.Contracts.Game.Events
{
    /// <summary>The Cutter has called the target rank.</summary>
    /// <param name="GameIndex">0-based index of the game within the match.</param>
    /// <param name="Cutter">The seat that called.</param>
    /// <param name="TargetRank">The rank that wins the game.</param>
    public sealed record TargetAnnounced(int GameIndex, Seat Cutter, Rank TargetRank) : IGameEvent;
}
