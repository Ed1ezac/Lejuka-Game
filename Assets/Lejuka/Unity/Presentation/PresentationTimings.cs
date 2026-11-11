using System;

namespace Lejuka.Unity.Presentation
{
    /// <summary>How long the presentation holds on each beat of a game, in seconds.</summary>
    public sealed class PresentationTimings
    {
        /// <summary>Creates the timings from the configured deal speed. The other beats are fixed.</summary>
        public PresentationTimings(float dealTimePerCard)
        {
            if (dealTimePerCard < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(dealTimePerCard), dealTimePerCard, "Cannot be negative.");
            }

            DealTimePerCard = dealTimePerCard;
        }

        /// <summary>How long a dealt card takes to fly from the deck to its pile.</summary>
        public float DealTimePerCard { get; }

        /// <summary>How long the target card stays highlighted before the game moves on.</summary>
        public float TargetCardHold => 0.6f;

        /// <summary>How long the "wins game" banner stays up before the piles are cleared.</summary>
        public float GameWonHold => 1.5f;
    }
}
