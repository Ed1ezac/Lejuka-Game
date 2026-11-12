using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Application.Tables;
using Lejuka.Contracts.Tables;
using Lejuka.Domain.Players;
using Lejuka.Domain.Tables;
using Lejuka.Unity.Agents;
using Lejuka.Unity.Persistence;
using Lejuka.Unity.Presentation;
using Lejuka.Unity.Presenters;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Lejuka.Bootstrap
{
    /// <summary>
    /// The mockup's "main": plays match after match at the first table the player can join,
    /// until the scene goes away or no table is left to play at.
    /// </summary>
    public sealed class MockupEntryPoint : IStartable, IDisposable
    {
        private const string ProfileId = "local-player";

        private readonly TableService _tables;
        private readonly ProfileStore _profiles;
        private readonly TableSeats _seats;
        private readonly SeededRandomSource _random;
        private readonly MatchViewState _state;
        private readonly IHumanInputSource _humanInput;
        private readonly HudPresenter _hud;
        private readonly ActionPanelPresenter _actionPanel;
        private readonly CeremonyPresenter _ceremony;

        // Cancelled but never disposed: tasks still unwinding may touch the token after Dispose,
        // and a source that owns no timer has nothing to release.
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        /// <summary>Creates the entry point. Called by the container.</summary>
        [Inject]
        public MockupEntryPoint(
            TableService tables,
            ProfileStore profiles,
            TableSeats seats,
            SeededRandomSource random,
            MatchViewState state,
            IHumanInputSource humanInput,
            HudPresenter hud,
            ActionPanelPresenter actionPanel,
            CeremonyPresenter ceremony)
        {
            _tables = tables ?? throw new ArgumentNullException(nameof(tables));
            _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
            _seats = seats ?? throw new ArgumentNullException(nameof(seats));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _humanInput = humanInput ?? throw new ArgumentNullException(nameof(humanInput));
            _hud = hud != null ? hud : throw new ArgumentNullException(nameof(hud));
            _actionPanel = actionPanel != null ? actionPanel : throw new ArgumentNullException(nameof(actionPanel));
            _ceremony = ceremony != null ? ceremony : throw new ArgumentNullException(nameof(ceremony));
        }

        /// <inheritdoc />
        public void Start()
        {
            _actionPanel.Bind(_humanInput);

            // Not awaited on purpose: the loop runs for the life of the scene and reports its own failures.
            _ = RunAsync(_cancellation.Token);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _cancellation.Cancel();
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Awaitable.MainThreadAsync();

                while (await PlayMatchAsync(cancellationToken))
                {
                    await _ceremony.WaitForPlayAgainAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // The scene is closing. Stopping quietly is the expected outcome.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (_hud != null)
                {
                    _hud.SetBanner("Something went wrong. See the Console for details.");
                }
            }
        }

        /// <summary>Plays one match. Returns false when the player cannot join any table.</summary>
        private async Task<bool> PlayMatchAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var profile = await _profiles.LoadOrCreateAsync(ProfileId, cancellationToken);
            var statuses = _tables.GetTables(profile);

            var table = FirstJoinableTable(statuses);
            if (table == null)
            {
                var reason = DescribeNoTable(statuses, profile);
                Debug.LogWarning("[Lejuka] " + reason);
                _hud.SetBanner(reason);
                return false;
            }

            Debug.Log($"[Lejuka] Playing at '{table.DisplayName}': {table.GamesPerMatch} games, stake {table.StakePerGame}.");

            _random.Reseed();
            _state.BeginMatch(
                new SeatSetup(_seats.SeatA.Name, _seats.SeatA.Kind, profile.Wallet.Balance.Amount),
                new SeatSetup(_seats.SeatB.Name, _seats.SeatB.Kind, Escrow(table).Amount));

            // The service seats the profile in seat A ("humanAgent") and the opponent in seat B.
            await _tables.PlayAsync(
                profileId: ProfileId,
                tableId: table.Id,
                opponentAgent: _seats.SeatB.Agent,
                humanAgent: _seats.SeatA.Agent,
                cancellationToken: cancellationToken);

            return true;
        }

        private static TableDefinition FirstJoinableTable(IReadOnlyList<TableStatus> statuses)
        {
            foreach (var status in statuses)
            {
                if (status.Access.CanJoin)
                {
                    return status.Table;
                }
            }

            return null;
        }

        // The credits a seat must cover to play at the table. It is also the opponent's whole bankroll:
        // the core's free-play service stakes the opponent exactly this much.
        private static Credits Escrow(TableDefinition table)
        {
            return EligibilityRule.RequiredCredits(table.GamesPerMatch, new Credits(table.StakePerGame));
        }

        private static string DescribeNoTable(IReadOnlyList<TableStatus> statuses, PlayerProfile profile)
        {
            if (statuses.Count == 0)
            {
                return "No table to play at: the table catalog is empty.";
            }

            var text = new StringBuilder("No table to play at.");
            foreach (var status in statuses)
            {
                text.Append('\n').Append(status.Table.DisplayName).Append(": ").Append(DescribeDenial(status, profile));
            }

            return text.ToString();
        }

        private static string DescribeDenial(TableStatus status, PlayerProfile profile)
        {
            switch (status.Access.DenialReason)
            {
                case TableDenialReason.Locked:
                    return $"locked until {status.Table.MatchWinsRequired} match wins (you have {profile.MatchesWon})";
                case TableDenialReason.InsufficientCredits:
                    return $"needs {Escrow(status.Table)} credits (you have {profile.Wallet.Available})";
                default:
                    return status.Access.DenialReason.ToString();
            }
        }
    }
}
