using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Lejuka.Contracts.Tables;
using Lejuka.Domain.Game;

namespace Lejuka.Domain.Tables
{
    /// <summary>
    /// The ordered list of tables on offer. Rejects a bad configuration up front,
    /// so a broken table is found at startup rather than when a player sits down.
    /// </summary>
    public sealed class TableCatalog
    {
        private const string TablesParameter = "tables";

        private readonly List<TableDefinition> _tables = new List<TableDefinition>();
        private readonly Dictionary<string, TableDefinition> _tablesById =
            new Dictionary<string, TableDefinition>(StringComparer.Ordinal);

        private readonly ReadOnlyCollection<TableDefinition> _tablesView;

        /// <summary>
        /// Creates the catalog. Ids must be unique, stakes must not decrease down the list,
        /// and every table must describe a valid match.
        /// </summary>
        public TableCatalog(IEnumerable<TableDefinition> tables)
        {
            if (tables == null)
            {
                throw new ArgumentNullException(nameof(tables));
            }

            foreach (var table in tables)
            {
                Add(table);
            }

            _tablesView = _tables.AsReadOnly();
        }

        /// <summary>The tables in catalog order.</summary>
        public IReadOnlyList<TableDefinition> Tables => _tablesView;

        /// <summary>Returns the table with the given id. Throws when the id is unknown.</summary>
        public TableDefinition Get(string tableId)
        {
            if (tableId != null && _tablesById.TryGetValue(tableId, out var table))
            {
                return table;
            }

            throw new ArgumentException($"There is no table with id '{tableId}'.", nameof(tableId));
        }

        private void Add(TableDefinition table)
        {
            if (table == null)
            {
                throw new ArgumentException("The catalog cannot contain a null table.", TablesParameter);
            }

            if (string.IsNullOrWhiteSpace(table.Id))
            {
                throw new ArgumentException("Every table needs an id.", TablesParameter);
            }

            if (_tablesById.ContainsKey(table.Id))
            {
                throw new ArgumentException($"Table id '{table.Id}' is used more than once.", TablesParameter);
            }

            MatchRules.EnsureValidGameCount(table.GamesPerMatch, TablesParameter);
            MatchRules.EnsureValidStake(table.StakePerGame, TablesParameter);
            if (table.MatchWinsRequired < 0)
            {
                throw new ArgumentOutOfRangeException(
                    TablesParameter,
                    table.MatchWinsRequired,
                    $"Table '{table.Id}' cannot require a negative number of match wins.");
            }

            if (_tables.Count > 0 && table.StakePerGame < _tables[_tables.Count - 1].StakePerGame)
            {
                throw new ArgumentException(
                    $"Table '{table.Id}' has a lower stake than the table before it.",
                    TablesParameter);
            }

            _tables.Add(table);
            _tablesById.Add(table.Id, table);
        }
    }
}
