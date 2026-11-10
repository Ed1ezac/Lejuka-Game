using Lejuka.Contracts.Game;
using Lejuka.Contracts.Players;

namespace Lejuka.Application.Tables
{
    /// <summary>Configuration for free-play mode. Supplied by the composition root.</summary>
    /// <param name="DealMode">The deal mode used at every table.</param>
    /// <param name="StartingCredits">Credits a first-time player starts with.</param>
    /// <param name="FirstDealer">The seat that deals the first game of each match.</param>
    public sealed record FreePlaySettings(DealMode DealMode, int StartingCredits, Seat FirstDealer = Seat.A);
}
