using System;
using System.Collections.Generic;
using Lejuka.Contracts.Tables;
using Lejuka.Domain.Tables;
using UnityEngine;

namespace Lejuka.Unity.Config
{
    /// <summary>The tables on offer in free play, in the order they are shown and tried.</summary>
    [CreateAssetMenu(menuName = "Lejuka/Table Catalog", fileName = "TableCatalogConfig")]
    public sealed class TableCatalogConfig : ScriptableObject
    {
        [SerializeField] private List<TableEntry> _tables = CreatePlaceholderTables();

        /// <summary>Number of tables in the list.</summary>
        public int TableCount => _tables.Count;

        /// <summary>
        /// Builds the domain catalog. The domain validates it, so a bad list
        /// (duplicate id, odd game count, falling stakes) throws here, at startup.
        /// </summary>
        public TableCatalog ToCatalog()
        {
            var definitions = new List<TableDefinition>(_tables.Count);
            foreach (var table in _tables)
            {
                definitions.Add(table.ToDefinition());
            }

            return new TableCatalog(definitions);
        }

        private static List<TableEntry> CreatePlaceholderTables()
        {
            // Placeholder values until the real table design is agreed: stake, games per match, wins to unlock.
            return new List<TableEntry>
            {
                new TableEntry("table-1", "Table 1", 1, 2, 0, true),
                new TableEntry("table-2", "Table 2", 2, 4, 2, true),
                new TableEntry("table-3", "Table 3", 5, 6, 5, true)
            };
        }

        // Called by the Editor when the asset is created or reset from the Inspector menu.
        private void Reset()
        {
            _tables = CreatePlaceholderTables();
        }

        [Serializable]
        private sealed class TableEntry
        {
            [SerializeField] private string _id;
            [SerializeField] private string _displayName;
            [SerializeField, Min(1)] private int _stakePerGame = 1;
            [SerializeField, Min(2)] private int _gamesPerMatch = 2;
            [SerializeField, Min(0)] private int _matchWinsRequired;

            [Tooltip("Marks values that are stand-ins and still need a design decision.")]
            [SerializeField] private bool _isPlaceholder;

            // Unity creates new list elements in the Inspector through this constructor.
            public TableEntry()
            {
            }

            public TableEntry(
                string id,
                string displayName,
                int stakePerGame,
                int gamesPerMatch,
                int matchWinsRequired,
                bool isPlaceholder)
            {
                _id = id;
                _displayName = displayName;
                _stakePerGame = stakePerGame;
                _gamesPerMatch = gamesPerMatch;
                _matchWinsRequired = matchWinsRequired;
                _isPlaceholder = isPlaceholder;
            }

            public bool IsPlaceholder => _isPlaceholder;

            public TableDefinition ToDefinition()
            {
                return new TableDefinition(_id, _displayName, _stakePerGame, _gamesPerMatch, _matchWinsRequired);
            }
        }
    }
}
