using System;
using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Data
{
    [Serializable]
    public class NeighborRule
    {
        public enum Condition
        {
            This, // Должен быть ТАКОЙ ЖЕ тайл
            NotThis, // Должен быть НЕ такой тайл
            Any, // Любой тайл (даже пустой)
            Empty, // Должен быть пусто
            NotEmpty, // Должен быть любой тайл (но не пусто)
            Specific // Должен быть конкретный тайл
        }

        [SerializeField] private Condition condition = Condition.This;
        [SerializeField] private string specificTileId = "";

        public Condition RuleCondition => condition;
        public string SpecificTileId => specificTileId;

        public void SetCondition(Condition newCondition)
        {
            condition = newCondition;
        }

        public void SetSpecificTileId(string tileId)
        {
            specificTileId = tileId;
        }

        public bool Check(TileData neighborTile, TileData currentTile)
        {
            switch (condition)
            {
                case Condition.This:
                    return neighborTile == currentTile;

                case Condition.NotThis:
                    return neighborTile != currentTile;

                case Condition.Any:
                    return true;

                case Condition.Empty:
                    return neighborTile == null;

                case Condition.NotEmpty:
                    return neighborTile != null;

                case Condition.Specific:
                    return neighborTile != null && neighborTile.TileId == specificTileId;

                default:
                    return false;
            }
        }
    }
}
