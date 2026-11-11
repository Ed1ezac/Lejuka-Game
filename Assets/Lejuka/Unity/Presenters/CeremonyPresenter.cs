using System;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Players;
using Lejuka.Unity.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lejuka.Unity.Presenters
{
    /// <summary>The end-of-match panel: who won, the score, the credits won or lost, and Play Again.</summary>
    public sealed class CeremonyPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private Button _playAgainButton;

        private TaskCompletionSource<bool> _playAgain;
        private bool _isWired;
        private bool _isDestroyed;

        /// <summary>Shows the result of the settled match, seen from the saved profile's seat.</summary>
        public void Show(MatchViewState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            // Created here, not when someone starts waiting, so a quick press on Play Again is never lost.
            _playAgain = new TaskCompletionSource<bool>();
            if (!_isWired)
            {
                return;
            }

            _resultText.text = ResultText(state);
            _panel.SetActive(true);
        }

        /// <summary>Completes when Play Again is pressed, then hides the panel.</summary>
        public async Task WaitForPlayAgainAsync(CancellationToken cancellationToken)
        {
            await Awaitable.MainThreadAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var playAgain = _playAgain;
            if (playAgain == null)
            {
                throw new InvalidOperationException("The ceremony panel is not showing a result.");
            }

            if (!_isWired)
            {
                throw new InvalidOperationException(
                    "The ceremony panel is not wired, so Play Again can never be pressed. " + WiringGuard.FixHint);
            }

            using (cancellationToken.Register(() => playAgain.TrySetCanceled(cancellationToken)))
            {
                await playAgain.Task;
            }

            _playAgain = null;
            Hide();
        }

        /// <summary>Hides the panel.</summary>
        public void Hide()
        {
            if (_isWired && !_isDestroyed)
            {
                _panel.SetActive(false);
            }
        }

        private static string ResultText(MatchViewState state)
        {
            var seatA = state.SeatState(Seat.A);
            var seatB = state.SeatState(Seat.B);
            var profileSeat = state.SeatState(MatchViewState.ProfileSeat);

            var net = profileSeat.LastNetChange;
            var change = net > 0 ? "+" + net : net.ToString();

            return $"{Headline(state)}\n" +
                   $"{seatA.Name} {seatA.GamesWon} - {seatB.GamesWon} {seatB.Name}\n" +
                   $"Credits: {change}\n" +
                   $"Balance: {profileSeat.Balance}";
        }

        private static string Headline(MatchViewState state)
        {
            if (!state.MatchWinner.HasValue)
            {
                return "Match tied";
            }

            var winner = state.SeatState(state.MatchWinner.Value);
            return winner.Kind == PlayerKind.Human ? "You win the match!" : $"{winner.Name} wins the match";
        }

        private void Awake()
        {
            // Not short-circuited: every missing reference should be reported, not just the first.
            _isWired = WiringGuard.IsAssigned(this, _panel, nameof(_panel))
                       & WiringGuard.IsAssigned(this, _resultText, nameof(_resultText))
                       & WiringGuard.IsAssigned(this, _playAgainButton, nameof(_playAgainButton));

            if (_isWired)
            {
                _playAgainButton.onClick.AddListener(OnPlayAgainClicked);
                _panel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            _isDestroyed = true;
        }

        private void OnPlayAgainClicked()
        {
            _playAgain?.TrySetResult(true);
        }
    }
}
