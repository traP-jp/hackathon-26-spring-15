using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Model;
using R3;
using VContainer.Unity;

namespace MyProject.Director
{
    public class MainEntryPoint : IAsyncStartable, ITickable, IDisposable
    {
        readonly RootDirector rootDirector;
        readonly Dictionary<SceneType, ISceneDirector> directors = new();

        readonly SemaphoreSlim sceneChangeSemaphore = new(1, 1);
        readonly CompositeDisposable disposables = new();
        CancellationTokenSource cts;
        SceneType currentScene;

        public MainEntryPoint
        (
            GameConfigSO gameConfig,
            RootDirector rootDirector,
            TitleDirector titleDirector,
            SelectDirector selectDirector,
            GameDirector gameDirector,
            ResultDirector resultDirector
        )
        {
            this.rootDirector = rootDirector;
            currentScene = gameConfig.InitialSceneType;
            directors.Add(SceneType.Title, titleDirector);
            directors.Add(SceneType.Select, selectDirector);
            directors.Add(SceneType.Game, gameDirector);
            directors.Add(SceneType.Result, resultDirector);
        }

        public async UniTask StartAsync(CancellationToken ct)
        {
            cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            await rootDirector.InitializeAsync(cts.Token);
            await ResetSceneAsync(cts.Token);
        }

        public void Tick()
        {
            rootDirector.Tick();

            var director = GetDirector(currentScene);
            director.Tick();
        }

        public void Dispose()
        {
            disposables.Dispose();
            cts?.Cancel();
            cts?.Dispose();
            sceneChangeSemaphore.Dispose();
        }

        async UniTask ResetSceneAsync(CancellationToken ct)
        {
            disposables.Clear();

            foreach (var item in directors.Values)
            {
                await item.InitializeAsync(ct);
            }

            // シーンチェンジリクエストを購読
            var sceneChangeRequests = directors.Values
                .Select(item => item.SceneChangeRequest)
                .ToArray();
            Observable.Merge(sceneChangeRequests)
                .Subscribe(HandleSceneChangeRequest)
                .AddTo(disposables);

            var sceneReloadRequests = directors.Values
                .Select(item => item.SceneReloadRequest)
                .ToArray();
            Observable.Merge(sceneReloadRequests)
                .Subscribe(_ => HandleSceneChangeRequest(currentScene))
                .AddTo(disposables);

            // 初期シーンを起動
            var currentDirector = GetDirector(currentScene);
            await currentDirector.BeforeEnterAsync(ct);
            await currentDirector.EnterAsync(ct);
            await currentDirector.AfterEnterAsync(ct);
        }

        void HandleSceneChangeRequest(SceneType to)
        {
            if (!sceneChangeSemaphore.Wait(0))
            {
                throw new InvalidOperationException("Scene change was requested while another scene change is in progress.");
            }

            var from = currentScene;
            currentScene = to;
            ExecuteSceneTransitionAsync(from, to, cts.Token).Forget();
        }

        async UniTask ExecuteSceneTransitionAsync(SceneType from, SceneType to, CancellationToken ct)
        {
            var fromDirector = GetDirector(from);
            var toDirector = GetDirector(to);

            try
            {
                await fromDirector.BeforeExitAsync(ct);
                await toDirector.BeforeEnterAsync(ct);
                await UniTask.WhenAll
                (
                    fromDirector.ExitAsync(ct),
                    toDirector.EnterAsync(ct)
                );
                await toDirector.AfterEnterAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                // シーンを初期化
                await ResetSceneAsync(ct);

                throw new InvalidOperationException($"Scene transition failed from {from} to {to}. Scene has been reset.", ex);

            }
            finally
            {
                if (sceneChangeSemaphore.CurrentCount == 1)
                {
                    throw new InvalidOperationException("No scene change is in progress.");
                }

                sceneChangeSemaphore.Release();
            }
        }

        ISceneDirector GetDirector(SceneType sceneType)
        {
            if (directors.TryGetValue(sceneType, out var director))
            {
                return director;
            }

            throw new InvalidOperationException($"Director not found for SceneType: {sceneType}");
        }
    }
}
