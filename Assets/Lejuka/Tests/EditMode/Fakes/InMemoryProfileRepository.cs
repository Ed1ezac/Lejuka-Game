using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Players;

namespace Lejuka.Tests.EditMode.Fakes
{
    /// <summary>A profile repository held in memory, with a count of the saves it received.</summary>
    public sealed class InMemoryProfileRepository : IProfileRepository
    {
        private readonly Dictionary<string, PlayerProfileData> _profiles = new Dictionary<string, PlayerProfileData>();

        public int SaveCount { get; private set; }

        /// <summary>Stores a profile directly, without counting it as a save.</summary>
        public void Seed(PlayerProfileData profile)
        {
            _profiles[profile.Id] = profile;
        }

        /// <summary>Returns the stored profile, or null when there is none.</summary>
        public PlayerProfileData Find(string id)
        {
            return _profiles.TryGetValue(id, out var profile) ? profile : null;
        }

        public Task<PlayerProfileData> LoadAsync(string id, CancellationToken cancellationToken)
        {
            return cancellationToken.IsCancellationRequested
                ? Task.FromCanceled<PlayerProfileData>(cancellationToken)
                : Task.FromResult(Find(id));
        }

        public Task SaveAsync(PlayerProfileData profile, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            _profiles[profile.Id] = profile;
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}
