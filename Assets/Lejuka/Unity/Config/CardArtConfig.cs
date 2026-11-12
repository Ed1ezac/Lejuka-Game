using System;
using System.Collections.Generic;
using Lejuka.Contracts.Cards;
using UnityEngine;

namespace Lejuka.Unity.Config
{
    /// <summary>The sprites for the 52 card faces and the card back.</summary>
    [CreateAssetMenu(menuName = "Lejuka/Card Art", fileName = "CardArtConfig")]
    public sealed class CardArtConfig : ScriptableObject
    {
        /// <summary>Number of faces in a full set.</summary>
        public const int FaceCount = SuitCount * RankCount;

        /// <summary>The back used when several are available. Matches the sprite on <c>Card.prefab</c>.</summary>
        public const string PreferredBackName = "Back_Blue_1";

        private const int SuitCount = 4;
        private const int RankCount = 13;

        [SerializeField] private Sprite _back;
        [SerializeField] private List<FaceEntry> _faces = new List<FaceEntry>();

        // Built from _faces on first use. Not serialized, so it is rebuilt after every domain reload.
        private Sprite[] _lookup;

        /// <summary>The card back.</summary>
        public Sprite Back => _back;

        /// <summary>How many of the 52 faces have no sprite.</summary>
        public int MissingFaceCount
        {
            get
            {
                var lookup = Lookup();
                var missing = 0;
                for (var i = 0; i < lookup.Length; i++)
                {
                    if (lookup[i] == null)
                    {
                        missing++;
                    }
                }

                return missing;
            }
        }

        /// <summary>Returns the face sprite for a card, or <c>null</c> when none is assigned.</summary>
        public Sprite GetFace(CardData card)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            return Lookup()[IndexOf(card.Suit, card.Rank)];
        }

        /// <summary>
        /// Replaces the art with whatever the sprite names describe (see <see cref="CardSpriteNames"/>).
        /// Sprites with other names are ignored. Returns the number of faces still missing.
        /// </summary>
        public int AutoFill(IEnumerable<Sprite> sprites)
        {
            if (sprites == null)
            {
                throw new ArgumentNullException(nameof(sprites));
            }

            var faces = new Sprite[FaceCount];
            Sprite back = null;
            string backName = null;

            foreach (var sprite in sprites)
            {
                if (sprite == null)
                {
                    continue;
                }

                if (CardSpriteNames.TryParseFace(sprite.name, out var card))
                {
                    faces[IndexOf(card.Suit, card.Rank)] = sprite;
                }
                else if (CardSpriteNames.TryParseBack(sprite.name, out _, out _) &&
                         IsBetterBack(sprite.name, backName))
                {
                    back = sprite;
                    backName = sprite.name;
                }
            }

            _back = back;
            _faces = new List<FaceEntry>(FaceCount);
            for (var suit = Suit.Clubs; suit <= Suit.Spades; suit++)
            {
                for (var rank = Rank.Ace; rank <= Rank.King; rank++)
                {
                    _faces.Add(new FaceEntry(suit, rank, faces[IndexOf(suit, rank)]));
                }
            }

            _lookup = null;
            return MissingFaceCount;
        }

        private static bool IsBetterBack(string candidate, string current)
        {
            if (current == null)
            {
                return true;
            }

            if (string.Equals(current, PreferredBackName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Otherwise the first name in alphabetical order wins, so the result does not depend on import order.
            return string.Equals(candidate, PreferredBackName, StringComparison.OrdinalIgnoreCase) ||
                   string.Compare(candidate, current, StringComparison.OrdinalIgnoreCase) < 0;
        }

        private static int IndexOf(Suit suit, Rank rank)
        {
            if (suit < Suit.Clubs || suit > Suit.Spades)
            {
                throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown suit.");
            }

            if (rank < Rank.Ace || rank > Rank.King)
            {
                throw new ArgumentOutOfRangeException(nameof(rank), rank, "Unknown rank.");
            }

            return ((int)suit * RankCount) + ((int)rank - (int)Rank.Ace);
        }

        private Sprite[] Lookup()
        {
            if (_lookup != null)
            {
                return _lookup;
            }

            var lookup = new Sprite[FaceCount];
            foreach (var entry in _faces)
            {
                if (entry.IsValid)
                {
                    lookup[IndexOf(entry.Suit, entry.Rank)] = entry.Sprite;
                }
            }

            _lookup = lookup;
            return lookup;
        }

        private void OnValidate()
        {
            // The list may have been edited in the Inspector.
            _lookup = null;
        }

        [Serializable]
        private struct FaceEntry
        {
            [SerializeField] private Suit _suit;
            [SerializeField] private Rank _rank;
            [SerializeField] private Sprite _sprite;

            public FaceEntry(Suit suit, Rank rank, Sprite sprite)
            {
                _suit = suit;
                _rank = rank;
                _sprite = sprite;
            }

            public Suit Suit => _suit;

            public Rank Rank => _rank;

            public Sprite Sprite => _sprite;

            // A new list element in the Inspector starts with rank 0, which is not a real rank.
            public bool IsValid => _suit >= Suit.Clubs && _suit <= Suit.Spades && _rank >= Rank.Ace && _rank <= Rank.King;
        }
    }
}
