using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Application.Game;
using Lejuka.Contracts.Common;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Players;
using Lejuka.Domain.Tables;

namespace Lejuka.Application.Tables
{
    /// <summary>Free-play use cases: list the tables for a player, and play a match at one.</summary>
    public sealed class TableService
    {
        // In free play the profile owner always sits in seat A and the opponent in seat B.
        private const Seat PlayerSeat = Seat.A;
        private const string OpponentId = "opponent";
        private const string OpponentName = "Opponent";

        private readonly TableCatalog _catalog;
        private readonly ProfileStore _profiles;
        private readonly MatchRunner _matchRunner;
        private readonly FreePlaySettings _settings;

        /// <summary>Creates the service.</summary>
        public TableService(
            TableCatalog catalog,
            ProfileStore profiles,
            MatchRunner matchRunner,
            FreePlaySettings settings)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
            _matchRunner = matchRunner ?? throw new ArgumentNullException(nameof(matchRunner));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>Returns every table in catalog order with the player's access to it.</summary>
        public IReadOnlyList<TableStatus> GetTables(PlayerProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            var statuses = new List<TableStatus>(_catalog.Tables.Count);
            foreach (var table in _catalog.Tables)
            {
                statuses.Add(new TableStatus(table, TableAccess.Evaluate(profile, table)));
            }

            return statuses.AsReadOnly();
        }

        /// <summary>
        /// Plays a full match at the table and saves the updated profile.
        /// Throws <see cref="DomainRuleException"/> when the player may not join the table.
        /// Nothing is saved unless the match runs to the end.
        /// </summary>
        public async Task<MatchResult> PlayAsync(
            string profileId,
            string tableId,
            IPlayerAgent opponentAgent,
            IPlayerAgent humanAgent,
            CancellationToken cancellationToken)
        {
            if (opponentAgent == null)
            {
                throw new ArgumentNullException(nameof(opponentAgent));
            }

            if (humanAgent == null)
            {
                throw new ArgumentNullException(nameof(humanAgent));
            }

            var table = _catalog.Get(tableId);
            var profile = await _profiles.LoadOrCreateAsync(profileId, cancellationToken);

            var access = TableAccess.Evaluate(profile, table);
            if (!access.CanJoin)
            {
                throw new DomainRuleException(
                    $"Profile '{profile.Id}' cannot join table '{table.Id}': {access.DenialReason}.");
            }

            var settings = new MatchSettings(
                table.GamesPerMatch,
                table.StakePerGame,
                _settings.DealMode,
                _settings.FirstDealer);

            var player = new MatchParticipant(
                new Player(profile.Id, profile.DisplayName, PlayerKind.Human),
                humanAgent,
                profile.Wallet);

            // The opponent has no profile, so the house stakes it exactly the escrow: the most it can lose.
            var opponentBankroll = EligibilityRule.RequiredCredits(
                table.GamesPerMatch,
                new Credits(table.StakePerGame));
            var opponent = new MatchParticipant(
                new Player(OpponentId, OpponentName, PlayerKind.Ai),
                opponentAgent,
                new Wallet(opponentBankroll));

            var result = await _matchRunner.RunAsync(new MatchSetup(settings, player, opponent), cancellationToken);

            profile.RecordMatch(result.Score, PlayerSeat);
            await _profiles.SaveAsync(profile, cancellationToken);

            return result;
        }
    }
}
