using UnityEngine;
using WorldPainter.Runtime.Providers;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Data
{
    [System.Serializable]
    public class TileRule
    {
        [SerializeField] private Sprite ruleSprite; 
        [SerializeField] private int[] neighborMask = new int[8];
        
        public Sprite RuleSprite => ruleSprite;

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
            for (int i = 0; i < 8; i++)
                neighborMask[i] = 0;
        }
        
        public bool CheckRule(Vector2Int position, TileData currentTile, IWorldDataProvider provider)
        {
            if (provider == null)
                return false;
            
            for (int i = 0; i < 8; i++)
            {
                if (neighborMask[i] == 0) continue;
                
                Vector2Int neighborPos = position + Directions[i];
                TileData neighborTile = provider.GetTileAt(neighborPos);
                
                bool hasTile = neighborTile is not null;
            
                if (neighborMask[i] == 1)
                {
                    if (!hasTile || neighborTile != currentTile)
                        return false;
                }
                else if (neighborMask[i] == 2)
                {
                    if (hasTile && neighborTile == currentTile)
                        return false;
                }

            }
            
            return true;
        }
    }
}