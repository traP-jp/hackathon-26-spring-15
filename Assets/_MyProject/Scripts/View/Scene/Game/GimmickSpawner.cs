using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.Serialization;

namespace MyProject.View
{
    public class GimmickSpawner : ViewBase
    {
        public Observable<Unit> GimmickCleared => gimmickCleared;
        public Observable<Unit> PhaseCompleted => phaseCompleted;

        [SerializeField] Transform playerTransform;
        [SerializeField] Camera targetCamera;
        [SerializeField] List<GimmickView> wallPrefabs = new();
        [SerializeField, Min(1)] int gimmickCountPerPhase = 5;
        [SerializeField, FormerlySerializedAs("spawnAheadDistance"), Min(0f)] float spawnPreloadDistance = 15f;
        [SerializeField, Min(0f)] float offscreenSpawnMargin = 1f;
        [SerializeField, FormerlySerializedAs("wallDistance"), Min(0.1f)] float minGimmickDistance = 5f;
        [SerializeField, Min(0f)] float randomAdditionalDistance = 10f;
        readonly List<GimmickView> walls = new();
        readonly Subject<Unit> gimmickCleared = new();
        readonly Subject<Unit> phaseCompleted = new();
        int passedGimmickCountInPhase;
        float nextSpawnPositionX;
        bool isSpawning;

        float PlayerLocalPositionX => transform.InverseTransformPoint(playerTransform.position).x;

        public override void Initialize()
        {
            targetCamera ??= Camera.main;
            ValidateSettings();
            ValidateWallPrefabs();
            ResetState();
            gameObject.SetActive(false);
        }

        public void StartSpawn()
        {
            isSpawning = true;
        }

        public void StopSpawn()
        {
            isSpawning = false;
        }

        public override void Show()
        {
            ResetState();
            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            ResetState();
            gameObject.SetActive(false);
        }

        public void ResetState()
        {
            StopSpawn();
            passedGimmickCountInPhase = 0;
            ResetNextSpawnPosition();

            foreach (var wall in walls)
            {
                Destroy(wall.gameObject);
            }

            walls.Clear();
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
            if (!isSpawning)
            {
                return;
            }

            foreach (var wall in walls)
            {
                if (wall.TryPass(PlayerLocalPositionX, out var cleared))
                {
                    passedGimmickCountInPhase += 1;

                    if (cleared)
                    {
                        gimmickCleared.OnNext(Unit.Default);
                    }

                    if (passedGimmickCountInPhase >= gimmickCountPerPhase)
                    {
                        passedGimmickCountInPhase = 0;
                        phaseCompleted.OnNext(Unit.Default);
                    }
                }
            }

            var screenRightLocalX = GetScreenRightLocalX();

            if (screenRightLocalX + spawnPreloadDistance < nextSpawnPositionX)
            {
                return;
            }

            SpawnGimmick(nextSpawnPositionX, screenRightLocalX);
        }

        void ResetNextSpawnPosition()
        {
            targetCamera ??= Camera.main;
            nextSpawnPositionX = targetCamera != null
                ? GetScreenRightLocalX() + offscreenSpawnMargin + GetRandomGimmickDistance()
                : 0f;
        }

        void SpawnGimmick(float scheduledPositionX, float screenRightLocalX)
        {
            GimmickView newWall = Instantiate(GetRandomWallPrefab(), transform);
            float halfWidth = GetLocalHalfWidth(newWall);
            float spawnPositionX = Mathf.Max(
                scheduledPositionX,
                screenRightLocalX + offscreenSpawnMargin + halfWidth);

            Vector3 spawnPosition = new Vector3(spawnPositionX, 0, 0);
            newWall.transform.SetLocalPositionAndRotation(spawnPosition, Quaternion.identity);
            newWall.Initialize();
            newWall.Show();
            walls.Add(newWall);

            if (walls.Count > 10)
            {
                Destroy(walls[0].gameObject);
                walls.RemoveAt(0);
            }

            nextSpawnPositionX = spawnPositionX + GetRandomGimmickDistance();
        }

        GimmickView GetRandomWallPrefab()
        {
            return wallPrefabs[UnityEngine.Random.Range(0, wallPrefabs.Count)];
        }

        float GetRandomGimmickDistance()
        {
            return minGimmickDistance + UnityEngine.Random.Range(0f, randomAdditionalDistance);
        }

        float GetScreenRightLocalX()
        {
            float cameraDistance = Mathf.Abs(transform.position.z - targetCamera.transform.position.z);
            Vector3 screenRightWorldPosition = targetCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, cameraDistance));
            return transform.InverseTransformPoint(screenRightWorldPosition).x;
        }

        float GetLocalHalfWidth(GimmickView wall)
        {
            var renderers = wall.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length == 0)
            {
                return 0f;
            }

            var bounds = renderers[0].bounds;

            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            float minLocalX = transform.InverseTransformPoint(bounds.min).x;
            float maxLocalX = transform.InverseTransformPoint(bounds.max).x;
            return Mathf.Abs(maxLocalX - minLocalX) * 0.5f;
        }

        void ValidateSettings()
        {
            if (playerTransform == null)
            {
                throw new InvalidOperationException("GimmickSpawner: PlayerTransform が設定されていません。");
            }

            if (targetCamera == null)
            {
                throw new InvalidOperationException("GimmickSpawner: TargetCamera が設定されていません。");
            }

            if (minGimmickDistance <= 0f)
            {
                throw new InvalidOperationException("GimmickSpawner: MinGimmickDistance は 0 より大きい値にしてください。");
            }
        }

        void ValidateWallPrefabs()
        {
            if (wallPrefabs == null || wallPrefabs.Count == 0)
            {
                throw new InvalidOperationException("GimmickSpawner: WallPrefabs が設定されていません。");
            }

            foreach (var wallPrefab in wallPrefabs)
            {
                if (wallPrefab == null)
                {
                    throw new InvalidOperationException("GimmickSpawner: WallPrefabs に null が含まれています。");
                }
            }
        }

        void OnDestroy()
        {
            gimmickCleared.OnCompleted();
            gimmickCleared.Dispose();
            phaseCompleted.OnCompleted();
            phaseCompleted.Dispose();
        }
    }
}
