using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Domain.Cards;
using Lejuka.Unity.Config;
using Lejuka.Unity.Tweening;
using UnityEngine;

namespace Lejuka.Unity.Views
{
    /// <summary>
    /// The deck on the table: a short stack of card backs standing in for all 52 cards.
    /// It only animates. The real deck lives in the domain.
    /// </summary>
    public sealed class DeckView : MonoBehaviour
    {
        private const float ShuffleTime = 0.6f;
        private const float RiffleBeats = 2f;
        private const float CutStepTime = 0.2f;

        [SerializeField] private CardView _cardPrefab;
        [SerializeField] private CardArtConfig _art;

        [Tooltip("How many card backs make up the stack.")]
        [SerializeField, Min(2)] private int _stackSize = 6;

        [Tooltip("Offset between one card of the stack and the next, in world units.")]
        [SerializeField] private Vector2 _stackStep = new Vector2(-0.015f, 0.02f);

        [Tooltip("How far the two halves part during the shuffle, in world units.")]
        [SerializeField] private float _riffleSpread = 0.45f;

        [Tooltip("How far the halves tilt during the shuffle, in degrees.")]
        [SerializeField] private float _riffleTilt = 8f;

        [Tooltip("How far the lifted part slides aside during the cut, in world units.")]
        [SerializeField] private float _cutSlide = 1.6f;

        // Bottom card first.
        private readonly List<CardView> _stack = new List<CardView>();

        /// <summary>World position of the top of the stack, where dealt cards start from.</summary>
        public Vector3 TopPosition => SlotPosition(_stack.Count);

        /// <summary>Riffles the stack: the two halves part and close twice.</summary>
        public async Task ShuffleAsync(CancellationToken cancellationToken)
        {
            await Awaitable.MainThreadAsync();
            if (_stack.Count == 0)
            {
                return;
            }

            await Tween.RunAsync(ShuffleTime, ApplyRiffle, cancellationToken);
            Layout();
        }

        /// <summary>
        /// Shows the cut: the stack splits where <paramref name="cutCount"/> falls in a full deck,
        /// the top part slides aside and goes back in underneath.
        /// </summary>
        public async Task CutAsync(int cutCount, CancellationToken cancellationToken)
        {
            await Awaitable.MainThreadAsync();
            if (_stack.Count < 2)
            {
                return;
            }

            var liftedCount = Mathf.Clamp(
                Mathf.RoundToInt(_stack.Count * (cutCount / (float)Deck.StandardSize)),
                1,
                _stack.Count - 1);
            var keptCount = _stack.Count - liftedCount;

            var slide = new Vector3(_cutSlide, 0f, 0f);
            await Tween.RunAsync(
                CutStepTime,
                progress =>
                {
                    for (var i = keptCount; i < _stack.Count; i++)
                    {
                        _stack[i].transform.position = SlotPosition(i) + (slide * progress);
                    }
                },
                cancellationToken,
                Ease.OutCubic);

            // The lifted part now belongs underneath, so it moves to the front of the bottom-first list.
            var lifted = _stack.GetRange(keptCount, liftedCount);
            _stack.RemoveRange(keptCount, liftedCount);
            _stack.InsertRange(0, lifted);

            var starts = new Vector3[_stack.Count];
            for (var i = 0; i < _stack.Count; i++)
            {
                starts[i] = _stack[i].transform.position;
                _stack[i].SortingOrder = i;
            }

            await Tween.RunAsync(
                CutStepTime,
                progress =>
                {
                    for (var i = 0; i < _stack.Count; i++)
                    {
                        _stack[i].transform.position = Vector3.LerpUnclamped(starts[i], SlotPosition(i), progress);
                    }
                },
                cancellationToken,
                Ease.OutCubic);

            Layout();
        }

        private void Awake()
        {
            if (!WiringGuard.IsAssigned(this, _cardPrefab, nameof(_cardPrefab)))
            {
                return;
            }

            WiringGuard.IsAssigned(this, _art, nameof(_art));

            for (var i = 0; i < _stackSize; i++)
            {
                var card = Instantiate(_cardPrefab, transform);
                card.name = "DeckCard";
                card.Initialize(_art);
                card.ResetVisuals();
                _stack.Add(card);
            }

            Layout();
        }

        private void Layout()
        {
            for (var i = 0; i < _stack.Count; i++)
            {
                _stack[i].transform.SetPositionAndRotation(SlotPosition(i), Quaternion.identity);
                _stack[i].SortingOrder = i;
            }
        }

        private void ApplyRiffle(float progress)
        {
            var parted = Mathf.Abs(Mathf.Sin(progress * Mathf.PI * RiffleBeats));
            for (var i = 0; i < _stack.Count; i++)
            {
                // Alternate cards go left and right, like the two halves of a riffle.
                var side = i % 2 == 0 ? -1f : 1f;
                _stack[i].transform.SetPositionAndRotation(
                    SlotPosition(i) + new Vector3(side * _riffleSpread * parted, 0f, 0f),
                    Quaternion.Euler(0f, 0f, -side * _riffleTilt * parted));
            }
        }

        private Vector3 SlotPosition(int index)
        {
            return transform.position + new Vector3(_stackStep.x * index, _stackStep.y * index, 0f);
        }
    }
}
