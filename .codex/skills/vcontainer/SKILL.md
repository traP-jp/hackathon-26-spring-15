---
name: vcontainer
description: VContainerを使ってUnityでDI登録、EntryPoint起動、LifetimeScopeのスコープ管理を実装・整理するためのskill
---

# VContainer DI

## 目的

VContainerの公式思想に沿って、依存登録・解決・ライフサイクルをシンプルかつ安全に実装する。

## 公式思想に沿った基本原則

- 依存注入はコンストラクタ注入を第一選択にし、依存は `readonly` フィールドで保持する。
- `MonoBehaviour` への注入は自動ではない前提で設計し、`RegisterComponent*` / Inspector指定 / `IObjectResolver.Instantiate` を使い分ける。
- EntryPointは `RegisterEntryPoint<T>()` で登録し、PlayerLoop実行（`IStartable` / `ITickable` / `IAsyncStartable` など）に寄せる。
- Lifetimeは `Singleton` / `Scoped` / `Transient` の意味を明確にして選択する。
- 直接 `Resolve` は最小限にし、基本はコンストラクタ注入で依存を渡す。

## 実装フロー

1. Scopeを決める
   - `LifetimeScope` を起点に、どこまでを `Singleton` / `Scoped` にするか先に決める。
2. Plain C#型を登録する
   - `builder.Register<T>(Lifetime.X)` を基本に、必要な契約だけ `.As<...>()` で公開する。
3. MonoBehaviourを登録する
   - 既存Scene参照は `RegisterComponent(...)` / `RegisterComponentInHierarchy<T>()`、生成系は `RegisterComponentInNewPrefab` / `RegisterComponentOnNewGameObject` を使う。
4. EntryPointを登録する
   - 起動制御クラスを `RegisterEntryPoint<T>()` で登録し、`IStartable` / `IAsyncStartable` / `ITickable` などで実行タイミングを定義する。
5. 子スコープを使い分ける
   - Additive Sceneや一時コンテキストは `LifetimeScope.EnqueueParent(...)` / `CreateChild(...)` で分離し、不要時は `Dispose()` する。
6. 診断・最適化を必要時だけ適用する
   - まずは通常構成で実装し、必要になったら Diagnostics / Source Generator / Async Build を導入する。

## ベストプラクティス

- 実装の主経路は「コンストラクタ注入 + `Register`」に統一し、`IObjectResolver.Resolve` は例外的ケースに限定する。
- インターフェース公開は必要最小限にし、具象型公開が必要なときだけ `.AsSelf()` を追加する。
- `RegisterInstance` はコンテナ管理外（自動Dispose/自動Method Injectionなし）と理解した上で使う。
- 複数実装の切り替えは `.Keyed(...)` + `[Key(...)]` を使うが、細かい値注入には多用せず、Factory/Provider化を優先する。
- `IAsyncStartable.StartAsync(CancellationToken)` では、受け取った `CancellationToken` を下位処理へ必ず伝播する。
- 例外ハンドリングを差し替えるときは `RegisterEntryPointExceptionHandler(...)` を使い、デフォルトログ抑止を理解して運用する。
- `Enable Diagnostics` は調査時だけ有効化する（性能・GCコストが大きい）。

## 禁止/注意

- `[Inject]` 属性を付けただけで `MonoBehaviour` に自動注入される前提で実装しない。
- 動的生成Prefabを `Object.Instantiate` だけで生成して注入漏れを起こさない（必要なら `IObjectResolver.Instantiate` を使う）。
- `Scoped` な `MonoBehaviour` は `LifetimeScope` 破棄だけで自動Destroyされない点を見落とさない。
- バックグラウンドスレッドで `RegisterComponentInHierarchy()` などUnity依存登録を実行しない。
- `VCONTAINER_PARALLEL_CONTAINER_BUILD` を登録数が少ないケースへ安易に有効化しない（遅くなることがある）。
- constructor複数定義時に `[Inject]` 指定を曖昧にしない（1つだけ明示）。

## 最小例

```csharp
public sealed class MainLifetimeScope : LifetimeScope
{
    [SerializeField] RootViewHub rootViewHub;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<SceneModel>(Lifetime.Singleton);
        builder.RegisterComponent(rootViewHub);
        builder.RegisterEntryPoint<MainEntryPoint>();
    }
}
```

## 参照

- https://vcontainer.hadashikick.jp/
- https://github.com/hadashiA/VContainer
- https://vcontainer.hadashikick.jp/resolving/constructor-injection
- https://vcontainer.hadashikick.jp/resolving/gameobject-injection
- https://vcontainer.hadashikick.jp/resolving/container-api
- https://vcontainer.hadashikick.jp/registering/register-type
- https://vcontainer.hadashikick.jp/registering/register-monobehaviour
- https://vcontainer.hadashikick.jp/registering/register-with-keys
- https://vcontainer.hadashikick.jp/integrations/entrypoint
- https://vcontainer.hadashikick.jp/integrations/unitask
- https://vcontainer.hadashikick.jp/scoping/lifetime-overview
- https://vcontainer.hadashikick.jp/scoping/generate-child-via-scene
- https://vcontainer.hadashikick.jp/scoping/generate-child-with-code-first
- https://vcontainer.hadashikick.jp/diagnostics/diagnostics-window
- https://vcontainer.hadashikick.jp/optimization/source-generator
- https://vcontainer.hadashikick.jp/optimization/async-container-build
- https://vcontainer.hadashikick.jp/optimization/parallel-container-build
- ローカル参照
  - `references/vcontainer-official-notes.md`
