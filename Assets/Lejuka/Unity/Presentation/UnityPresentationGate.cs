using System;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Game.Events;
using Lejuka.Contracts.Players;
using Lejuka.Unity.Presenters;
using Lejuka.Unity.Views;
using UnityEngine;

namespace Lejuka.Unity.Presentation
{
    /// <summary>
    /// Shows the core's events on screen. Each event updates the match state, then goes to the view
    /// that animates it; the game waits until that animation has finished.
    /// </summary>
    public sealed class UnityPresentationGate : IPresentationGate
    {
        private readonly TableView _table;
        private readonly HudPresenter _hud;
        private readonly CeremonyPresenter _ceremony;
        private readonly MatchViewState _state;
        private readonly PresentationTimings _timings;

        /// <summary>Creates the gate.</summary>
        public UnityPresentationGate(
            TableView table,
            HudPresenter hud,
            CeremonyPresenter ceremony,
            MatchViewState state,
            PresentationTimings timings)
        {
            // Unity's own null check, so a destroyed or unassigned component is rejected as well.
            _table = table != null ? table : throw new ArgumentNullException(nameof(table));
            _hud = hud != null ? hud : throw new ArgumentNullException(nameof(hud));
            _ceremony = ceremony != null ? ceremony : throw new ArgumentNullException(nameof(ceremony));
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _timings = timings ?? throw new ArgumentNullException(nameof(timings));
        }

        /// <inheritdoc />
        public async Task PresentAsync(IGameEvent gameEvent, CancellationToken cancellationToken)
        {
            if (gameEvent == null)
            {
                throw new ArgumentNullException(nameof(gameEvent));
            }

            await Awaitable.MainThreadAsync();
            cancellationToken.ThrowIfCancellationRequested();

            switch (gameEvent)
            {
                case MatchStarted matchStarted:
                    _state.Apply(matchStarted);
                    _ceremony.Hide();
                    _table.ClearImmediate();
                    _hud.ClearTarget();
                    _hud.Render(_state);
                    break;

                case GameStarted gameStarted:
                    _state.Apply(gameStarted);
                    _hud.ClearTarget();
                    _hud.Render(_state);
                    break;

                case StageChanged stageChanged:
                    _hud.ShowStage(stageChanged.Stage);
                    break;

                case DeckCut deckCut:
                    await _table.CutAsync(deckCut.CutCount, cancellationToken);
                    break;

                case TargetAnnounced targetAnnounced:
                    _state.Apply(targetAnnounced);
                    await _hud.ShowTargetAsync(targetAnnounced.TargetRank, cancellationToken);
                    break;

                case CardDealt cardDealt:
                    await _table.DealAsync(cardDealt, _timings, cancellationToken);
                    break;

                case GameWon gameWon:
                    _state.Apply(gameWon);
                    _hud.Render(_state);
                    await _hud.ShowBannerAsync(GameWonText(gameWon), _timings.GameWonHold, cancellationToken);
                    await _table.ClearPilesAsync(cancellationToken);
                    break;

                case MatchSettled matchSettled:
                    _state.Apply(matchSettled);
                    _hud.ClearTarget();
                    _hud.Render(_state);
                    _ceremony.Show(_state);
                    break;

                default:
                    // An event this presentation does not know yet never holds the game up.
                    break;
            }
        }

        private string GameWonText(GameWon gameWon)
        {
            var winner = _state.SeatState(gameWon.Winner);
            var gameNumber = gameWon.GameIndex + 1;

            return winner.Kind == PlayerKind.Human
                ? $"You win game {gameNumber}"
                : $"{winner.Name} wins game {gameNumber}";
        }
    }
}
