using System;
using Lejuka.Contracts.Players;

namespace Lejuka.Domain.Game
{
    /// <summary>Games won by each seat in a match.</summary>
    /// <param name="GamesWonA">Games won by seat A.</param>
    /// <param name="GamesWonB">Games won by seat B.</param>
    public sealed record MatchScore(int GamesWonA, int GamesWonB)
    {
        /// <summary>The seat with more wins, or <c>null</c> when the score is level.</summary>
        public Seat? Leader
        {
            get
            {
                if (GamesWonA == GamesWonB)
                {
                    return null;
                }

                return GamesWonA > GamesWonB ? Seat.A : Seat.B;
            }
        }

        /// <summary>Games won by the given seat.</summary>
        public int WinsFor(Seat seat)
        {
            switch (seat)
            {
                case Seat.A:
                    return GamesWonA;
                case Seat.B:
                    return GamesWonB;
                default:
                    throw new ArgumentOutOfRangeException(nameof(seat), seat, "Unknown seat.");
            }
        }
    }
}
