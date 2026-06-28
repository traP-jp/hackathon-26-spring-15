using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.View;
using MyProject.Model;
using R3;

namespace MyProject.Director
{
    public class ResultDirector : ISceneDirector, IDisposable
    {
        public Observable<SceneType> SceneChangeRequest => sceneChangeRequest;
        readonly Subject<SceneType> sceneChangeRequest = new();

        public Observable<Unit> SceneReloadRequest => sceneReloadRequest;
        readonly Subject<Unit> sceneReloadRequest = new();

        readonly ResultViewHub resultViewHub;
        readonly GameSessionModel gameSessionModel;

        readonly CompositeDisposable disposables = new();

        public ResultDirector(ResultViewHub resultViewHub, GameSessionModel gameSessionModel)
        {
            this.resultViewHub = resultViewHub;
            this.gameSessionModel = gameSessionModel;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            resultViewHub.Initialize();
            await UniTask.CompletedTask;
        }

        public async UniTask BeforeEnterAsync(CancellationToken ct)
        {
            disposables.Clear();
            gameSessionModel.Score
                .Take(1)
                .Subscribe(resultViewHub.SetScore)
                .AddTo(disposables);

            await UniTask.CompletedTask;
        }

        public async UniTask EnterAsync(CancellationToken ct)
        {
            await resultViewHub.ShowAsync(ct);
        }

        public async UniTask AfterEnterAsync(CancellationToken ct)
        {
            resultViewHub.Retry
                .Take(1)
                .Subscribe(_ => sceneChangeRequest.OnNext(SceneType.Game))
                .AddTo(disposables);

            resultViewHub.Quit
                .Take(1)
                .Subscribe(_ => sceneChangeRequest.OnNext(SceneType.Title))
                .AddTo(disposables);

            await UniTask.CompletedTask;
        }

        public void Tick()
        {
        }

        public async UniTask BeforeExitAsync(CancellationToken ct)
        {
            disposables.Clear();
            await UniTask.CompletedTask;
        }

        public async UniTask ExitAsync(CancellationToken ct)
        {
            await resultViewHub.HideAsync(ct);
        }

        public void Dispose()
        {
            disposables.Dispose();
            sceneChangeRequest.OnCompleted();
            sceneChangeRequest.Dispose();
            sceneReloadRequest.OnCompleted();
            sceneReloadRequest.Dispose();
        }
    }
}
