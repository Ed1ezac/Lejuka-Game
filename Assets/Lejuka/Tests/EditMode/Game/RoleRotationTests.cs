using System;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Game;
using NUnit.Framework;

namespace Lejuka.Tests.EditMode.Game
{
    public sealed class RoleRotationTests
    {
        [Test]
        public void FirstGame_IsDealtByTheFirstDealer()
        {
            Assert.That(new RoleRotation(Seat.A).RolesFor(0), Is.EqualTo(new GameRoles(Seat.A, Seat.B)));
            Assert.That(new RoleRotation(Seat.B).RolesFor(0), Is.EqualTo(new GameRoles(Seat.B, Seat.A)));
        }

        [Test]
        public void Roles_SwapEveryGame()
        {
            var rotation = new RoleRotation(Seat.A);

            Assert.That(rotation.RolesFor(1), Is.EqualTo(new GameRoles(Seat.B, Seat.A)));
            Assert.That(rotation.RolesFor(2), Is.EqualTo(new GameRoles(Seat.A, Seat.B)));
            Assert.That(rotation.RolesFor(3), Is.EqualTo(new GameRoles(Seat.B, Seat.A)));
        }

        [Test]
        public void NegativeGameIndex_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RoleRotation(Seat.A).RolesFor(-1));
        }
    }
}
