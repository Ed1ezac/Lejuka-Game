using System;
using Lejuka.Contracts.Players;

namespace Lejuka.Bootstrap
{
    /// <summary>Who sits in a seat and which agent decides for them.</summary>
    public sealed class SeatBinding
    {
        /// <summary>Creates the binding.</summary>
        public SeatBinding(string name, PlayerKind kind, IPlayerAgent agent)
        {
            Name = name ?? string.Empty;
            Kind = kind;
            Agent = agent ?? throw new ArgumentNullException(nameof(agent));
        }

        /// <summary>Name shown on the HUD.</summary>
        public string Name { get; }

        /// <summary>Whether a human or the AI decides for the seat.</summary>
        public PlayerKind Kind { get; }

        /// <summary>The agent that makes the seat's decisions.</summary>
        public IPlayerAgent Agent { get; }
    }
}
