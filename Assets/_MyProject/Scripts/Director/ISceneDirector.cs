using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Model;
using R3;

namespace MyProject.Director
{
    public interface ISceneDirector
    {
        /// <summary>
        /// シーンチェンジのリクエストイベント
        /// </summary>
        Observable<SceneType> SceneChangeRequest { get; }

        /// <summary>
        /// シーンリロードのリクエストイベント
        /// </summary>
        Observable<Unit> SceneReloadRequest { get; }

        /// <summary>
        /// シーンの初期化処理を行う。
        /// </summary>
        UniTask InitializeAsync(CancellationToken ct);

        /// <summary>
        /// シーンチェンジでこのシーンに遷移する前に呼び出される処理。
        /// 遷移元のBeforeExitAsyncの後に呼び出される。
        /// </summary>
        UniTask BeforeEnterAsync(CancellationToken ct);

        /// <summary>
        /// シーンチェンジでこのシーンに遷移するときに呼び出される処理。
        /// 遷移元のExitAsyncと同時に呼び出される。
        /// </summary>
        UniTask EnterAsync(CancellationToken ct);

        /// <summary>
        /// シーンチェンジでこのシーンへの遷移が完了した後に呼び出される処理。
        /// 遷移元のExitAsyncと遷移先のEnterAsyncの後に呼び出される。
        /// </summary>
        UniTask AfterEnterAsync(CancellationToken ct);

        /// <summary>
        /// このシーンが選ばれている間、毎フレーム呼び出される処理。
        /// </summary>
        void Tick();

        /// <summary>
        /// シーンチェンジでこのシーンから遷移する前に呼び出される処理。
        /// 遷移先のBeforeEnterAsyncよりも先に呼び出される。
        /// </summary>
        UniTask BeforeExitAsync(CancellationToken ct);
        
        /// <summary>
        /// シーンチェンジでこのシーンから遷移するときに呼び出される処理。
        /// 遷移先のEnterAsyncと同時に呼び出される。
        /// </summary>
        UniTask ExitAsync(CancellationToken ct);
    }
}
