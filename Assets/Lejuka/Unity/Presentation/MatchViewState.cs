using System;
using Lejuka.Contracts.Cards;
using Lejuka.Contracts.Game.Events;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Players;
using Lejuka.Domain.Tables;

namespace Lejuka.Unity.Presentation
{
    /// <summary>
    /// The presentation's picture of the running match, built only from the events the core publishes.
    /// The gate applies each event here first; the HUD and the ceremony panel then draw from it.
    /// It decides nothing: winners, roles and transfers all come from the events.
    /// </summary>
    public sealed class MatchViewState
    {
        /// <summary>The seat the saved profile plays from. The core's free-play service always uses seat A.</summary>
        public const Seat ProfileSeat = Seat.A;

        private readonly SeatViewState _seatA = new SeatViewState();
        private readonly SeatViewState _seatB = new SeatViewState();

        /// <summary>Number of games in the match.</summary>
        public int GameCount { get; private set; }

        /// <summary>0-based index of the game being played.</summary>
        public int GameIndex { get; private set; }

        /// <summary>The rank called for the current game, or <c>null</c> before it is called.</summary>
        public Rank? TargetRank { get; private set; }

        /// <summary>The seat that won the settled match, or <c>null</c> for a tie or an unsettled match.</summary>
        public Seat? MatchWinner { get; private set; }

        /// <summary>Returns the state of a seat.</summary>
        public SeatViewState SeatState(Seat seat)
        {
            switch (seat)
            {
                case Seat.A:
                    return _seatA;
                case Seat.B:
                    return _seatB;
                default:
                    throw new ArgumentOutOfRangeException(nameof(seat), seat, "Unknown seat.");
            }
        }

        /// <summary>
        /// Sets who sits where and what they bring. Called by the composition root before each match,
        /// because the events carry no names or balances.
        /// </summary>
        public void BeginMatch(SeatSetup seatA, SeatSetup seatB)
        {
            Seed(_seatA, seatA ?? throw new ArgumentNullException(nameof(seatA)));
            Seed(_seatB, seatB ?? throw new ArgumentNullException(nameof(seatB)));

            GameCount = 0;
            GameIndex = 0;
            TargetRank = null;
            MatchWinner = null;
        }

        /// <summary>Both escrows are now reserved.</summary>
        public void Apply(MatchStarted matchStarted)
        {
            var settings = matchStarted.Settings;

            // The domain's own rule, so the HUD shows exactly what the wallets reserved.
            var escrow = EligibilityRule.RequiredCredits(settings.GameCount, new Credits(settings.StakePerGame)).Amount;

            GameCount = settings.GameCount;
            _seatA.Reserved = escrow;
            _seatB.Reserved = escrow;
        }

        /// <summary>A new game has its roles.</summary>
        public void Apply(GameStarted gameStarted)
        {
            GameIndex = gameStarted.GameIndex;
            TargetRank = null;

            SeatState(gameStarted.Dealer).Role = SeatRole.Dealer;
            SeatState(gameStarted.Cutter).Role = SeatRole.Cutter;
        }

        /// <summary>The target rank is known.</summary>
        public void Apply(TargetAnnounced targetAnnounced)
        {
            TargetRank = targetAnnounced.TargetRank;
        }

        /// <summary>A seat has won a game.</summary>
        public void Apply(GameWon gameWon)
        {
            SeatState(gameWon.Winner).GamesWon++;
        }

        /// <summary>The match is settled: the escrows are released and the net transfer is paid.</summary>
        public void Apply(MatchSettled matchSettled)
        {
            _seatA.GamesWon = matchSettled.GamesWonA;
            _seatB.GamesWon = matchSettled.GamesWonB;
            MatchWinner = matchSettled.Payee;

            Settle(_seatA, NetChange(matchSettled, Seat.A));
            Settle(_seatB, NetChange(matchSettled, Seat.B));
        }

        private static void Seed(SeatViewState seat, SeatSetup setup)
        {
            seat.Name = setup.Name;
            seat.Kind = setup.Kind;
            seat.Balance = setup.Balance;
            seat.Reserved = 0;
            seat.GamesWon = 0;
            seat.Role = SeatRole.None;
            seat.LastNetChange = 0;
        }

        private static int NetChange(MatchSettled matchSettled, Seat seat)
        {
            if (matchSettled.Payee == seat)
            {
                return matchSettled.NetTransfer;
            }

            return matchSettled.Payer == seat ? -matchSettled.NetTransfer : 0;
        }

        private static void Settle(SeatViewState seat, int netChange)
        {
            seat.Balance += netChange;
            seat.Reserved = 0;
            seat.Role = SeatRole.None;
            seat.LastNetChange = netChange;
        }
    }
}
