using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Core
{
    public class MultiTile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        public MultiTileData Data { get; private set; }
        public Vector2Int RootPosition { get; private set; } // Левый нижний угол области
        
        /// <summary>
        /// Инициализирует мультитайл
        /// </summary>
        /// <param name="data">Данные мультитайла</param>
        /// <param name="rootPosition">Левый нижний угол в мировых координатах</param>
        public void Initialize(MultiTileData data, Vector2Int rootPosition)
        {
            Data = data;
            RootPosition = rootPosition;
            
            // Получаем SpriteRenderer если не назначен
            spriteRenderer ??= GetComponent<SpriteRenderer>();
            
            if (data != null && data.DefaultSprite != null)
            {
                spriteRenderer.sprite = data.DefaultSprite;
                spriteRenderer.color = data.TintColor;
                
                // Позиционируем по центру области с учётом pivotOffset
                Vector3 spritePosition = new Vector3(
                    rootPosition.x + data.pivotOffset.x - 0.5f,
                    rootPosition.y + data.pivotOffset.y - 0.5f,
                    0
                );
                
                transform.position = spritePosition;
                
                // Можно также настроить размер коллайдера если нужно
                SetupCollider(data);
            }
            
            gameObject.name = $"MultiTile_{data.DisplayName}_{rootPosition}";
        }
        
        /// <summary>
        /// Настраивает коллайдер для мультитайла
        /// </summary>
        private void SetupCollider(MultiTileData data)
        {
            // Если нужен коллайдер (для isSolid объектов)
            if (data.isSolid)
            {
                var collider = GetComponent<BoxCollider2D>();
                if (collider == null)
                    collider = gameObject.AddComponent<BoxCollider2D>();
                
                // Размер коллайдера = размеру в тайлах
                collider.size = new Vector2(data.size.x, data.size.y);
                collider.offset = new Vector2(
                    data.size.x / 2f - data.pivotOffset.x,
                    data.size.y / 2f - data.pivotOffset.y
                );
            }
        }
        
        /// <summary>
        /// Получает все мировые позиции, занимаемые этим мультитайлом
        /// </summary>
        public Vector2Int[] GetAllOccupiedPositions()
        {
            if (Data == null) 
                return new Vector2Int[0];
                
            return Data.GetAllOccupiedPositions(RootPosition);
        }
        
        /// <summary>
        /// Проверяет, находится ли позиция внутри этого мультитайла
        /// </summary>
        public bool ContainsPosition(Vector2Int worldPos)
        {
            if (Data == null) 
                return false;
                
            return Data.ContainsPosition(RootPosition, worldPos);
        }
        
        /// <summary>
        /// Возвращает позицию относительно левого нижнего угла
        /// </summary>
        public Vector2Int WorldToLocalPosition(Vector2Int worldPos)
        {
            return worldPos - RootPosition;
        }
        
        /// <summary>
        /// Подготовка к возврату в пул
        /// </summary>
        public void Recycle()
        {
            Data = null;
            RootPosition = Vector2Int.zero;
            
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = null;
                spriteRenderer.color = Color.white;
            }
            
            gameObject.SetActive(false);
        }
    }
}