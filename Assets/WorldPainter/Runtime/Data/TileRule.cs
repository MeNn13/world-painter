// 📁 WorldPainter/Runtime/Data/TileRule.cs
using UnityEngine;
using WorldPainter.Runtime.Providers;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Data
{
    [System.Serializable]
    public class TileRule
    {
        [SerializeField] private Sprite ruleSprite; // Спрайт для этого правила
        [SerializeField] private int[] neighborMask = new int[8];
        
        public Sprite RuleSprite => ruleSprite;
        public int[] NeighborMask => neighborMask;
        
        private static readonly Vector2Int[] Directions = {
            Vector2Int.up,
            Vector2Int.up + Vector2Int.right,
            Vector2Int.right,
            Vector2Int.down + Vector2Int.right,
            Vector2Int.down,
            Vector2Int.down + Vector2Int.left,
            Vector2Int.left,
            Vector2Int.up + Vector2Int.left
        };
        
        public TileRule()
        {
            // По умолчанию все 0 (не проверять)
            for (int i = 0; i < 8; i++)
            {
                neighborMask[i] = 0;
            }
        }
        
        public bool CheckRule(Vector2Int position, TileData currentTile, IWorldDataProvider provider)
        {
            // ЕСЛИ ПРОВАЙДЕР NULL - НЕ МОЖЕМ ПРОВЕРЯТЬ ПРАВИЛА
            if (provider == null)
            {
                return false; // ИЛИ true, если хочешь чтобы правила работали без провайдера
            }
            
            for (int i = 0; i < 8; i++)
            {
                if (neighborMask[i] == 0) continue; // Не проверяем
                
                Vector2Int neighborPos = position + Directions[i];
                TileData neighborTile = provider.GetTileAt(neighborPos);
                bool hasTile = neighborTile != null;
                
                if (neighborMask[i] == 1) // Должен быть такой же тайл
                {
                    if (!hasTile || neighborTile != currentTile)
                        return false;
                }
                else if (neighborMask[i] == 2) // Не должен быть такой же тайл
                {
                    if (hasTile && neighborTile == currentTile)
                        return false;
                }
            }
            
            return true;
        }
    }
}