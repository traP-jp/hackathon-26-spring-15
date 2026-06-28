using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace MyProject.View
{
    [RequireComponent(typeof(ViewAnimationTimeline))]
    public class GameViewHub : SceneViewHubBase
    {
        public Observable<Unit> Quit => gameActionsObserver.Quit.Select(_ =>
        {
            PlaySe(quitSeClip);
            return Unit.Default;
        });
        public Observable<int> PlayerDamaged => player.Damaged.Select(damage =>
        {
            postProcessView?.PlayDamage();
            return damage;
        });
        public Observable<Unit> GimmickCleared => gimmickSpawner.GimmickCleared;
        public Observable<Unit> PhaseCompleted => gimmickSpawner.PhaseCompleted;

        [SerializeField] AudioClip quitSeClip;
        [SerializeField] PlayerView player;
        [SerializeField] GimmickSpawner gimmickSpawner;
        [SerializeField] HealthView healthView;
        [SerializeField] ScoreView scoreView;
        [SerializeField] GamePostProcessView postProcessView;

        GameActionsObserver gameActionsObserver;
        ViewAnimationTimeline animationTimeline;
        readonly CompositeDisposable disposables = new();

        public override void Initialize()
        {
            gameActionsObserver ??= new GameActionsObserver();
            animationTimeline = GetComponent<ViewAnimationTimeline>();

            disposables.Clear();
            gameActionsObserver.Disable();
            gimmickSpawner.ResetState();
            animationTimeline.Initialize();
            postProcessView.Initialize();
            player.BoostingChanged
                .Subscribe(postProcessView.SetBoosting)
                .AddTo(disposables);
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
            postProcessView?.ResetState();
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
            postProcessView?.SetHealth(health);
        }

        public void SetScore(int score)
        {
            scoreView?.SetScore(score);
        }

        public void SetPhase(int phase)
        {
            player.SetPhase(phase);
        }

        void OnDestroy()
        {
            disposables.Dispose();
            gameActionsObserver?.Dispose();
        }
    }
}
