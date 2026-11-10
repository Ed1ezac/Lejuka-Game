using System;
using System.Linq;
using Lejuka.Contracts.Tables;
using Lejuka.Domain.Tables;
using NUnit.Framework;

namespace Lejuka.Tests.EditMode.Tables
{
    public sealed class TableCatalogTests
    {
        [Test]
        public void Tables_KeepTheirConfiguredOrder()
        {
            var catalog = new TableCatalog(new[]
            {
                Table("bronze", 10),
                Table("silver", 50),
                Table("gold", 50)
            });

            Assert.That(catalog.Tables.Select(table => table.Id), Is.EqualTo(new[] { "bronze", "silver", "gold" }));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        [TestCase(25)]
        public void Catalog_AcceptsAnyNumberOfTables(int count)
        {
            var tables = Enumerable.Range(0, count).Select(index => Table("table-" + index, 10 * (index + 1)));

            var catalog = new TableCatalog(tables);

            Assert.That(catalog.Tables.Count, Is.EqualTo(count));
        }

        [Test]
        public void Get_ReturnsTheTableWithThatId()
        {
            var silver = Table("silver", 50);
            var catalog = new TableCatalog(new[] { Table("bronze", 10), silver });

            Assert.That(catalog.Get("silver"), Is.SameAs(silver));
        }

        [Test]
        public void Get_UnknownId_Throws()
        {
            var catalog = new TableCatalog(new[] { Table("bronze", 10) });

            Assert.Throws<ArgumentException>(() => catalog.Get("platinum"));
            Assert.Throws<ArgumentException>(() => catalog.Get(null));
        }

        [Test]
        public void DuplicateIds_AreRejected()
        {
            Assert.Throws<ArgumentException>(
                () => new TableCatalog(new[] { Table("bronze", 10), Table("bronze", 20) }));
        }

        [Test]
        public void DecreasingStakes_AreRejected()
        {
            Assert.Throws<ArgumentException>(
                () => new TableCatalog(new[] { Table("silver", 50), Table("bronze", 10) }));
        }

        [Test]
        public void BlankId_IsRejected()
        {
            Assert.Throws<ArgumentException>(() => new TableCatalog(new[] { Table(" ", 10) }));
        }

        [TestCase(0)]
        [TestCase(3)]
        public void InvalidGamesPerMatch_IsRejected(int gamesPerMatch)
        {
            var table = new TableDefinition("bronze", "Bronze", 10, gamesPerMatch, 0);

            Assert.Throws<ArgumentOutOfRangeException>(() => new TableCatalog(new[] { table }));
        }

        [Test]
        public void NonPositiveStake_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TableCatalog(new[] { Table("bronze", 0) }));
        }

        [Test]
        public void NegativeMatchWinsRequired_IsRejected()
        {
            var table = new TableDefinition("bronze", "Bronze", 10, 2, -1);

            Assert.Throws<ArgumentOutOfRangeException>(() => new TableCatalog(new[] { table }));
        }

        private static TableDefinition Table(string id, int stakePerGame)
        {
            return new TableDefinition(id, id, stakePerGame, 2, 0);
        }
    }
}
