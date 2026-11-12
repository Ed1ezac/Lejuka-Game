using System;
using Lejuka.Application.Players;
using Lejuka.Contracts.Common;
using Lejuka.Contracts.Players;
using Lejuka.Unity.Agents;

namespace Lejuka.Bootstrap
{
    /// <summary>Builds the agent for a seat from its kind.</summary>
    public sealed class SeatAgentFactory
    {
        private readonly HumanAgent _humanAgent;
        private readonly IRandomSource _random;
        private readonly IShuffleAnimation _shuffleAnimation;
        private readonly float _aiDelayMin;
        private readonly float _aiDelayMax;

        /// <summary>Creates the factory.</summary>
        /// <param name="humanAgent">The one human agent. Every human seat shares it, as only one seat acts at a time.</param>
        /// <param name="random">The game's random source, used for the AI's decisions.</param>
        /// <param name="shuffleAnimation">Played whenever a seat shuffles.</param>
        /// <param name="aiDelayMin">Shortest AI thinking pause, in seconds.</param>
        /// <param name="aiDelayMax">Longest AI thinking pause, in seconds.</param>
        public SeatAgentFactory(
            HumanAgent humanAgent,
            IRandomSource random,
            IShuffleAnimation shuffleAnimation,
            float aiDelayMin,
            float aiDelayMax)
        {
            _humanAgent = humanAgent ?? throw new ArgumentNullException(nameof(humanAgent));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _shuffleAnimation = shuffleAnimation ?? throw new ArgumentNullException(nameof(shuffleAnimation));
            _aiDelayMin = aiDelayMin;
            _aiDelayMax = aiDelayMax;
        }

        /// <summary>Returns the agent for a seat of the given kind.</summary>
        public IPlayerAgent Create(PlayerKind kind)
        {
            switch (kind)
            {
                case PlayerKind.Human:
                    return new ShuffleAnimatingAgent(_humanAgent, _shuffleAnimation);
                case PlayerKind.Ai:
                    return new ShuffleAnimatingAgent(
                        new ThinkingDelayAgent(new RandomAiAgent(_random), _aiDelayMin, _aiDelayMax),
                        _shuffleAnimation);
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown player kind.");
            }
        }
    }
}
