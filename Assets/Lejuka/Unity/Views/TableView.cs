using System;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Game.Events;
using Lejuka.Contracts.Players;
using Lejuka.Unity.Agents;
using Lejuka.Unity.Presentation;
using UnityEngine;

namespace Lejuka.Unity.Views
{
    /// <summary>The table in world space: the deck and one pile per seat.</summary>
    public sealed class TableView : MonoBehaviour, IShuffleAnimation
    {
        private const float MaxFlipTime = 0.15f;

        [SerializeField] private DeckView _deck;
        [SerializeField] private SeatPileView _seatAPile;
        [SerializeField] private SeatPileView _seatBPile;
        [SerializeField] private CardPool _pool;

        private bool _isWired;

        /// <inheritdoc />
        public async Task ShuffleAsync(CancellationToken cancellationToken)
        {
            await Awaitable.MainThreadAsync();
            if (_isWired)
            {
                await _deck.ShuffleAsync(cancellationToken);
            }
        }

        /// <summary>Shows the deck being cut.</summary>
        public async Task CutAsync(int cutCount, CancellationToken cancellationToken)
        {
            await Awaitable.MainThreadAsync();
            if (_isWired)
            {
                await _deck.CutAsync(cutCount, cancellationToken);
            }
        }

        /// <summary>
        /// Deals one card: it flies face-down from the deck to the receiver's pile, flips face-up,
        /// and is highlighted when it is the target card.
        /// </summary>
        public async Task DealAsync(CardDealt dealt, PresentationTimings timings, CancellationToken cancellationToken)
        {
            if (dealt == null)
            {
                throw new ArgumentNullException(nameof(dealt));
            }

            if (timings == null)
            {
                throw new ArgumentNullException(nameof(timings));
            }

            await Awaitable.MainThreadAsync();
            if (!_isWired)
            {
                return;
            }

            var card = _pool.Rent(_deck.TopPosition);
            if (card == null)
            {
                return;
            }

            card.SetFace(dealt.Card);
            await PileFor(dealt.Recipient).AddAsync(card, timings.DealTimePerCard, cancellationToken);
            await card.FlipAsync(Mathf.Min(MaxFlipTime, timings.DealTimePerCard), cancellationToken);

            if (dealt.IsTarget)
            {
                await card.HighlightAsync(timings.TargetCardHold, cancellationToken);
            }
        }

        /// <summary>Gathers both piles back to the deck.</summary>
        public async Task ClearPilesAsync(CancellationToken cancellationToken)
        {
            await Awaitable.MainThreadAsync();
            if (!_isWired)
            {
                return;
            }

            var gatherPosition = _deck.TopPosition;
            await Task.WhenAll(
                _seatAPile.ClearAsync(gatherPosition, _pool, cancellationToken),
                _seatBPile.ClearAsync(gatherPosition, _pool, cancellationToken));
        }

        /// <summary>Removes any cards left on the table without animating. Used when a match starts.</summary>
        public void ClearImmediate()
        {
            if (_isWired)
            {
                _seatAPile.ClearImmediate(_pool);
                _seatBPile.ClearImmediate(_pool);
            }
        }

        private void Awake()
        {
            // Not short-circuited: every missing reference should be reported, not just the first.
            _isWired = WiringGuard.IsAssigned(this, _deck, nameof(_deck))
                       & WiringGuard.IsAssigned(this, _seatAPile, nameof(_seatAPile))
                       & WiringGuard.IsAssigned(this, _seatBPile, nameof(_seatBPile))
                       & WiringGuard.IsAssigned(this, _pool, nameof(_pool));
        }

        private SeatPileView PileFor(Seat seat)
        {
            switch (seat)
            {
                case Seat.A:
                    return _seatAPile;
                case Seat.B:
                    return _seatBPile;
                default:
                    throw new ArgumentOutOfRangeException(nameof(seat), seat, "Unknown seat.");
            }
        }
    }
}
