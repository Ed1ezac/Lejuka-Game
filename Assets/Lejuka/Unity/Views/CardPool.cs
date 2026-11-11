using System.Collections.Generic;
using Lejuka.Unity.Config;
using UnityEngine;

namespace Lejuka.Unity.Views
{
    /// <summary>
    /// Creates cards from the card prefab and reuses them between games.
    /// Each card handed out is drawn above the ones before it, so overlapping cards stack in deal order.
    /// </summary>
    public sealed class CardPool : MonoBehaviour
    {
        [SerializeField] private CardView _cardPrefab;
        [SerializeField] private CardArtConfig _art;

        [Tooltip("Sorting order of the first card handed out. Keep it above the deck stack.")]
        [SerializeField] private int _baseSortingOrder = 100;

        private readonly Stack<CardView> _free = new Stack<CardView>();
        private int _rentedCount;
        private int _nextSortingOrder;

        /// <summary>Hands out a face-down card at the given world position.</summary>
        public CardView Rent(Vector3 position)
        {
            var card = _free.Count > 0 ? _free.Pop() : Create();
            if (card == null)
            {
                return null;
            }

            card.transform.SetPositionAndRotation(position, Quaternion.identity);
            card.gameObject.SetActive(true);
            card.ResetVisuals();
            card.SortingOrder = _baseSortingOrder + _nextSortingOrder;

            _nextSortingOrder++;
            _rentedCount++;
            return card;
        }

        /// <summary>Takes a card back. Once every card is back, the sorting order starts again from the base.</summary>
        public void Release(CardView card)
        {
            if (card == null)
            {
                return;
            }

            card.gameObject.SetActive(false);
            _free.Push(card);

            _rentedCount--;
            if (_rentedCount <= 0)
            {
                _rentedCount = 0;
                _nextSortingOrder = 0;
            }
        }

        private CardView Create()
        {
            if (!WiringGuard.IsAssigned(this, _cardPrefab, nameof(_cardPrefab)))
            {
                return null;
            }

            var card = Instantiate(_cardPrefab, transform);
            card.name = _cardPrefab.name;
            card.Initialize(_art);
            return card;
        }

        private void Awake()
        {
            WiringGuard.IsAssigned(this, _art, nameof(_art));
        }
    }
}
