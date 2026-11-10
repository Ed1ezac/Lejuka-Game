using Lejuka.Contracts.Players;

namespace Lejuka.Contracts.Game.Events
{
    /// <summary>All games are played and the stakes are settled.</summary>
    /// <param name="GamesWonA">Games won by seat A.</param>
    /// <param name="GamesWonB">Games won by seat B.</param>
    /// <param name="NetTransfer">Credits moving from payer to payee. Zero for a tied match.</param>
    /// <param name="Payer">The seat paying, or <c>null</c> for a tied match.</param>
    /// <param name="Payee">The seat being paid, or <c>null</c> for a tied match.</param>
    public sealed record MatchSettled(
        int GamesWonA,
        int GamesWonB,
        int NetTransfer,
        Seat? Payer,
        Seat? Payee) : IGameEvent;
}
