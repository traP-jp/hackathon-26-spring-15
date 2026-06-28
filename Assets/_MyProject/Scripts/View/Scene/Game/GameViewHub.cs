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
        public Observable<Unit> GimmickCleared => gimmickSpawner.GimmickCleared;
        public Observable<Unit> PhaseCompleted => gimmickSpawner.PhaseCompleted;

        [SerializeField] PlayerView player;
        [SerializeField] GimmickSpawner gimmickSpawner;
        [SerializeField] HealthView healthView;
        [SerializeField] ScoreView scoreView;

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
            await UniTask.WhenAll(
                animationTimeline.ShowAsync(ct),
                healthView != null ? healthView.ShowAsync(ct) : UniTask.CompletedTask,
                scoreView != null ? scoreView.ShowAsync(ct) : UniTask.CompletedTask
            );
            gameActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            player.SetInputEnabled(false);
            gimmickSpawner.StopSpawn();
            gameActionsObserver.Disable();
            await UniTask.WhenAll(
                healthView != null ? healthView.HideAsync(ct) : UniTask.CompletedTask,
                scoreView != null ? scoreView.HideAsync(ct) : UniTask.CompletedTask,
                animationTimeline.HideAsync(ct)
            );
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
            player.PlayDeathSe();
            await UniTask.CompletedTask;
        }

        public void SetHealth(int health)
        {
            healthView?.SetHealth(health);
        }

        public void SetScore(int score)
        {
            scoreView?.SetScore(score);
        }

        public void SetPhase(int phase)
        {
            player.SetPhase(phase);
            gimmickSpawner.BeginPhase();
        }

        void OnDestroy()
        {
            gameActionsObserver.Dispose();
        }
    }
}
