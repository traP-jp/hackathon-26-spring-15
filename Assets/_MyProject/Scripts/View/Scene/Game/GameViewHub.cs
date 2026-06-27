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

        [SerializeField] PlayerView player;

        GameActionsObserver gameActionsObserver;
        ViewAnimationTimeline animationTimeline;

        public override void Initialize()
        {
            gameActionsObserver ??= new GameActionsObserver();
            animationTimeline = GetComponent<ViewAnimationTimeline>();

            gameActionsObserver.Disable();
            player.SetInputEnabled(false);
            animationTimeline.Initialize();
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await animationTimeline.ShowAsync(ct);
            gameActionsObserver.Enable();
            player.SetInputEnabled(true);
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            player.SetInputEnabled(false);
            gameActionsObserver.Disable();
            await animationTimeline.HideAsync(ct);
            gameObject.SetActive(false);
        }

        public async UniTask ShowStartGameAsync(CancellationToken ct)
        {
            await UniTask.CompletedTask;
        }

        public async UniTask ShowFinishGameAsync(CancellationToken ct)
        {
            await UniTask.CompletedTask;
        }

        void OnDestroy()
        {
            gameActionsObserver.Dispose();
        }
    }
}
