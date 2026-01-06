using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WorldPainter.Runtime.ScriptableObjects;

namespace WorldPainter.Runtime.Core
{
    public class TilePool : MonoBehaviour
    {
        [SerializeField] private Tile tilePrefab;
        [SerializeField] private int initialPoolSize = 50;

        private Queue<Tile> _pool = new();
        private Transform _poolContainer;

        private void Awake()
        {
            _poolContainer = new GameObject("TilePool Container").transform;
            _poolContainer.SetParent(transform);
            _poolContainer.gameObject.SetActive(false);

            WarmPool();
        }

        private void WarmPool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                Tile tile = CreateNewTile();
                tile.transform.SetParent(_poolContainer);
                _pool.Enqueue(tile);
            }
        }

        private Tile CreateNewTile(bool setActive = true)
        {
            Tile tile = Instantiate(tilePrefab);
            tile.gameObject.SetActive(setActive);
            return tile;
        }

        public Tile GetTile(TileData data, Vector2Int gridPosition)
        {
            Tile tile;

            // ПРОБУЕМ БРАТЬ ИЗ ПУЛА, ПОКА НЕ НАЙДЕМ ЖИВОЙ ТАЙЛ
            while (_pool.Count > 0)
            {
                tile = _pool.Dequeue();

                // БЕЗОПАСНАЯ ПРОВЕРКА
                if (tile != null)
                {
                    try
                    {
                        // Если можем получить gameObject - тайл жив
                        var go = tile.gameObject;
                        if (go != null)
                        {
                            go.SetActive(true);
                            tile.Initialize(data, gridPosition);
                            return tile;
                        }
                    }
                    catch
                    {
                        // Тайл уничтожен - продолжаем искать
                        continue;
                    }
                }
            }

            // Если в пуле нет живых тайлов - создаем новый
            tile = CreateNewTile();
            tile.Initialize(data, gridPosition);
            return tile;
        }

        public void ReturnTile(Tile tile)
        {
            if (tile == null) return;

                #if UNITY_EDITOR
            // В РЕДАКТОРЕ УНИЧТОЖАЕМ ТАЙЛЫ
            if (!Application.isPlaying)
            {
                DestroyImmediate(tile.gameObject);
                return;
            }
    #endif

            tile.Recycle();
            tile.transform.SetParent(_poolContainer);
            _pool.Enqueue(tile);
        }
    }
}
