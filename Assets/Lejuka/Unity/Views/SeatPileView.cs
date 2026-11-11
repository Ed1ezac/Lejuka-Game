using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Unity.Tweening;
using UnityEngine;

namespace Lejuka.Unity.Views
{
    /// <summary>
    /// The cards dealt to one seat, laid out as a row centred on this object.
    /// The row tightens as it grows so it never gets wider than the table.
    /// </summary>
    public sealed class SeatPileView : MonoBehaviour
    {
        private const float ClearTime = 0.25f;

        [Tooltip("Gap between neighbouring cards while the pile is small, in world units.")]
        [SerializeField, Min(0f)] private float _maxSpacing = 0.5f;

        [Tooltip("The spacing shrinks to this width divided by the card count once that is smaller.")]
        [SerializeField, Min(0f)] private float _maxWidth = 9f;

        private readonly List<CardView> _cards = new List<CardView>();

        /// <summary>Adds a card and slides every card to its place in the wider row.</summary>
        public async Task AddAsync(CardView card, float duration, CancellationToken cancellationToken)
        {
            await Awaitable.MainThreadAsync();

            _cards.Add(card);

            var cards = _cards.ToArray();
            var starts = new Vector3[cards.Length];
            var ends = new Vector3[cards.Length];
            for (var i = 0; i < cards.Length; i++)
            {
                starts[i] = cards[i].transform.position;
                ends[i] = SlotPosition(i, cards.Length);
            }

            await Tween.RunAsync(
                duration,
                progress =>
                {
                    for (var i = 0; i < cards.Length; i++)
                    {
                        cards[i].transform.position = Vector3.LerpUnclamped(starts[i], ends[i], progress);
                    }
                },
                cancellationToken,
                Ease.OutCubic);
        }

        /// <summary>Gathers every card to <paramref name="gatherPosition"/> and hands them back to the pool.</summary>
        public async Task ClearAsync(Vector3 gatherPosition, CardPool pool, CancellationToken cancellationToken)
        {
            await Awaitable.MainThreadAsync();
            if (_cards.Count == 0)
            {
                return;
            }

            var cards = _cards.ToArray();
            _cards.Clear();

            var starts = new Vector3[cards.Length];
            for (var i = 0; i < cards.Length; i++)
            {
                starts[i] = cards[i].transform.position;
            }

            await Tween.RunAsync(
                ClearTime,
                progress =>
                {
                    for (var i = 0; i < cards.Length; i++)
                    {
                        cards[i].transform.position = Vector3.LerpUnclamped(starts[i], gatherPosition, progress);
                    }
                },
                cancellationToken,
                Ease.InOutSine);

            foreach (var card in cards)
            {
                pool.Release(card);
            }
        }

        /// <summary>Hands every card back to the pool without animating.</summary>
        public void ClearImmediate(CardPool pool)
        {
            foreach (var card in _cards)
            {
                pool.Release(card);
            }

            _cards.Clear();
        }

        private Vector3 SlotPosition(int index, int count)
        {
            var spacing = Mathf.Min(_maxSpacing, _maxWidth / count);
            var offset = (index - ((count - 1) * 0.5f)) * spacing;
            return transform.position + new Vector3(offset, 0f, 0f);
        }
    }
}
