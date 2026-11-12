using System;
using Lejuka.Contracts.Players;

namespace Lejuka.Unity.Presentation
{
    /// <summary>What the HUD needs to know about a seat before a match starts.</summary>
    public sealed class SeatSetup
    {
        /// <summary>Creates the setup.</summary>
        /// <param name="name">Name shown on the seat panel.</param>
        /// <param name="kind">Whether a human or the AI decides for the seat.</param>
        /// <param name="balance">Credits the seat brings to the match.</param>
        public SeatSetup(string name, PlayerKind kind, int balance)
        {
            if (balance < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(balance), balance, "Cannot be negative.");
            }

            Name = name ?? string.Empty;
            Kind = kind;
            Balance = balance;
        }

        /// <summary>Name shown on the seat panel.</summary>
        public string Name { get; }

        /// <summary>Whether a human or the AI decides for the seat.</summary>
        public PlayerKind Kind { get; }

        /// <summary>Credits the seat brings to the match.</summary>
        public int Balance { get; }
    }
}
