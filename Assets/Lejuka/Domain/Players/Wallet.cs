using Lejuka.Contracts.Common;

namespace Lejuka.Domain.Players
{
    /// <summary>
    /// A player's credits. Part of the balance can be reserved as match escrow,
    /// which keeps it out of <see cref="Available"/> until the match settles.
    /// </summary>
    public sealed class Wallet
    {
        /// <summary>Creates a wallet with nothing reserved.</summary>
        public Wallet(Credits balance)
        {
            Balance = balance;
            Reserved = Credits.Zero;
        }

        /// <summary>All credits owned, including reserved ones.</summary>
        public Credits Balance { get; private set; }

        /// <summary>Credits held in escrow for a running match.</summary>
        public Credits Reserved { get; private set; }

        /// <summary>Credits free to stake.</summary>
        public Credits Available => Balance - Reserved;

        /// <summary>Moves credits from available into escrow.</summary>
        public void Reserve(Credits amount)
        {
            if (amount > Available)
            {
                throw new DomainRuleException(
                    $"Cannot reserve {amount} credits: only {Available} available.");
            }

            Reserved += amount;
        }

        /// <summary>
        /// Applies a match result (positive for a win, negative for a loss) and releases the whole reservation.
        /// Nothing changes if the result would push the balance below zero.
        /// </summary>
        public void Settle(int signedDelta)
        {
            // Widened so that a large win is reported as an overflow rather than wrapping negative.
            var newBalance = (long)Balance.Amount + signedDelta;
            if (newBalance < 0)
            {
                throw new DomainRuleException(
                    $"Cannot settle {signedDelta} credits against a balance of {Balance}.");
            }

            Balance = new Credits(checked((int)newBalance));
            Reserved = Credits.Zero;
        }
    }
}
