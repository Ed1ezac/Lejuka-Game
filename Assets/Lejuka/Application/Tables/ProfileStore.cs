using System;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Players;
using Lejuka.Domain.Tables;

namespace Lejuka.Application.Tables
{
    /// <summary>
    /// Loads and saves player profiles as domain objects, and creates the profile
    /// for a first-time player. Keeps the storage format out of the domain.
    /// </summary>
    public sealed class ProfileStore
    {
        private readonly IProfileRepository _repository;
        private readonly Credits _startingCredits;

        /// <summary>Creates the store. Starting credits come from the free-play settings.</summary>
        public ProfileStore(IProfileRepository repository, FreePlaySettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _startingCredits = new Credits(settings.StartingCredits);
        }

        /// <summary>
        /// Loads the profile, or creates a new one with the starting credits when none is stored.
        /// A new profile is not saved until its first match is.
        /// </summary>
        public async Task<PlayerProfile> LoadOrCreateAsync(string profileId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(profileId))
            {
                throw new ArgumentException("Profile id must not be blank.", nameof(profileId));
            }

            var data = await _repository.LoadAsync(profileId, cancellationToken);
            if (data == null)
            {
                // The id doubles as the name until the player picks one.
                return PlayerProfile.CreateNew(profileId, profileId, _startingCredits);
            }

            return new PlayerProfile(
                data.Id,
                data.DisplayName,
                new Wallet(new Credits(data.Credits)),
                data.MatchesPlayed,
                data.MatchesWon,
                data.GamesWon);
        }

        /// <summary>Saves the profile's current balance and statistics.</summary>
        public Task SaveAsync(PlayerProfile profile, CancellationToken cancellationToken)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            var data = new PlayerProfileData(
                profile.Id,
                profile.DisplayName,
                profile.Wallet.Balance.Amount,
                profile.MatchesPlayed,
                profile.MatchesWon,
                profile.GamesWon);

            return _repository.SaveAsync(data, cancellationToken);
        }
    }
}
