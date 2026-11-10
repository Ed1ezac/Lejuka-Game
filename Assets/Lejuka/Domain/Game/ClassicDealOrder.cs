using System;

namespace Lejuka.Domain.Game
{
    /// <summary>Strict alternation: the first receiver takes every even-indexed card.</summary>
    public sealed class ClassicDealOrder : IDealOrder
    {
        /// <inheritdoc />
        public bool IsFirstReceiverTurn(int dealIndex)
        {
            if (dealIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dealIndex), dealIndex, "Deal index cannot be negative.");
            }

            return dealIndex % 2 == 0;
        }
    }
}
