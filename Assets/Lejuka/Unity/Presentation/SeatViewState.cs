using Lejuka.Contracts.Players;

namespace Lejuka.Unity.Presentation
{
    /// <summary>What the HUD shows for one seat. Changed only by <see cref="MatchViewState"/>.</summary>
    public sealed class SeatViewState
    {
        /// <summary>Name shown on the seat panel.</summary>
        public string Name { get; internal set; } = string.Empty;

        /// <summary>Whether a human or the AI decides for the seat.</summary>
        public PlayerKind Kind { get; internal set; }

        /// <summary>All credits the seat owns, including the reserved part.</summary>
        public int Balance { get; internal set; }

        /// <summary>Credits held in escrow for the running match.</summary>
        public int Reserved { get; internal set; }

        /// <summary>Credits free to stake.</summary>
        public int Available => Balance - Reserved;

        /// <summary>Games won in the current match.</summary>
        public int GamesWon { get; internal set; }

        /// <summary>Role in the current game.</summary>
        public SeatRole Role { get; internal set; }

        /// <summary>Credits gained (positive) or lost (negative) when the last match settled.</summary>
        public int LastNetChange { get; internal set; }
    }
}
