using System;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Game;

namespace Lejuka.Application.Game
{
    /// <summary>What a stage handler needs to run one game: the game and the agent for each role.</summary>
    public sealed class GameContext
    {
        /// <summary>Creates the context for one game.</summary>
        public GameContext(GameRound round, IPlayerAgent dealerAgent, IPlayerAgent cutterAgent)
        {
            Round = round ?? throw new ArgumentNullException(nameof(round));
            DealerAgent = dealerAgent ?? throw new ArgumentNullException(nameof(dealerAgent));
            CutterAgent = cutterAgent ?? throw new ArgumentNullException(nameof(cutterAgent));
        }

        /// <summary>The game being played.</summary>
        public GameRound Round { get; }

        /// <summary>The agent deciding for this game's Dealer.</summary>
        public IPlayerAgent DealerAgent { get; }

        /// <summary>The agent deciding for this game's Cutter.</summary>
        public IPlayerAgent CutterAgent { get; }
    }
}
