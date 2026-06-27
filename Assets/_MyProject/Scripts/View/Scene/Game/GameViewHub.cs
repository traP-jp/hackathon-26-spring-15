using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace MyProject.View
{
    [RequireComponent(typeof(ViewAnimationTimeline))]
    public class GameViewHub : SceneViewHubBase
    {
        public Observable<Unit> Quit => gameActionsObserver.Quit;
        public Observable<int> PlayerDamaged => player.Damaged;

        [SerializeField] PlayerView player;
        [SerializeField] GimmickSpawner gimmickSpawner;

        GameActionsObserver gameActionsObserver;
        ViewAnimationTimeline animationTimeline;

        public override void Initialize()
        {
            gameActionsObserver ??= new GameActionsObserver();
            animationTimeline = GetComponent<ViewAnimationTimeline>();

            gameActionsObserver.Disable();
            gimmickSpawner.ResetState();
            animationTimeline.Initialize();
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await animationTimeline.ShowAsync(ct);
            gameActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            player.SetInputEnabled(false);
            gimmickSpawner.StopSpawn();
            gameActionsObserver.Disable();
            await animationTimeline.HideAsync(ct);
            gameObject.SetActive(false);
        }

        public async UniTask ShowStartGameAsync(CancellationToken ct)
        {
            player.SetInputEnabled(true);
            gimmickSpawner.StartSpawn();
            await UniTask.CompletedTask;
        }

        public async UniTask ShowFinishGameAsync(CancellationToken ct)
        {
            player.SetInputEnabled(false);
            gimmickSpawner.StopSpawn();
            await UniTask.CompletedTask;
        }

        void OnDestroy()
        {
            gameActionsObserver.Dispose();
        }
    }
}
