using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Core
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public TileData Data { get; private set; }
        public Vector2Int GridPosition { get; private set; }
        
        public void Initialize(TileData data, Vector2Int gridPosition)
        {
            Data = data;
            GridPosition = gridPosition;
            
            spriteRenderer ??= GetComponent<SpriteRenderer>();
                
            if (data is not null && data.AutoTileSprites.Length > 0)
                spriteRenderer.sprite = data.AutoTileSprites[0];
            
            transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
        }
        
        public void Recycle()
        {
            Data = null;
            gameObject.SetActive(false);
        }
    }
}
