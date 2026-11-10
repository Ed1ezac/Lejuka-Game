using Lejuka.Contracts.Players;

namespace Lejuka.Domain.Game
{
    /// <summary>Which seat deals and which seat cuts in one game.</summary>
    /// <param name="Dealer">The seat that shuffles and deals.</param>
    /// <param name="Cutter">The seat that cuts, calls the target and receives the first card.</param>
    public sealed record GameRoles(Seat Dealer, Seat Cutter);
}
