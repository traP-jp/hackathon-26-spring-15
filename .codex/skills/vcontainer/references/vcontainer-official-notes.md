# VContainer Official Notes

VContainer公式ドキュメントと公式リポジトリ（README / website docs）から、実装判断に直結する点だけを要約する。

## 1. 注入方式の優先順位

- 公式推奨はコンストラクタ注入 + `readonly` フィールド。
- コンストラクタのオプション依存は未対応。依存が足りないとBuild/Validate時に例外になる。
- コンストラクタが複数ある場合、`[Inject]` を付けるなら1つだけに限定する。
- `MonoBehaviour` はコンストラクタ注入不可なので、`[Inject]` メソッドや `RegisterComponent*` を使う。

## 2. MonoBehaviour注入は明示的に行う

- `[Inject]` を付けただけで自動注入はされない。
- 公式で示される注入経路は次の3つ。
  - `LifetimeScope` Inspector の auto inject 対象に指定
  - `RegisterComponent*` 系で登録
  - 動的生成時に `IObjectResolver.Instantiate` を使う
- `Inject` を `MonoBehaviour` 単体へ直接呼ぶより、`InjectGameObject` でオーナーGameObject単位に扱う考え方が推奨される。

## 3. 登録APIの重要挙動

- `Register<T>(Lifetime.X)` を基点に、`.As<...>()` / `.AsImplementedInterfaces()` / `.AsSelf()` で公開面を調整する。
- `RegisterEntryPoint<T>()` はPlayerLoop連携のライフサイクル実行（`IStartable` / `ITickable` / `IAsyncStartable` 等）を有効化する。
- `RegisterInstance` は常にSingleton扱いだが、インスタンス自体はコンテナ管理外。
  - 自動Disposeされない
  - 自動Method Injectionされない
- 複数実装の切り替えは `.Keyed(...)` と `[Key(...)]` を対で使う。

## 4. Lifetimeとスコープ破棄

- `Singleton`: 基本的に全コンテナ共通の単一インスタンス。
- `Transient`: Resolveごとに新規生成。
- `Scoped`: `LifetimeScope` ごとに1インスタンス。
- 親子スコープでは、未登録型は親を探索する。子で同型登録があれば子側を使う。
- `LifetimeScope` 破棄時には、登録済み `IDisposable` が `Dispose()` される。
- 注意点として、Sceneが生存したまま `LifetimeScope` だけ破棄しても、`Lifetime.Scoped` 登録した `MonoBehaviour` は自動Destroyされない。

## 5. EntryPoint / UniTask連携

- `RegisterEntryPoint<T>()` で登録した型は、VContainer独自PlayerLoopでスケジュールされる。
- `IAsyncStartable` は `com.cysharp.unitask` 導入時に `UniTask` 戻り値を使える。
- `StartAsync(CancellationToken)` のトークンは、そのEntryPointを登録した `LifetimeScope` の破棄に連動してキャンセルされる。
- EntryPoint内の未捕捉例外は既定では `Debug.LogException`。`RegisterEntryPointExceptionHandler` 登録時は既定ログが抑止される。

## 6. 診断・最適化機能

- Diagnostics Windowは依存グラフと登録元の追跡に有効だが、Enable時は性能/GCコストが大きい。
- Source Generator（Unity 2021.3+）で反射コストを軽減できる。対象外（入れ子クラス/struct等）は反射経由。
- Async Container Buildでは `LifetimeScope.Build()` を別スレッド化できるが、`RegisterComponentInHierarchy()` 等のUnity依存処理はバックグラウンド不可。
- `VCONTAINER_PARALLEL_CONTAINER_BUILD` は登録数が少ないケースでは逆に遅くなる可能性がある。

## 参照URL

- https://github.com/hadashiA/VContainer
- https://raw.githubusercontent.com/hadashiA/VContainer/master/README.md
- https://raw.githubusercontent.com/hadashiA/VContainer/master/website/docs/resolving/constructor-injection.mdx
- https://raw.githubusercontent.com/hadashiA/VContainer/master/website/docs/resolving/method-injection.mdx
- https://raw.githubusercontent.com/hadashiA/VContainer/master/website/docs/resolving/property-field-injection.mdx
- https://raw.githubusercontent.com/hadashiA/VContainer/master/website/docs/resolving/gameobject-injection.mdx
- https://raw.githubusercontent.com/hadashiA/VContainer/master/website/docs/resolving/container-api.mdx
- https://raw.githubusercontent.com/hadashiA/VContainer/master/website/docs/registering/register-type.mdx
- https://raw.githubusercontent.com/hadashiA/VContainer/master/website/docs/registering/register-monobehaviour.mdx
- https://raw.githubusercontent.com/hadashiA/VContainer/master/website/docs/registering/register-callbacks.mdx
- https://raw.githubusercontent.com/hadashiA/VContainer/master/website/docs/registering/register-with-keys.mdx
- https://raw.githubusercontent.com/hadashiA/VContainer/master/website/docs/integrations/entrypoint.mdx
- https://raw.githubusercontent.com/hadashiA/VContainer/master/website/docs/integrations/unitask.mdx
- https://raw.githubusercontent.com/hadashiA/VContainer/master/website/docs/scoping/lifetime-overview.mdx
- https://raw.githubusercontent.com/hadashiA/VContainer/master/website/docs/scoping/generate-child-via-scene.mdx
- https://raw.githubusercontent.com/hadashiA/VContainer/master/website/docs/scoping/generate-child-with-code-first.mdx
- https://raw.githubusercontent.com/hadashiA/VContainer/master/website/docs/scoping/project-root-lifetimescope.mdx
- https://raw.githubusercontent.com/hadashiA/VContainer/master/website/docs/diagnostics/diagnostics-window.mdx
- https://raw.githubusercontent.com/hadashiA/VContainer/master/website/docs/optimization/source-generator.mdx
- https://raw.githubusercontent.com/hadashiA/VContainer/master/website/docs/optimization/async-container-build.mdx
- https://raw.githubusercontent.com/hadashiA/VContainer/master/website/docs/optimization/parallel-container-build.mdx
