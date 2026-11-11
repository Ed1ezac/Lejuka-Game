using System;
using UnityEngine;

namespace Lejuka.Unity.Persistence
{
    /// <summary>
    /// Chooses the random seed for each match and logs it.
    /// To replay a match, put its logged seed in the game config's fixed seed: the first match then repeats it.
    /// </summary>
    public sealed class SeedProvider
    {
        private readonly int _fixedSeed;
        private int _matchNumber;

        /// <summary>Creates the provider. A fixed seed of 0 means "pick one from the clock for every match".</summary>
        public SeedProvider(int fixedSeed)
        {
            _fixedSeed = fixedSeed;
        }

        /// <summary>Returns the seed for the next match and logs it.</summary>
        public int NextSeed()
        {
            var seed = _fixedSeed != 0
                ? unchecked(_fixedSeed + _matchNumber)
                : unchecked((int)DateTime.UtcNow.Ticks);

            _matchNumber++;
            Debug.Log($"[Lejuka] Match {_matchNumber} uses seed {seed}.");
            return seed;
        }
    }
}
