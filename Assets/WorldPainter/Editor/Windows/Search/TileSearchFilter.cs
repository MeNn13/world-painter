using System;
using System.Linq;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Editor.Windows.Search
{
    public class TileSearchFilter
    {
        private string _searchQuery = "";

        public string SearchQuery
        {
            get => _searchQuery;
            set => _searchQuery = value ?? "";
        }

        public bool HasSearchQuery => !string.IsNullOrWhiteSpace(_searchQuery);

        public TileData[] FilterTiles(TileData[] allTiles)
        {
            if (allTiles == null || allTiles.Length == 0 || !HasSearchQuery)
                return Array.Empty<TileData>();

            string query = _searchQuery.Trim().ToLower();

            return allTiles.Where(tile => tile is not null)
                .Where(tile => tile.DisplayName is not null
                               && tile.DisplayName.ToLower().Contains(query)
                               || tile.TileId is not null
                               && tile.TileId.ToLower().Contains(query)).ToArray();
        }
    }
}
