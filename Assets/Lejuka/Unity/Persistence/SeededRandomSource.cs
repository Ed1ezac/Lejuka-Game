using System;
using Lejuka.Application.Game;
using Lejuka.Contracts.Common;

namespace Lejuka.Unity.Persistence
{
    /// <summary>
    /// The game's <see cref="IRandomSource"/>, restarted from a fresh seed for every match.
    /// The core services keep one random source for their whole lifetime, so reseeding
    /// has to happen behind it.
    /// </summary>
    public sealed class SeededRandomSource : IRandomSource
    {
        private readonly SeedProvider _seeds;
        private IRandomSource _current;

        /// <summary>Creates the source. It takes its first seed when it is first used or reseeded.</summary>
        public SeededRandomSource(SeedProvider seeds)
        {
            _seeds = seeds ?? throw new ArgumentNullException(nameof(seeds));
        }

        /// <summary>Starts a new sequence from the provider's next seed. Call once before each match.</summary>
        public void Reseed()
        {
            _current = new SystemRandomSource(_seeds.NextSeed());
        }

        /// <inheritdoc />
        public int Next(int minInclusive, int maxExclusive)
        {
            if (_current == null)
            {
                Reseed();
            }

            return _current.Next(minInclusive, maxExclusive);
        }
    }
}
