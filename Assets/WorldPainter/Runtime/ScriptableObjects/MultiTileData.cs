using UnityEngine;

namespace WorldPainter.Runtime.ScriptableObjects
{
    [CreateAssetMenu(fileName = "MultiTile Data", menuName = "WorldPainter/MultiTileData")]
    public class MultiTileData : TileData
    {
        [Header("Multitile Settings")] [Tooltip("Размер объекта в тайлах (ширина x высота)")]
        public Vector2Int size = Vector2Int.one;

        [Tooltip("Смещение спрайта от левого нижнего угла (в тайлах)")]
        public Vector2 pivotOffset = Vector2.zero;

        [Header("Placement Rules")] [Tooltip("К чему должен крепиться объект")]
        public AttachmentType attachmentType = AttachmentType.Ground;

        [Header("Wall Placement Settings")]
        [Tooltip("С какой стороны объекта требуется стена")]
        public WallAttachmentSide wallAttachmentSide = WallAttachmentSide.Back;
        
        [Header("Wall Placement Settings")]
        public WallAttachmentTarget wallAttachmentTarget = WallAttachmentTarget.BackgroundWall;

        [Tooltip("Точка крепления относительно левого нижнего угла (в тайлах)")]
        public Vector2Int attachmentPoint = Vector2Int.zero;

        [Tooltip("Вторая точка крепления (для дверей и т.п.)")]
        public Vector2Int secondaryAttachmentPoint = Vector2Int.zero;

        [Header("Physics")] [Tooltip("Имеет ли коллизию (нельзя пройти сквозь)")]
        public bool isSolid = true;

        [Tooltip("Можно ли взаимодействовать с объектом")] public bool isInteractable = false;
        
        public bool ContainsPosition(Vector2Int rootPosition, Vector2Int positionToCheck) => 
            positionToCheck.x >= rootPosition.x 
            && positionToCheck.x < rootPosition.x + size.x 
            && positionToCheck.y >= rootPosition.y
            && positionToCheck.y < rootPosition.y + size.y;
        
        public Vector2Int[] GetAllOccupiedPositions(Vector2Int rootPosition)
        {
            var positions = new Vector2Int[size.x * size.y];
            int index = 0;

            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                {
                    positions[index] = rootPosition + new Vector2Int(x, y);
                    index++;
                }

            return positions;
        }
        
        public Vector2Int[] GetAttachmentPoints(Vector2Int rootPosition)
        {
            var points = new System.Collections.Generic.List<Vector2Int>
            {
                rootPosition + attachmentPoint
            };

            if (attachmentType == AttachmentType.GroundAndCeiling && secondaryAttachmentPoint != Vector2Int.zero)
                points.Add(rootPosition + secondaryAttachmentPoint);

            return points.ToArray();
        }
        
        public bool ShouldCheckNeighbors() => 
            attachmentType is not AttachmentType.None;
    }
    
    public enum AttachmentType
    {
        None, // Не крепится
        Ground, // Крепится к земле (стоит на блоках)
        Ceiling, // Крепится к потолку
        Wall, // Крепится к стене (направление будет в отдельном поле)
        GroundAndCeiling // Две точки крепления (двери)
    }
    
    public enum WallAttachmentTarget
    {
        BackgroundWall,   // Только к фоновым стенам
        SolidTile,        // Только к твердым блокам  
        Any               // К чему угодно (стенам или блокам)
    }
    
    public enum WallAttachmentSide
    {
        Back,      // ТОЛЬКО к задней стене (как картины)
        Left,      // ТОЛЬКО к левой стене (специальные случаи)
        Right,     // ТОЛЬКО к правой стене (специальные случаи)
        AnySide    // К любой стороне (как факелы)
    }
}
