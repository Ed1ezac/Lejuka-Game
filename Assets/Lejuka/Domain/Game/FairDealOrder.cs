using System;

namespace Lejuka.Domain.Game
{
    /// <summary>
    /// Thue–Morse order: the first receiver takes the cards whose index has an even number of set bits.
    /// This cancels the head start that strict alternation gives the first receiver.
    /// </summary>
    public sealed class FairDealOrder : IDealOrder
    {
        /// <inheritdoc />
        public bool IsFirstReceiverTurn(int dealIndex)
        {
            if (dealIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dealIndex), dealIndex, "Deal index cannot be negative.");
            }

            return CountSetBits(dealIndex) % 2 == 0;
        }

        private static int CountSetBits(int value)
        {
            var count = 0;
            while (value != 0)
            {
                value &= value - 1;
                count++;
            }

            return count;
        }
    }
}
