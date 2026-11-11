using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Cards;
using Lejuka.Unity.Config;
using Lejuka.Unity.Tweening;
using UnityEngine;

namespace Lejuka.Unity.Views
{
    /// <summary>One card on the table: a sprite that can show its back or its face, move, flip and pulse.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class CardView : MonoBehaviour
    {
        private const float HighlightScale = 1.25f;
        private const float HighlightTime = 0.3f;

        private static readonly Color HighlightTint = new Color(1f, 0.9f, 0.45f);

        [SerializeField] private SpriteRenderer _renderer;

        private CardArtConfig _art;
        private Sprite _face;
        private Vector3 _restScale = Vector3.one;
        private bool _isInitialized;
        private bool _isFaceUp;

        /// <summary>Draw order within the card sorting layer. Higher is drawn on top.</summary>
        public int SortingOrder
        {
            get => SpriteRenderer.sortingOrder;
            set => SpriteRenderer.sortingOrder = value;
        }

        // Looked up on demand as well as serialized, so a card works even if its Awake has not run yet.
        private SpriteRenderer SpriteRenderer
        {
            get
            {
                if (_renderer == null)
                {
                    _renderer = GetComponent<SpriteRenderer>();
                }

                return _renderer;
            }
        }

        /// <summary>Gives the card its art. Must be called once before the card is shown.</summary>
        public void Initialize(CardArtConfig art)
        {
            _art = art;
            if (!_isInitialized)
            {
                _restScale = transform.localScale;
                _isInitialized = true;
            }
        }

        /// <summary>Puts the card back to its resting look: normal size, no tint, back showing.</summary>
        public void ResetVisuals()
        {
            transform.localScale = _restScale;
            SpriteRenderer.color = Color.white;
            ShowBack();
        }

        /// <summary>Shows the back of the card straight away.</summary>
        public void ShowBack()
        {
            _isFaceUp = false;

            // Without art the prefab's own sprite stays, which is a card back.
            if (_art != null && _art.Back != null)
            {
                SpriteRenderer.sprite = _art.Back;
            }
        }

        /// <summary>Sets which card this is. The face only becomes visible once the card is flipped.</summary>
        public void SetFace(CardData card)
        {
            _face = _art != null ? _art.GetFace(card) : null;
            if (_face == null)
            {
                Debug.LogWarning($"[Lejuka] No face sprite for {card.Rank} of {card.Suit}. Check the CardArtConfig.", this);
            }

            if (_isFaceUp)
            {
                ShowCurrentSide();
            }
        }

        /// <summary>Turns the card over by squeezing it flat, swapping the sprite and opening it again.</summary>
        public async Task FlipAsync(float duration, CancellationToken cancellationToken)
        {
            await Awaitable.MainThreadAsync();

            var half = duration * 0.5f;
            await Tween.RunAsync(half, progress => SetWidth(1f - progress), cancellationToken);

            _isFaceUp = !_isFaceUp;
            ShowCurrentSide();

            await Tween.RunAsync(half, SetWidth, cancellationToken);
        }

        /// <summary>Moves the card to a world position and rotation.</summary>
        public Task MoveToAsync(
            Vector3 position,
            Quaternion rotation,
            float duration,
            CancellationToken cancellationToken)
        {
            return Tween.MoveAsync(transform, position, rotation, duration, cancellationToken);
        }

        /// <summary>Pulses the card, tints it, and holds so the player can take it in.</summary>
        public async Task HighlightAsync(float holdSeconds, CancellationToken cancellationToken)
        {
            await Awaitable.MainThreadAsync();

            var spriteRenderer = SpriteRenderer;
            var startColor = spriteRenderer.color;
            var colorTween = Tween.RunAsync(
                HighlightTime,
                progress => spriteRenderer.color = Color.Lerp(startColor, HighlightTint, progress),
                cancellationToken);
            var scaleTween = Tween.PunchScaleAsync(transform, HighlightScale, HighlightTime, cancellationToken);

            await Task.WhenAll(colorTween, scaleTween);
            await Tween.DelayAsync(holdSeconds, cancellationToken);
        }

        private void ShowCurrentSide()
        {
            if (!_isFaceUp)
            {
                ShowBack();
            }
            else if (_face != null)
            {
                SpriteRenderer.sprite = _face;
            }
        }

        private void SetWidth(float fraction)
        {
            transform.localScale = new Vector3(_restScale.x * fraction, _restScale.y, _restScale.z);
        }
    }
}
