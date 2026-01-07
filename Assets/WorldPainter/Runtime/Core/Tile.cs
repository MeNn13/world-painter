// 📁 WorldPainter/Runtime/Core/Tile.cs
using UnityEngine;
using WorldPainter.Runtime.Providers;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Core
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public TileData Data { get; private set; }
        public Vector2Int GridPosition { get; private set; }

        public void Initialize(TileData data, Vector2Int gridPosition, IWorldDataProvider worldProvider = null)
        {
            Data = data;
            GridPosition = gridPosition;

            spriteRenderer ??= GetComponent<SpriteRenderer>();

            if (data != null)
            {
                // ВАЖНО: Проверяем что worldProvider не null
                if (worldProvider != null)
                {
                    Sprite sprite = data.GetSpriteForNeighbors(gridPosition, worldProvider);
                    spriteRenderer.sprite = sprite;
                    spriteRenderer.color = data.TintColor;
                }
                else
                {
                    // Если нет провайдера - используем дефолтный спрайт
                    spriteRenderer.sprite = data.DefaultSprite;
                    spriteRenderer.color = data.TintColor;
                }
            }

            transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
        }

        public void UpdateSprite(IWorldDataProvider worldProvider)
        {
            if (Data != null && worldProvider != null)
            {
                Sprite sprite = Data.GetSpriteForNeighbors(GridPosition, worldProvider);
                spriteRenderer.sprite = sprite;
            }
        }

        public void Recycle()
        {
            Data = null;
            spriteRenderer.sprite = null;
            gameObject.SetActive(false);
        }
    }
}
