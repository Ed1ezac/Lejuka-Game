using Lejuka.Application.Tables;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Players;
using UnityEngine;

namespace Lejuka.Unity.Config
{
    /// <summary>Everything about a free-play session that a designer may want to change without code.</summary>
    [CreateAssetMenu(menuName = "Lejuka/Game Config", fileName = "LejukaGameConfig")]
    public sealed class LejukaGameConfig : ScriptableObject
    {
        [Header("Match")]
        [SerializeField] private DealMode _dealMode = DealMode.Classic;
        [SerializeField] private Seat _firstDealer = Seat.A;
        [SerializeField, Min(0)] private int _startingCredits = 20;

        [Header("Seat A (bottom)")]
        [SerializeField] private string _seatAName = "You";
        [SerializeField] private PlayerKind _seatAKind = PlayerKind.Human;

        [Header("Seat B (top)")]
        [SerializeField] private string _seatBName = "AI";
        [SerializeField] private PlayerKind _seatBKind = PlayerKind.Ai;

        [Header("Pacing (seconds)")]
        [SerializeField, Min(0f)] private float _aiDelayMin = 0.5f;
        [SerializeField, Min(0f)] private float _aiDelayMax = 1.2f;
        [SerializeField, Min(0f)] private float _dealTimePerCard = 0.25f;

        [Header("Debugging")]
        [Tooltip("0 picks a new seed from the clock for every match. Any other value replays the same cards.")]
        [SerializeField] private int _fixedSeed;

        [Tooltip("Editor only: delete the saved profile every time Play mode starts.")]
        [SerializeField] private bool _resetProfileOnPlay;

        /// <summary>How dealt cards alternate between the players.</summary>
        public DealMode DealMode => _dealMode;

        /// <summary>The seat that deals the first game of each match.</summary>
        public Seat FirstDealer => _firstDealer;

        /// <summary>Credits a first-time player starts with.</summary>
        public int StartingCredits => _startingCredits;

        /// <summary>Shortest pause before an AI decision.</summary>
        public float AiDelayMin => _aiDelayMin;

        /// <summary>Longest pause before an AI decision.</summary>
        public float AiDelayMax => _aiDelayMax;

        /// <summary>How long a dealt card takes to fly from the deck to its pile.</summary>
        public float DealTimePerCard => _dealTimePerCard;

        /// <summary>The seed every match starts from, or 0 to pick one from the clock.</summary>
        public int FixedSeed => _fixedSeed;

        /// <summary>Editor only: whether the saved profile is deleted when Play mode starts.</summary>
        public bool ResetProfileOnPlay => _resetProfileOnPlay;

        /// <summary>The name shown for a seat.</summary>
        public string NameOf(Seat seat)
        {
            return seat == Seat.A ? _seatAName : _seatBName;
        }

        /// <summary>Whether a human or the AI decides for a seat.</summary>
        public PlayerKind KindOf(Seat seat)
        {
            return seat == Seat.A ? _seatAKind : _seatBKind;
        }

        /// <summary>Maps the config to the settings the core's free-play services take.</summary>
        public FreePlaySettings ToFreePlaySettings()
        {
            return new FreePlaySettings(_dealMode, _startingCredits, _firstDealer);
        }

        private void OnValidate()
        {
            _aiDelayMax = Mathf.Max(_aiDelayMax, _aiDelayMin);
        }
    }
}
