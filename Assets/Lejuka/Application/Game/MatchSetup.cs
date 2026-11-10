using System;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Players;

namespace Lejuka.Application.Game
{
    /// <summary>Everything needed to run a match: its settings and the participant in each seat.</summary>
    public sealed class MatchSetup
    {
        /// <summary>Creates the setup. The two seats must not share a wallet.</summary>
        public MatchSetup(MatchSettings settings, MatchParticipant seatA, MatchParticipant seatB)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            SeatA = seatA ?? throw new ArgumentNullException(nameof(seatA));
            SeatB = seatB ?? throw new ArgumentNullException(nameof(seatB));

            // One wallet in both seats would reserve twice and then settle against itself.
            if (ReferenceEquals(seatA.Wallet, seatB.Wallet))
            {
                throw new ArgumentException("The two seats must use different wallets.", nameof(seatB));
            }
        }

        /// <summary>The match settings.</summary>
        public MatchSettings Settings { get; }

        /// <summary>The participant in seat A.</summary>
        public MatchParticipant SeatA { get; }

        /// <summary>The participant in seat B.</summary>
        public MatchParticipant SeatB { get; }

        /// <summary>Returns the participant in the given seat.</summary>
        public MatchParticipant ParticipantAt(Seat seat)
        {
            switch (seat)
            {
                case Seat.A:
                    return SeatA;
                case Seat.B:
                    return SeatB;
                default:
                    throw new ArgumentOutOfRangeException(nameof(seat), seat, "Unknown seat.");
            }
        }
    }
}
