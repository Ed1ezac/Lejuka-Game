using System;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Game;
using Lejuka.Domain.Players;

namespace Lejuka.Domain.Tables
{
    /// <summary>A player's wallet and career statistics.</summary>
    public sealed class PlayerProfile
    {
        /// <summary>Restores a profile from stored values.</summary>
        public PlayerProfile(
            string id,
            string displayName,
            Wallet wallet,
            int matchesPlayed,
            int matchesWon,
            int gamesWon)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Profile id must not be blank.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("Profile display name must not be blank.", nameof(displayName));
            }

            if (matchesPlayed < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(matchesPlayed), matchesPlayed, "Cannot be negative.");
            }

            if (matchesWon < 0 || matchesWon > matchesPlayed)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(matchesWon),
                    matchesWon,
                    "Must be between zero and the number of matches played.");
            }

            if (gamesWon < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gamesWon), gamesWon, "Cannot be negative.");
            }

            Id = id;
            DisplayName = displayName;
            Wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            MatchesPlayed = matchesPlayed;
            MatchesWon = matchesWon;
            GamesWon = gamesWon;
        }

        /// <summary>Stable profile id.</summary>
        public string Id { get; }

        /// <summary>Name shown to the player.</summary>
        public string DisplayName { get; }

        /// <summary>The player's credits.</summary>
        public Wallet Wallet { get; }

        /// <summary>Total completed matches.</summary>
        public int MatchesPlayed { get; private set; }

        /// <summary>Matches won outright. A tied match is played but not won.</summary>
        public int MatchesWon { get; private set; }

        /// <summary>Total single games won.</summary>
        public int GamesWon { get; private set; }

        /// <summary>Creates a profile for a first-time player.</summary>
        public static PlayerProfile CreateNew(string id, string displayName, Credits startingCredits)
        {
            return new PlayerProfile(id, displayName, new Wallet(startingCredits), 0, 0, 0);
        }

        /// <summary>
        /// Adds a finished match to the statistics. The wallet is not touched here:
        /// credits move when the match itself settles.
        /// </summary>
        public void RecordMatch(MatchScore score, Seat seat)
        {
            if (score == null)
            {
                throw new ArgumentNullException(nameof(score));
            }

            var gamesWon = score.WinsFor(seat);

            MatchesPlayed++;
            GamesWon += gamesWon;
            if (score.Leader == seat)
            {
                MatchesWon++;
            }
        }
    }
}
