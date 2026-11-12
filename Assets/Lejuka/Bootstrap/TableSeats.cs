using System;

namespace Lejuka.Bootstrap
{
    /// <summary>The two seats of the mockup table.</summary>
    public sealed class TableSeats
    {
        /// <summary>Creates the pair.</summary>
        public TableSeats(SeatBinding seatA, SeatBinding seatB)
        {
            SeatA = seatA ?? throw new ArgumentNullException(nameof(seatA));
            SeatB = seatB ?? throw new ArgumentNullException(nameof(seatB));
        }

        /// <summary>Seat A: the bottom of the table, played from the saved profile.</summary>
        public SeatBinding SeatA { get; }

        /// <summary>Seat B: the top of the table, the opponent.</summary>
        public SeatBinding SeatB { get; }
    }
}
