using System;
using Lejuka.Contracts.Players;

namespace Lejuka.Domain.Players
{
    /// <summary>Seat helpers.</summary>
    public static class SeatExtensions
    {
        /// <summary>Returns the other seat at the table.</summary>
        public static Seat Opponent(this Seat seat)
        {
            switch (seat)
            {
                case Seat.A:
                    return Seat.B;
                case Seat.B:
                    return Seat.A;
                default:
                    throw new ArgumentOutOfRangeException(nameof(seat), seat, "Unknown seat.");
            }
        }
    }
}
