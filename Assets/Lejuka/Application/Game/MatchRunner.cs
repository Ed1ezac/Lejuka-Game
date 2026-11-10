using System;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Common;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Game.Events;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Game;
using Lejuka.Domain.Players;
using Lejuka.Domain.Tables;

namespace Lejuka.Application.Game
{
    /// <summary>
    /// Runs a whole match: eligibility, escrow, every game with swapping roles, then settlement.
    /// It changes the wallets it is given but saves nothing; persistence is the caller's job.
    /// </summary>
    public sealed class MatchRunner
    {
        private readonly GameRunner _gameRunner;
        private readonly IPresentationGate _gate;

        /// <summary>Creates the runner.</summary>
        public MatchRunner(GameRunner gameRunner, IPresentationGate gate)
        {
            _gameRunner = gameRunner ?? throw new ArgumentNullException(nameof(gameRunner));
            _gate = gate ?? throw new ArgumentNullException(nameof(gate));
        }

        /// <summary>
        /// Plays the match to the end and settles both wallets.
        /// If the match is cancelled or fails, both reservations are released and no credits move.
        /// </summary>
        public async Task<MatchResult> RunAsync(MatchSetup setup, CancellationToken cancellationToken)
        {
            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }

            var match = new Match(setup.Settings);
            var escrow = EligibilityRule.RequiredCredits(match.GameCount, match.StakePerGame);

            // Both seats are checked before either is touched, so a refusal leaves no reservation behind.
            EnsureEligible(setup, Seat.A, escrow);
            EnsureEligible(setup, Seat.B, escrow);
            setup.SeatA.Wallet.Reserve(escrow);
            setup.SeatB.Wallet.Reserve(escrow);

            try
            {
                await _gate.PresentAsync(new MatchStarted(setup.Settings), cancellationToken);
                await PlayGamesAsync(match, setup, cancellationToken);

                var settlement = match.CalculateSettlement();
                await _gate.PresentAsync(ToEvent(settlement), cancellationToken);

                // Credits move last, so the wallets only ever change for a match that ran to the very end.
                setup.SeatA.Wallet.Settle(settlement.SignedDeltaFor(Seat.A));
                setup.SeatB.Wallet.Settle(settlement.SignedDeltaFor(Seat.B));

                return new MatchResult(setup.Settings, settlement.Score, settlement, match.Results);
            }
            catch
            {
                ReleaseReservation(setup.SeatA.Wallet);
                ReleaseReservation(setup.SeatB.Wallet);
                throw;
            }
        }

        private static void EnsureEligible(MatchSetup setup, Seat seat, Credits escrow)
        {
            var available = setup.ParticipantAt(seat).Wallet.Available;
            if (available < escrow)
            {
                throw new DomainRuleException(
                    $"Seat {seat} needs {escrow} available credits for this match but has {available}.");
            }
        }

        private static void ReleaseReservation(Wallet wallet)
        {
            // A zero settlement moves nothing; its only effect is to release the escrow.
            wallet.Settle(0);
        }

        private static MatchSettled ToEvent(Settlement settlement)
        {
            return new MatchSettled(
                settlement.Score.GamesWonA,
                settlement.Score.GamesWonB,
                settlement.Amount.Amount,
                settlement.Payer,
                settlement.Payee);
        }

        private async Task PlayGamesAsync(Match match, MatchSetup setup, CancellationToken cancellationToken)
        {
            while (!match.IsComplete)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var round = match.StartNextGame();
                var context = new GameContext(
                    round,
                    setup.ParticipantAt(round.Dealer).Agent,
                    setup.ParticipantAt(round.Cutter).Agent);

                await _gameRunner.RunAsync(context, cancellationToken);
                match.RecordResult(round);
            }
        }
    }
}
