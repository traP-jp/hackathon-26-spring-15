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

        [SerializeField] private Transform player_transform;
        [SerializeField] private GimmickView wall_prefab;
        [SerializeField, Min(1)] private int gimmickCountPerPhase = 5;
        [SerializeField, Min(1)] private int wall_distant = 5;
        readonly List<GimmickView> walls = new();
        readonly Subject<Unit> gimmickCleared = new();
        readonly Subject<Unit> phaseCompleted = new();
        int spawnedGimmickCount;
        int passedGimmickCount;
        float nextSpawnTriggerX;
        bool isSpawning;
        bool isPhaseCompleted;

        public override void Initialize()
        {
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
            nextSpawnTriggerX = player_transform.position.x;
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
                if (wall.TryPass(player_transform.position.x, out var cleared))
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

            if (spawnedGimmickCount >= gimmickCountPerPhase || player_transform.position.x < nextSpawnTriggerX)
            {
                return;
            }

            SpawnGimmick();
        }

        void SpawnGimmick()
        {
            Vector3 spawnPosition = new Vector3(player_transform.position.x + 15, 0, 0);
            GimmickView newWall = Instantiate(wall_prefab, spawnPosition, Quaternion.identity, transform);
            newWall.Initialize();
            newWall.Show();
            walls.Add(newWall);

            if (walls.Count > 10)
            {
                Destroy(walls[0].gameObject);
                walls.RemoveAt(0);
            }

            spawnedGimmickCount += 1;
            nextSpawnTriggerX += wall_distant;
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
