using System;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Players;

namespace Lejuka.Domain.Game
{
    /// <summary>Assigns Dealer and Cutter for each game of a match. The roles swap every game.</summary>
    public sealed class RoleRotation
    {
        private readonly Seat _firstDealer;
        private readonly Seat _secondDealer;

        /// <summary>Creates a rotation in which <paramref name="firstDealer"/> deals the first game.</summary>
        public RoleRotation(Seat firstDealer)
        {
            _firstDealer = firstDealer;
            _secondDealer = firstDealer.Opponent();
        }

        /// <summary>Returns the roles for the game at the 0-based <paramref name="gameIndex"/>.</summary>
        public GameRoles RolesFor(int gameIndex)
        {
            if (gameIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gameIndex), gameIndex, "Game index cannot be negative.");
            }

            return gameIndex % 2 == 0
                ? new GameRoles(_firstDealer, _secondDealer)
                : new GameRoles(_secondDealer, _firstDealer);
        }
    }
}
