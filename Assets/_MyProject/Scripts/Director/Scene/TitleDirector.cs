using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.View;
using MyProject.Model;
using R3;

namespace MyProject.Director
{
    public class TitleDirector : ISceneDirector, IDisposable
    {
        public Observable<SceneType> SceneChangeRequest => sceneChangeRequest;
        readonly Subject<SceneType> sceneChangeRequest = new();

        public Observable<Unit> SceneReloadRequest => sceneReloadRequest;
        readonly Subject<Unit> sceneReloadRequest = new();

        readonly TitleViewHub titleViewHub;

        readonly CompositeDisposable disposables = new();

        public TitleDirector(TitleViewHub titleViewHub)
        {
            this.titleViewHub = titleViewHub;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            titleViewHub.Initialize();
            await UniTask.CompletedTask;
        }

        public async UniTask BeforeEnterAsync(CancellationToken ct)
        {
            disposables.Clear();
            await UniTask.CompletedTask;
        }

        public async UniTask EnterAsync(CancellationToken ct)
        {
            await titleViewHub.ShowAsync(ct);
        }

        public async UniTask AfterEnterAsync(CancellationToken ct)
        {
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
            await titleViewHub.HideAsync(ct);
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
