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
            return condition switch
            {
                Condition.This => neighborTile == currentTile,
                Condition.NotThis => neighborTile != currentTile,
                Condition.Any => true,
                Condition.Empty => neighborTile == null,
                Condition.NotEmpty => neighborTile != null,
                Condition.Specific => neighborTile != null && neighborTile.TileId == specificTileId,
                _ => false
            };
        }
    }
}
