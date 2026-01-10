using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WorldPainter.Editor.Tools
{
    public class PreviewManager
    {
        private readonly Dictionary<string, GameObject> _previewObjects = new();
        private readonly Dictionary<string, SpriteRenderer> _previewRenderers = new();

        public GameObject GetOrCreatePreview(string id, string name)
        {
            if (!_previewObjects.TryGetValue(id, out GameObject previewObject) || previewObject == null)
            {
                previewObject = new GameObject($"{name} Preview ({id})")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                _previewObjects[id] = previewObject;
            }
            return previewObject;
        }

        public SpriteRenderer GetOrCreateSpriteRenderer(string id)
        {
            if (_previewRenderers.TryGetValue(id, out SpriteRenderer renderer))
            {
                if (renderer != null && renderer.gameObject != null)
                    return renderer;
                // Если GameObject уничтожен, удаляем из словарей
                _previewRenderers.Remove(id);
                if (_previewObjects.ContainsKey(id))
                    _previewObjects.Remove(id);
            }

            // Создаем новый GameObject с правильным именем
            GameObject previewObject = new GameObject($"Sprite Preview ({id})")
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            // ВАЖНО: Обязательно добавляем SpriteRenderer
            renderer = previewObject.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 1000; // Чтобы был поверх всего

            // Сохраняем в словари
            _previewObjects[id] = previewObject;
            _previewRenderers[id] = renderer;

            return renderer;
        }

        public void SetPreviewTransform(string id, Vector3 position, Vector3? scale = null)
        {
            if (_previewObjects.TryGetValue(id, out GameObject previewObject) && previewObject != null)
            {
                // Оптимизация: обновляем только если позиция изменилась
                if (previewObject.transform.position != position)
                {
                    previewObject.transform.position = position;
                }
            
                if (scale.HasValue && previewObject.transform.localScale != scale.Value)
                {
                    previewObject.transform.localScale = scale.Value;
                }
            }
        }

        public void SetPreviewSprite(string id, Sprite sprite, Color? color = null)
        {
            if (_previewRenderers.TryGetValue(id, out SpriteRenderer renderer))
            {
                if (renderer != null)
                {
                    renderer.sprite = sprite;
                    if (color.HasValue)
                        renderer.color = color.Value;
                }
            }
            else
            {
                // Если рендерера нет, создаем его
                renderer = GetOrCreateSpriteRenderer(id);
                if (renderer != null)
                {
                    renderer.sprite = sprite;
                    if (color.HasValue)
                        renderer.color = color.Value;
                }
            }
        }

        public void DestroyPreview(string id)
        {
            if (_previewObjects.TryGetValue(id, out GameObject previewObject) && previewObject != null)
            {
                Object.DestroyImmediate(previewObject);
            }
        
            _previewObjects.Remove(id);
            _previewRenderers.Remove(id);
        }

        public void DestroyAllPreviews()
        {
            foreach (GameObject previewObject in _previewObjects.Values.Where(previewObject => previewObject is not null))
                Object.DestroyImmediate(previewObject);

            _previewObjects.Clear();
            _previewRenderers.Clear();
        }

        public bool HasPreview(string id) => _previewObjects.ContainsKey(id);
    }
}
