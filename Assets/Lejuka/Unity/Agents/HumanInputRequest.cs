using Lejuka.Contracts.Players;

namespace Lejuka.Unity.Agents
{
    /// <summary>One decision the game is waiting on a human for.</summary>
    public sealed class HumanInputRequest
    {
        private HumanInputRequest(HumanInputKind kind, Seat seat, int minCut, int maxCut)
        {
            Kind = kind;
            Seat = seat;
            MinCut = minCut;
            MaxCut = maxCut;
        }

        /// <summary>The decision being asked for.</summary>
        public HumanInputKind Kind { get; }

        /// <summary>The seat that has to decide.</summary>
        public Seat Seat { get; }

        /// <summary>Smallest cut the game offers. Only meaningful for <see cref="HumanInputKind.Cut"/>.</summary>
        public int MinCut { get; }

        /// <summary>Largest cut the game offers. Only meaningful for <see cref="HumanInputKind.Cut"/>.</summary>
        public int MaxCut { get; }

        /// <summary>The Dealer is asked to shuffle.</summary>
        public static HumanInputRequest ForShuffle(Seat dealer)
        {
            return new HumanInputRequest(HumanInputKind.Shuffle, dealer, 0, 0);
        }

        /// <summary>The Cutter is asked for a cut within the given range.</summary>
        public static HumanInputRequest ForCut(Seat cutter, int minCut, int maxCut)
        {
            return new HumanInputRequest(HumanInputKind.Cut, cutter, minCut, maxCut);
        }

        /// <summary>The Cutter is asked to call the target rank.</summary>
        public static HumanInputRequest ForRank(Seat cutter)
        {
            return new HumanInputRequest(HumanInputKind.Rank, cutter, 0, 0);
        }
    }
}
