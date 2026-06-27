using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.AddressableAssets;
using UnityEngine;

namespace MyProject.Infrastructure
{
    public static class AddressableLoader
    {
        /// <summary>
        /// 指定ラベルのAddressableアセットを全てロードします。
        /// </summary>
        /// <typeparam name="T">ロード対象の型。</typeparam>
        /// <param name="label">ロード対象のラベル。</param>
        /// <param name="ct">キャンセル用トークン。</param>
        /// <returns>ロードされたアセットのリスト。</returns>
        public static async UniTask<List<T>> LoadAllByLabelAsync<T>(AssetLabelReference label, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            Debug.Log($"[AddressableLoader] Start loading assets with label: {label}");

            var assets = await Addressables.LoadAssetsAsync<T>(label, asset =>
            {
                Debug.Log($"[AddressableLoader] Loaded ({typeof(T).Name}): {asset}");
            }).ToUniTask(cancellationToken: ct);

            Debug.Log($"[AddressableLoader] Finished loading assets with label: {label}. Total loaded: {assets.Count}");

            return new List<T>(assets);
        }

        /// <summary>
        /// 指定したAddressableアセットを1件ロードします。
        /// </summary>
        /// <typeparam name="T">ロード対象の型。</typeparam>
        /// <param name="reference">ロード対象アセット参照。</param>
        /// <param name="ct">キャンセル用トークン。</param>
        /// <returns>ロードされたアセット。</returns>
        public static async UniTask<T> LoadAsync<T>(AssetReferenceT<T> reference, CancellationToken ct)
            where T : Object
        {
            ct.ThrowIfCancellationRequested();

            Debug.Log($"[AddressableLoader] Start loading asset: {reference}");

            var asset = await reference.LoadAssetAsync().ToUniTask(cancellationToken: ct);

            Debug.Log($"[AddressableLoader] Loaded ({typeof(T).Name}): {asset}");

            return asset;
        }

        /// <summary>
        /// 指定したPrefab参照をインスタンス化します。
        /// </summary>
        /// <param name="reference">インスタンス化するPrefab参照。</param>
        /// <param name="parent">生成先の親Transform。</param>
        /// <param name="ct">キャンセル用トークン。</param>
        /// <returns>生成されたGameObjectインスタンス。</returns>
        public static async UniTask<GameObject> InstantiateAsync
        (
            AssetReferenceGameObject reference,
            Transform parent,
            CancellationToken ct
        )
        {
            ct.ThrowIfCancellationRequested();

            Debug.Log($"[AddressableLoader] Start instantiating: {reference}");

            var instance = await reference.InstantiateAsync(parent).ToUniTask(cancellationToken: ct);

            Debug.Log($"[AddressableLoader] Instantiated: {instance}");

            return instance;
        }

        /// <summary>
        /// Addressables経由で生成したインスタンスを解放します。
        /// </summary>
        /// <param name="instance">解放対象インスタンス。</param>
        public static void ReleaseInstance(GameObject instance)
        {
            Addressables.ReleaseInstance(instance);
            Debug.Log($"[AddressableLoader] Released instance: {instance}");
        }

        /// <summary>
        /// Addressablesでロードしたアセットを解放します。
        /// </summary>
        /// <typeparam name="T">解放対象アセット型。</typeparam>
        /// <param name="asset">解放対象アセット。</param>
        public static void Release<T>(T asset)
        {
            Addressables.Release(asset);
            Debug.Log($"[AddressableLoader] Released ({typeof(T).Name}): {asset}");
        }

        /// <summary>
        /// Addressablesでロードした複数アセットを一括解放します。
        /// </summary>
        /// <typeparam name="T">解放対象アセット型。</typeparam>
        /// <param name="assets">解放対象アセット列挙。</param>
        public static void ReleaseAll<T>(IEnumerable<T> assets)
        {
            foreach (var asset in assets)
            {
                Release(asset);
            }
        }

        /// <summary>
        /// 指定キーの依存アセットを含むダウンロードサイズを取得します。
        /// </summary>
        /// <param name="key">Addressablesキー。</param>
        /// <param name="ct">キャンセル用トークン。</param>
        /// <returns>ダウンロードサイズ（byte）。</returns>
        public static async UniTask<long> GetDownloadSizeAsync(object key, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return await Addressables.GetDownloadSizeAsync(key).ToUniTask(cancellationToken: ct);
        }

        /// <summary>
        /// 指定キーの依存アセットを事前ダウンロードします。
        /// </summary>
        /// <param name="key">Addressablesキー。</param>
        /// <param name="ct">キャンセル用トークン。</param>
        public static async UniTask DownloadDependenciesAsync(object key, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            Debug.Log($"[AddressableLoader] Start downloading dependencies: {key}");

            await Addressables.DownloadDependenciesAsync(key).ToUniTask(cancellationToken: ct);

            Debug.Log($"[AddressableLoader] Downloaded dependencies: {key}");
        }
    }
}
