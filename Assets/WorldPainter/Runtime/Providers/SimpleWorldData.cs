using System.Collections.Generic;
using UnityEngine;
using WorldPainter.Runtime.Interfaces;

namespace WorldPainter.Runtime.Providers
{
    public class SimpleWorldData : MonoBehaviour, IWorldDataProvider
    {
        private readonly Dictionary<Vector2Int, string> _tiles = new();

        public string GetTileIdAt(Vector2Int position) => 
            _tiles.GetValueOrDefault(position);

        public void SetTileIdAt(Vector2Int position, string tileId) => 
            _tiles.Add(position, tileId);
    }
}
