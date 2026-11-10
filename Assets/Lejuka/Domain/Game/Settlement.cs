using Lejuka.Contracts.Players;
using Lejuka.Domain.Players;

namespace Lejuka.Domain.Game
{
    /// <summary>The net credit transfer that settles a completed match.</summary>
    /// <param name="Score">The final score.</param>
    /// <param name="Amount">Credits moving from payer to payee. Zero for a tied match.</param>
    /// <param name="Payer">The seat paying, or <c>null</c> for a tied match.</param>
    /// <param name="Payee">The seat being paid, or <c>null</c> for a tied match.</param>
    public sealed record Settlement(MatchScore Score, Credits Amount, Seat? Payer, Seat? Payee)
    {
        /// <summary>The change to a seat's balance: positive for the payee, negative for the payer.</summary>
        public int SignedDeltaFor(Seat seat)
        {
            if (seat == Payee)
            {
                return Amount.Amount;
            }

            if (seat == Payer)
            {
                return -Amount.Amount;
            }

            return 0;
        }
    }
}
