using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MyProject.View
{
    public class GimmickSpawner : ViewBase
    {
        [SerializeField] private Transform player_transform;
        [SerializeField] private GimmickView wall_prefab;
        private int wall_number_count = 0;
        [SerializeField] private int wall_distant = 5;
        readonly List<GimmickView> walls = new();
        bool isSpawning;

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
            wall_number_count = 0;

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

            if (player_transform.position.x >= wall_number_count * wall_distant)
            {
                Vector3 spawnPosition = new Vector3(player_transform.position.x + 15, 0, 0);
                GimmickView newWall = Instantiate(wall_prefab, spawnPosition, Quaternion.identity, transform);
                newWall.Initialize();
                newWall.Show();
                walls.Add(newWall);

                if (walls.Count >= 10)
                {
                    Destroy(walls[0].gameObject);
                    walls.RemoveAt(0);
                }

                wall_number_count += 1;
            }
        }
    }
}
