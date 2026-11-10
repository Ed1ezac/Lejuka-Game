using System;
using Lejuka.Contracts.Common;

namespace Lejuka.Application.Game
{
    /// <summary>
    /// <see cref="IRandomSource"/> backed by <see cref="Random"/>. The seed is always explicit,
    /// so any game can be reproduced from its seed.
    /// </summary>
    public sealed class SystemRandomSource : IRandomSource
    {
        private readonly Random _random;

        /// <summary>Creates a source that yields the same sequence for the same seed.</summary>
        public SystemRandomSource(int seed)
        {
            _random = new Random(seed);
        }

        /// <inheritdoc />
        public int Next(int minInclusive, int maxExclusive)
        {
            if (minInclusive >= maxExclusive)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxExclusive),
                    maxExclusive,
                    $"Must be greater than {nameof(minInclusive)} ({minInclusive}).");
            }

            return _random.Next(minInclusive, maxExclusive);
        }
    }
}
