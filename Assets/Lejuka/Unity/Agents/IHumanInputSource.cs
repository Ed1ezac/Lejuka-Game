using System;
using Lejuka.Contracts.Cards;

namespace Lejuka.Unity.Agents
{
    /// <summary>
    /// The UI's view of the human player: what the game is waiting for, and how to answer it.
    /// </summary>
    public interface IHumanInputSource
    {
        /// <summary>
        /// Raised on the main thread whenever <see cref="Pending"/> changes.
        /// The argument is the new request, or <c>null</c> when nothing is pending any more.
        /// </summary>
        event Action<HumanInputRequest> PendingChanged;

        /// <summary>The request waiting for an answer, or <c>null</c>.</summary>
        HumanInputRequest Pending { get; }

        /// <summary>Answers a pending shuffle request.</summary>
        void SubmitShuffle();

        /// <summary>Answers a pending cut request.</summary>
        void SubmitCut(int cutCount);

        /// <summary>Answers a pending target request.</summary>
        void SubmitRank(Rank rank);
    }
}
