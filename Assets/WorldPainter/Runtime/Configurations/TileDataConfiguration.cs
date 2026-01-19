using System.Collections.Generic;
using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Configurations
{
    [CreateAssetMenu(fileName = "Tile Data Configuration", menuName = "WorldPainter/TileDataConfig", order = 0)]
    public class TileDataConfiguration : ScriptableObject
    {
        [SerializeField] private TileData[] config;
        public TileData[] Config => config;

        public bool TryGetTileDataFromId(string tileId, out TileData tileData)
        {
            foreach (var data in config)
                if (data.TileId == tileId)
                {
                    tileData = data;
                    return true;
                }

            tileData = null;
            return false;
        }
        public WallData[] GetAllWallData()
        {
            List<WallData> walls = new();

            foreach (var data in config)
                if (data is WallData wallData)
                    walls.Add(wallData);
            
            return walls.ToArray();
        }
        public MultiTileData[] GetAllMultiTileData()
        {
            List<MultiTileData> multiTiles = new();

            foreach (var data in config)
                if (data is MultiTileData multiTileData)
                    multiTiles.Add(multiTileData);
            
            return multiTiles.ToArray();
        }
    }
}
