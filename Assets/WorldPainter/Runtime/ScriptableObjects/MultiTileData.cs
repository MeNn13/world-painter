using UnityEngine;

namespace WorldPainter.Runtime.ScriptableObjects
{
    [CreateAssetMenu(fileName = "MultiTile Data", menuName = "WorldPainter/MultiTileData")]
    public class MultiTileData : TileData
    {
        public enum AttachmentType
        {
            None, // Не крепится
            Ground, // Крепится к земле (стоит на блоках)
            Ceiling, // Крепится к потолку
            Wall, // Крепится к стене (направление будет в отдельном поле)
            GroundAndCeiling // Две точки крепления (двери)
        }

        public enum WallDirection
        {
            Back, // Задняя стена
            Left,
            Right,
            Front // Передняя стена
        }

        [Header("Multitile Settings")] [Tooltip("Размер объекта в тайлах (ширина x высота)")]
        public Vector2Int size = Vector2Int.one;

        [Tooltip("Смещение спрайта от левого нижнего угла (в тайлах)")] public Vector2 pivotOffset = Vector2.zero;

        [Header("Placement Rules")] [Tooltip("К чему должен крепиться объект")]
        public AttachmentType attachmentType = AttachmentType.Ground;

        [Tooltip("Направление стены (если attachmentType = Wall)")] public WallDirection wallDirection = WallDirection.Back;

        [Tooltip("Точка крепления относительно левого нижнего угла (в тайлах)")]
        public Vector2Int attachmentPoint = Vector2Int.zero;

        [Tooltip("Вторая точка крепления (для дверей и т.п.)")] public Vector2Int secondaryAttachmentPoint = Vector2Int.zero;

        [Header("Physics")] [Tooltip("Имеет ли коллизию (нельзя пройти сквозь)")]
        public bool isSolid = true;

        [Tooltip("Можно ли взаимодействовать с объектом")] public bool isInteractable = false;

        /// <summary>
        /// Проверяет, находится ли позиция внутри этого мультитайла
        /// </summary>
        public bool ContainsPosition(Vector2Int rootPosition, Vector2Int positionToCheck)
        {
            return positionToCheck.x >= rootPosition.x && positionToCheck.x < rootPosition.x + size.x && positionToCheck.y >= rootPosition.y && positionToCheck.y < rootPosition.y + size.y;
        }

        /// <summary>
        /// Получить все позиции, занимаемые этим мультитайлом
        /// </summary>
        public Vector2Int[] GetAllOccupiedPositions(Vector2Int rootPosition)
        {
            Vector2Int[] positions = new Vector2Int[size.x * size.y];
            int index = 0;

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    positions[index] = rootPosition + new Vector2Int(x, y);
                    index++;
                }
            }

            return positions;
        }

        /// <summary>
        /// Получить мировые позиции точек крепления
        /// </summary>
        public Vector2Int[] GetAttachmentPoints(Vector2Int rootPosition)
        {
            var points = new System.Collections.Generic.List<Vector2Int>();

            points.Add(rootPosition + attachmentPoint);

            if (attachmentType == AttachmentType.GroundAndCeiling && secondaryAttachmentPoint != Vector2Int.zero)
            {
                points.Add(rootPosition + secondaryAttachmentPoint);
            }

            return points.ToArray();
        }

        /// <summary>
        /// Проверяет, нужно ли проверять соседей для этого типа крепления
        /// </summary>
        public bool ShouldCheckNeighbors()
        {
            return attachmentType != AttachmentType.None;
        }
    }
}
