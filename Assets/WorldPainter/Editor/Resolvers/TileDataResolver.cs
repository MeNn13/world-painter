using System.Collections.Generic;
using UnityEngine.Tilemaps;
using TileData = WorldPainter.Runtime.ScriptableObjects.TileData;

namespace WorldPainter.Editor.Resolvers
{
    public class TileDataResolver
    {
        private readonly List<TileData> _tileDatabase;

        public TileDataResolver(List<TileData> tileDatabase)
        {
            _tileDatabase = tileDatabase;
        }

        public TileData FindTileDataForTile(TileBase sourceTile)
        {
            foreach (TileData tileData in _tileDatabase)
            {
                if (tileData.RuleTile == sourceTile)
                    return tileData;

                // TODO: Добавить сравнение по спрайту для обычных тайлов
                if (tileData.Sprite is not null && sourceTile is Tile tile && tile.sprite == tileData.Sprite)
                    return tileData;
            }
            return null;
        }

        public ushort GetTileId(TileData tileData, List<TileData> database) => 
            (ushort)(database.IndexOf(tileData) + 1); // +1 because 0 is air

        public byte CalculateHealth(TileData tileData) => 
            (byte)(tileData.Hardness > 0 ? tileData.Hardness : 255);
    }
}
