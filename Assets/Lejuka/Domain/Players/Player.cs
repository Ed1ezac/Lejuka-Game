using System;
using Lejuka.Contracts.Players;

namespace Lejuka.Domain.Players
{
    /// <summary>The identity of someone sitting at a table.</summary>
    public sealed class Player
    {
        /// <summary>Creates a player. Id and display name must not be blank.</summary>
        public Player(string id, string displayName, PlayerKind kind)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Player id must not be blank.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("Player display name must not be blank.", nameof(displayName));
            }

            if (kind != PlayerKind.Human && kind != PlayerKind.Ai)
            {
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown player kind.");
            }

            Id = id;
            DisplayName = displayName;
            Kind = kind;
        }

        /// <summary>Stable player id.</summary>
        public string Id { get; }

        /// <summary>Name shown at the table.</summary>
        public string DisplayName { get; }

        /// <summary>Whether a human or an AI makes this player's decisions.</summary>
        public PlayerKind Kind { get; }
    }
}
