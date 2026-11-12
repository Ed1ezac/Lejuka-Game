using System;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Cards;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Players;
using Lejuka.Unity.Presentation;
using Lejuka.Unity.Tweening;
using TMPro;
using UnityEngine;

namespace Lejuka.Unity.Presenters
{
    /// <summary>The always-visible HUD: both seat panels, the score, the target rank and the stage banner.</summary>
    public sealed class HudPresenter : MonoBehaviour
    {
        private const float TargetPopScale = 1.4f;
        private const float TargetPopTime = 0.35f;

        [SerializeField] private SeatPanel _seatAPanel = new SeatPanel();
        [SerializeField] private SeatPanel _seatBPanel = new SeatPanel();
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _targetText;
        [SerializeField] private TMP_Text _stageBanner;

        private bool _isWired;

        /// <summary>Redraws the seat panels and the score from the match state.</summary>
        public void Render(MatchViewState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (!_isWired)
            {
                return;
            }

            var seatA = state.SeatState(Seat.A);
            var seatB = state.SeatState(Seat.B);
            _seatAPanel.Render(seatA);
            _seatBPanel.Render(seatB);

            var game = state.GameCount > 0 ? $"Game {state.GameIndex + 1} of {state.GameCount}\n" : string.Empty;
            _scoreText.text = $"{game}{seatA.Name} {seatA.GamesWon} - {seatB.GamesWon} {seatB.Name}";
        }

        /// <summary>Shows what the game is doing in the given stage.</summary>
        public void ShowStage(GameStage stage)
        {
            SetBanner(StageText(stage));
        }

        /// <summary>Puts a message in the stage banner. An empty message clears it.</summary>
        public void SetBanner(string message)
        {
            if (_isWired)
            {
                _stageBanner.text = message ?? string.Empty;
            }
        }

        /// <summary>Shows a message in the stage banner, holds it, then clears it.</summary>
        public async Task ShowBannerAsync(string message, float holdSeconds, CancellationToken cancellationToken)
        {
            await Awaitable.MainThreadAsync();

            SetBanner(message);
            await Tween.DelayAsync(holdSeconds, cancellationToken);
            SetBanner(string.Empty);
        }

        /// <summary>Shows the called rank with a pop.</summary>
        public async Task ShowTargetAsync(Rank rank, CancellationToken cancellationToken)
        {
            await Awaitable.MainThreadAsync();
            if (!_isWired)
            {
                return;
            }

            _targetText.text = $"Target\n{RankLabel.Short(rank)}";
            await Tween.PunchScaleAsync(_targetText.transform, TargetPopScale, TargetPopTime, cancellationToken);
        }

        /// <summary>Removes the called rank, ready for the next game.</summary>
        public void ClearTarget()
        {
            if (_isWired)
            {
                _targetText.text = string.Empty;
            }
        }

        private static string StageText(GameStage stage)
        {
            switch (stage)
            {
                case GameStage.ShuffleAndCut:
                    return "Shuffle and cut";
                case GameStage.AnnounceTarget:
                    return "Call the target";
                case GameStage.Dealing:
                    return "Dealing";
                default:
                    // The ceremony banner names the winner, and a completed game shows nothing.
                    return string.Empty;
            }
        }

        private void Awake()
        {
            // Not short-circuited: every missing reference should be reported, not just the first.
            _isWired = _seatAPanel.IsWired(this, nameof(_seatAPanel))
                       & _seatBPanel.IsWired(this, nameof(_seatBPanel))
                       & WiringGuard.IsAssigned(this, _scoreText, nameof(_scoreText))
                       & WiringGuard.IsAssigned(this, _targetText, nameof(_targetText))
                       & WiringGuard.IsAssigned(this, _stageBanner, nameof(_stageBanner));

            if (_isWired)
            {
                // Clears the placeholder text typed into the scene.
                _scoreText.text = string.Empty;
                _targetText.text = string.Empty;
                _stageBanner.text = string.Empty;
            }
        }
    }
}
