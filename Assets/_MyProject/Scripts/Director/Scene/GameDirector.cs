using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.View;
using MyProject.Model;
using R3;

namespace MyProject.Director
{
    public class GameDirector : ISceneDirector, IDisposable
    {
        public Observable<SceneType> SceneChangeRequest => sceneChangeRequest;
        readonly Subject<SceneType> sceneChangeRequest = new();

        public Observable<Unit> SceneReloadRequest => sceneReloadRequest;
        readonly Subject<Unit> sceneReloadRequest = new();

        readonly GameSessionModel gameSessionModel;
        readonly GameViewHub gameViewHub;

        readonly CompositeDisposable disposables = new();
        CancellationTokenSource cts;

        public GameDirector(GameSessionModel gameSessionModel, GameViewHub gameViewHub)
        {
            this.gameSessionModel = gameSessionModel;
            this.gameViewHub = gameViewHub;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            gameSessionModel.Initialize();
            gameViewHub.Initialize();

            await UniTask.CompletedTask;
        }

        public async UniTask BeforeEnterAsync(CancellationToken ct)
        {
            disposables.Clear();
            CreateCts(ct);

            gameSessionModel.Initialize();
            SubscribeModel();

            await UniTask.CompletedTask;
        }

        public async UniTask EnterAsync(CancellationToken ct)
        {
            await gameViewHub.ShowAsync(ct);
        }

        public async UniTask AfterEnterAsync(CancellationToken ct)
        {
            SubscribeView();
            await StartGameAsync(ct);
        }

        public void Tick()
        {
        }

        public async UniTask BeforeExitAsync(CancellationToken ct)
        {
            DisposeCts();
            disposables.Clear();
            await UniTask.CompletedTask;
        }

        public async UniTask ExitAsync(CancellationToken ct)
        {
            await gameViewHub.HideAsync(ct);
        }

        public void Dispose()
        {
            DisposeCts();
            disposables.Dispose();
            sceneChangeRequest.OnCompleted();
            sceneChangeRequest.Dispose();
            sceneReloadRequest.OnCompleted();
            sceneReloadRequest.Dispose();
        }

        void SubscribeModel()
        {
            gameSessionModel.Health
                .Subscribe(gameViewHub.SetHealth)
                .AddTo(disposables);

            gameSessionModel.Score
                .Subscribe(gameViewHub.SetScore)
                .AddTo(disposables);

            gameSessionModel.Finished
                .Take(1)
                .Subscribe(_ => HandleGameFinishedAsync(cts.Token).Forget())
                .AddTo(disposables);
        }

        void SubscribeView()
        {
            gameViewHub.Quit
                .Take(1)
                .Subscribe(_ => sceneChangeRequest.OnNext(SceneType.Title))
                .AddTo(disposables);

            gameViewHub.PlayerDamaged
                .Subscribe(gameSessionModel.TakeDamage)
                .AddTo(disposables);
        }

        async UniTask StartGameAsync(CancellationToken ct)
        {
            gameSessionModel.Start();
            await gameViewHub.ShowStartGameAsync(ct);
        }

        async UniTask HandleGameFinishedAsync(CancellationToken ct)
        {
            await UniTask.WhenAll(
                gameViewHub.ShowFinishGameAsync(ct),
                gameSessionModel.SaveAsync(ct)
            );
            sceneChangeRequest.OnNext(SceneType.Result);
        }

        void CreateCts(CancellationToken ct)
        {
            DisposeCts();
            cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        }

        void DisposeCts()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }
    }
}
