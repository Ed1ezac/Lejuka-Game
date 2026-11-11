using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Players;
using UnityEngine;

namespace Lejuka.Unity.Persistence
{
    /// <summary>
    /// Keeps the one local profile in a JSON file. The mockup has a single player,
    /// so the file holds one profile and a different id simply finds nothing.
    /// </summary>
    public sealed class JsonProfileRepository : IProfileRepository
    {
        /// <summary>The file name used inside the persistent data folder.</summary>
        public const string DefaultFileName = "lejuka-profile.json";

        private const string TempSuffix = ".tmp";

        private readonly string _filePath;

        /// <summary>Creates a repository that reads and writes the given file.</summary>
        public JsonProfileRepository(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("The profile file path must not be blank.", nameof(filePath));
            }

            _filePath = filePath;
        }

        /// <summary>The file the profile is stored in.</summary>
        public string FilePath => _filePath;

        /// <inheritdoc />
        public Task<PlayerProfileData> LoadAsync(string id, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<PlayerProfileData>(cancellationToken);
            }

            return Task.FromResult(Load(id));
        }

        /// <inheritdoc />
        public Task SaveAsync(PlayerProfileData profile, CancellationToken cancellationToken)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            Save(profile);
            return Task.CompletedTask;
        }

        /// <summary>Deletes the stored profile, if there is one.</summary>
        public void Delete()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }

        private PlayerProfileData Load(string id)
        {
            if (!File.Exists(_filePath))
            {
                return null;
            }

            ProfileDto dto;
            try
            {
                dto = JsonUtility.FromJson<ProfileDto>(File.ReadAllText(_filePath));
            }
            catch (ArgumentException exception)
            {
                // JsonUtility throws this for text that is not JSON. The game carries on with a fresh profile.
                Debug.LogWarning($"[Lejuka] The profile file '{_filePath}' is unreadable and will be replaced. {exception.Message}");
                return null;
            }

            if (dto == null || !string.Equals(dto.id, id, StringComparison.Ordinal))
            {
                return null;
            }

            return new PlayerProfileData(
                dto.id,
                dto.displayName,
                dto.credits,
                dto.matchesPlayed,
                dto.matchesWon,
                dto.gamesWon);
        }

        private void Save(PlayerProfileData profile)
        {
            var dto = new ProfileDto
            {
                id = profile.Id,
                displayName = profile.DisplayName,
                credits = profile.Credits,
                matchesPlayed = profile.MatchesPlayed,
                matchesWon = profile.MatchesWon,
                gamesWon = profile.GamesWon
            };

            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Written to a temporary file first, so a crash mid-write cannot leave a half-written profile.
            var tempPath = _filePath + TempSuffix;
            File.WriteAllText(tempPath, JsonUtility.ToJson(dto, true));
            File.Copy(tempPath, _filePath, true);
            File.Delete(tempPath);
        }

        // Public lower-case fields because that is the shape JsonUtility reads and writes.
        [Serializable]
        private sealed class ProfileDto
        {
            public string id;
            public string displayName;
            public int credits;
            public int matchesPlayed;
            public int matchesWon;
            public int gamesWon;
        }
    }
}
