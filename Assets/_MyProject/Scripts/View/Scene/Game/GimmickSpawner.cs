using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace MyProject.View
{
    public class GimmickSpawner : ViewBase
    {
        public Observable<Unit> GimmickCleared => gimmickCleared;
        public Observable<Unit> PhaseCompleted => phaseCompleted;

        [SerializeField] Transform playerTransform;
        [SerializeField] List<GimmickView> wallPrefabs = new();
        [SerializeField, Min(1)] int gimmickCountPerPhase = 5;
        [SerializeField, Min(1)] int wallDistance = 5;
        readonly List<GimmickView> walls = new();
        readonly Subject<Unit> gimmickCleared = new();
        readonly Subject<Unit> phaseCompleted = new();
        int spawnedGimmickCount;
        int passedGimmickCount;
        float nextSpawnTriggerX;
        bool isSpawning;
        bool isPhaseCompleted;

        float PlayerLocalPositionX => transform.InverseTransformPoint(playerTransform.position).x;

        public override void Initialize()
        {
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

        public void BeginPhase()
        {
            spawnedGimmickCount = 0;
            passedGimmickCount = 0;
            isPhaseCompleted = false;
            nextSpawnTriggerX = PlayerLocalPositionX;
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
            spawnedGimmickCount = 0;
            passedGimmickCount = 0;
            nextSpawnTriggerX = 0f;
            isPhaseCompleted = false;

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
                    passedGimmickCount += 1;

                    if (cleared)
                    {
                        gimmickCleared.OnNext(Unit.Default);
                    }
                }
            }

            if (!isPhaseCompleted && spawnedGimmickCount >= gimmickCountPerPhase && passedGimmickCount >= gimmickCountPerPhase)
            {
                isPhaseCompleted = true;
                phaseCompleted.OnNext(Unit.Default);
                return;
            }

            if (spawnedGimmickCount >= gimmickCountPerPhase || PlayerLocalPositionX < nextSpawnTriggerX)
            {
                return;
            }

            SpawnGimmick();
        }

        void SpawnGimmick()
        {
            Vector3 spawnPosition = new Vector3(PlayerLocalPositionX + 15, 0, 0);
            GimmickView newWall = Instantiate(GetRandomWallPrefab(), transform);
            newWall.transform.SetLocalPositionAndRotation(spawnPosition, Quaternion.identity);
            newWall.Initialize();
            newWall.Show();
            walls.Add(newWall);

            if (walls.Count > 10)
            {
                Destroy(walls[0].gameObject);
                walls.RemoveAt(0);
            }

            spawnedGimmickCount += 1;
            nextSpawnTriggerX += wallDistance;
        }

        GimmickView GetRandomWallPrefab()
        {
            return wallPrefabs[UnityEngine.Random.Range(0, wallPrefabs.Count)];
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
