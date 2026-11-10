using System;
using Lejuka.Contracts.Game;

namespace Lejuka.Domain.Game
{
    /// <summary>Creates the deal order for a deal mode.</summary>
    public static class DealOrderFactory
    {
        /// <summary>Returns the deal order that implements <paramref name="mode"/>.</summary>
        public static IDealOrder Create(DealMode mode)
        {
            switch (mode)
            {
                case DealMode.Classic:
                    return new ClassicDealOrder();
                case DealMode.Fair:
                    return new FairDealOrder();
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown deal mode.");
            }
        }
    }
}
