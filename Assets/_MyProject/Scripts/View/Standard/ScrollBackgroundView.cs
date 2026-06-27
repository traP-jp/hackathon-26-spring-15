using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.View
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class ScrollBackgroundView : ViewBase
    {
        [SerializeField] Image image;
        [SerializeField] float angleDegrees = 180f;
        [SerializeField] float speed = 80f;
        [SerializeField] bool generateTilesInEditor = false;

        readonly List<Image> tiles = new();
        readonly List<Vector2> tileOffsets = new();
        readonly List<Image> generatedTiles = new();

        RectTransform imageRectTransform;
        Vector2 baseAnchoredPosition;
        Vector2 tileSize;
        Vector2 basisX;
        Vector2 basisY;
        Vector2 basisXUnit;
        Vector2 basisYUnit;
        Vector2 scrollOffset;
        int cachedScreenWidth;
        int cachedScreenHeight;
        Vector2 cachedTileSize;
        Quaternion cachedImageLocalRotation;
        Sprite cachedSprite;
        Sprite cachedOverrideSprite;
        bool cachedGenerateTilesInEditor;
        RectTransform rootCanvasRectTransform;

        public override void Initialize()
        {
            imageRectTransform = image.rectTransform;
            baseAnchoredPosition = imageRectTransform.anchoredPosition;
            scrollOffset = Vector2.zero;
            rootCanvasRectTransform = GetRootCanvasRectTransform();

            RebuildTiles();
            CacheDimensions();
            ApplyTilePositions();

            gameObject.SetActive(false);
        }

        public override void Show()
        {
            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
        }

        public override UniTask ShowAsync(CancellationToken ct)
        {
            Show();
            return UniTask.CompletedTask;
        }

        public override UniTask HideAsync(CancellationToken ct)
        {
            Hide();
            return UniTask.CompletedTask;
        }

        void Update()
        {
            EnsureReferences();
            if (!Application.isPlaying)
            {
                baseAnchoredPosition = imageRectTransform.anchoredPosition;
            }

            if (ShouldRebuildTiles())
            {
                RebuildTiles();
                CacheDimensions();
            }

            if (Application.isPlaying)
            {
                var direction = GetMoveDirection();
                scrollOffset += direction * (speed * Time.deltaTime);

                var gridOffset = new Vector2(
                    Vector2.Dot(scrollOffset, basisXUnit),
                    Vector2.Dot(scrollOffset, basisYUnit));
                gridOffset = new Vector2(
                    WrapOffset(gridOffset.x, tileSize.x),
                    WrapOffset(gridOffset.y, tileSize.y));
                scrollOffset = (basisXUnit * gridOffset.x) + (basisYUnit * gridOffset.y);
            }
            else
            {
                scrollOffset = Vector2.zero;
            }

            ApplyTilePositions();
        }

        void RebuildTiles()
        {
            ClearGeneratedTiles();
            tiles.Clear();
            tileOffsets.Clear();

            Canvas.ForceUpdateCanvases();
            tileSize = imageRectTransform.rect.size;

            if (!Application.isPlaying && !generateTilesInEditor)
            {
                tiles.Add(image);
                tileOffsets.Add(Vector2.zero);
                return;
            }

            if (tileSize.x <= 0f || tileSize.y <= 0f)
            {
                throw new InvalidOperationException("ScrollBackgroundView: Image の Rect サイズが 0 です。");
            }

            UpdateTileBasis();

            var viewSize = GetRootCanvasSize(rootCanvasRectTransform);
            var projectedViewSize = ProjectViewSizeToTileAxes(viewSize);
            var columns = Mathf.CeilToInt(projectedViewSize.x / tileSize.x) + 2;
            var rows = Mathf.CeilToInt(projectedViewSize.y / tileSize.y) + 2;

            var minX = -Mathf.FloorToInt(columns * 0.5f);
            var maxX = minX + columns - 1;
            var minY = -Mathf.FloorToInt(rows * 0.5f);
            var maxY = minY + rows - 1;

            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var tile = x == 0 && y == 0
                        ? image
                        : Instantiate(image, imageRectTransform.parent);

                    if (x != 0 || y != 0)
                    {
                        if (!Application.isPlaying)
                        {
                            tile.gameObject.hideFlags = HideFlags.DontSaveInEditor;
                        }

                        generatedTiles.Add(tile);
                    }

                    var offset = (basisX * x) + (basisY * y);
                    tiles.Add(tile);
                    tileOffsets.Add(offset);
                }
            }
        }

        bool ShouldRebuildTiles()
        {
            if (cachedScreenWidth != Screen.width || cachedScreenHeight != Screen.height)
            {
                return true;
            }

            var currentTileSize = imageRectTransform.rect.size;
            if (cachedTileSize != currentTileSize)
            {
                return true;
            }

            var currentRotation = imageRectTransform.localRotation;
            if (Quaternion.Angle(cachedImageLocalRotation, currentRotation) > 0.01f)
            {
                return true;
            }

            if (cachedSprite != image.sprite)
            {
                return true;
            }

            if (cachedOverrideSprite != image.overrideSprite)
            {
                return true;
            }

            if (!Application.isPlaying && cachedGenerateTilesInEditor != generateTilesInEditor)
            {
                return true;
            }

            return false;
        }

        void CacheDimensions()
        {
            cachedScreenWidth = Screen.width;
            cachedScreenHeight = Screen.height;
            cachedTileSize = tileSize;
            cachedImageLocalRotation = imageRectTransform.localRotation;
            cachedSprite = image.sprite;
            cachedOverrideSprite = image.overrideSprite;
            cachedGenerateTilesInEditor = generateTilesInEditor;
        }

        void UpdateTileBasis()
        {
            basisXUnit = (Vector2)(imageRectTransform.localRotation * Vector3.right);
            basisYUnit = (Vector2)(imageRectTransform.localRotation * Vector3.up);
            basisX = basisXUnit * tileSize.x;
            basisY = basisYUnit * tileSize.y;
        }

        RectTransform GetRootCanvasRectTransform()
        {
            if (image.canvas == null)
            {
                throw new InvalidOperationException("ScrollBackgroundView: image は Canvas 配下に置いてください。");
            }

            if (image.canvas.rootCanvas.transform is not RectTransform rootCanvasRectTransform)
            {
                throw new InvalidOperationException("ScrollBackgroundView: rootCanvas の RectTransform を取得できません。");
            }

            return rootCanvasRectTransform;
        }

        static Vector2 GetRootCanvasSize(RectTransform rootCanvasRectTransform)
        {
            var size = rootCanvasRectTransform.rect.size;
            if (size.x <= 0f || size.y <= 0f)
            {
                throw new InvalidOperationException("ScrollBackgroundView: rootCanvas の Rect サイズが 0 です。");
            }

            return size;
        }

        Vector2 ProjectViewSizeToTileAxes(Vector2 viewSize)
        {
            if (basisXUnit.sqrMagnitude <= 0f || basisYUnit.sqrMagnitude <= 0f)
            {
                throw new InvalidOperationException("ScrollBackgroundView: 回転軸ベクトルを計算できません。");
            }

            var axisX = basisXUnit.normalized;
            var axisY = basisYUnit.normalized;

            var neededWidthOnTileX = (Mathf.Abs(axisX.x) * viewSize.x) + (Mathf.Abs(axisX.y) * viewSize.y);
            var neededWidthOnTileY = (Mathf.Abs(axisY.x) * viewSize.x) + (Mathf.Abs(axisY.y) * viewSize.y);
            return new Vector2(neededWidthOnTileX, neededWidthOnTileY);
        }

        void ApplyTilePositions()
        {
            for (var i = 0; i < tiles.Count; i++)
            {
                tiles[i].rectTransform.anchoredPosition = baseAnchoredPosition + tileOffsets[i] + scrollOffset;
            }
        }

        Vector2 GetMoveDirection()
        {
            var radians = angleDegrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        static float WrapOffset(float value, float length)
        {
            return Mathf.Repeat(value + length, length * 2f) - length;
        }

        void EnsureReferences()
        {
            imageRectTransform = image.rectTransform;
            rootCanvasRectTransform = GetRootCanvasRectTransform();
        }

        void OnDisable()
        {
            if (Application.isPlaying)
            {
                return;
            }

            ClearGeneratedTiles();
            tiles.Clear();
            tileOffsets.Clear();
        }

        void ClearGeneratedTiles()
        {
            for (var i = 0; i < generatedTiles.Count; i++)
            {
                if (Application.isPlaying)
                {
                    Destroy(generatedTiles[i].gameObject);
                }
                else
                {
                    DestroyImmediate(generatedTiles[i].gameObject);
                }
            }

            generatedTiles.Clear();
        }
    }
}
