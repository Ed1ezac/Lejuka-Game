using System;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Players;

namespace Lejuka.Application.Game
{
    /// <summary>One side of a match: who they are, who decides for them and what they pay from.</summary>
    public sealed class MatchParticipant
    {
        /// <summary>Creates a participant.</summary>
        public MatchParticipant(Player player, IPlayerAgent agent, Wallet wallet)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Agent = agent ?? throw new ArgumentNullException(nameof(agent));
            Wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
        }

        /// <summary>The participant's identity.</summary>
        public Player Player { get; }

        /// <summary>The agent that makes this participant's decisions.</summary>
        public IPlayerAgent Agent { get; }

        /// <summary>The wallet the escrow is reserved in and the match is settled against.</summary>
        public Wallet Wallet { get; }
    }
}
