---
name: r3
description: R3を使ってUnityでリアクティブな状態管理とイベント通知を実装するためのskill
---

# R3 Reactive

## 目的

R3の公式思想に沿って、状態公開とイベント通知をシンプルに実装し、購読リークを防ぐ。

## 公式思想に沿った基本原則

- 可変状態は `ReactiveProperty<T>` を private フィールドとして保持し、外部公開は `ReadOnlyReactiveProperty<T>` に限定する。
- イベント通知は `Subject<T>` を使い、公開面は `Observable<T>` だけを返す。
- 可変コレクションは `ObservableList<T>` などを内部で管理し、公開は `ReadOnlyObservableCollection<T>` で行う。
- `Subscribe(...)` の戻り値は必ず `CompositeDisposable` か `AddTo(this|disposables)` で寿命管理する。
- `Subject` / `ReactiveProperty` は破棄時に `OnCompleted` を流す前提で設計し、明示的に `Dispose` する。

## 実装フロー

1. 状態を定義する
   - `ReactiveProperty<T>` を内部状態にし、公開プロパティは `ReadOnlyReactiveProperty<T>` で出す。
2. イベントを定義する
   - Unityイベントを `Subject<T>` へ橋渡しし、`OnNext` で発行する。
3. Collectionの差分通知を定義する
   - `ObserveAdd` / `ObserveRemove` など、必要な差分イベントだけ購読する。
4. 購読する
   - イベントや状態を `Subscribe` し、`AddTo(disposables)` で束ねる。
5. ライフサイクルで解放する
   - 再初期化前は `disposables.Clear()`、終了時は `Dispose()` する。
6. 単発イベントを明示する
   - 一度きりの入力は `Take(1)` を付け、意図しない多重実行を防ぐ。

## ベストプラクティス

- `ReactiveProperty.Value` は同値代入で通知されない前提で設計し、同値通知が必要なときだけ `OnNext(value)` を使う。
- 複数箇所で書き換えたくない状態は `ReadOnlyReactiveProperty<T>` で公開し、書き込み経路を1箇所に固定する。
- `MainThread` へ戻す必要があるストリームは `ObserveOnMainThread()` を使う。
- uGUIのイベントは `OnClickAsObservable()` / `OnValueChangedAsObservable()` など拡張メソッドの利用を優先する。
- コレクション監視は `ObserveChanged` より `ObserveAdd` / `ObserveRemove` など用途別イベントを優先し、不要な分岐を減らす。
- `CreateView` / `ToNotifyCollectionChanged` / `ToNotifyCollectionChangedSlim` は内部イベント購読を持つため、必ず `Dispose()` する。
- リーク調査時は `Window/Observable Tracker` を使い、調査後は Tracking/StackTrace を無効化する。

## 禁止/注意

- `ReactiveProperty<T>` や `Subject<T>` を public フィールドで直接公開しない。
- `ObservableList<T>` など可変コレクションを mutable な型のまま public 公開しない。
- `Subscribe` したまま破棄経路を持たない実装を追加しない。
- `Subject` の破棄を忘れない（`OnCompleted` + `Dispose`）。
- 常時有効化された `Observable Tracker` を本番運用前提で残さない（性能影響がある）。
- `ReactiveProperty` はスレッドセーフではないため、並行アクセス前提の用途にそのまま使わない。

## 最小例

```csharp
public class ScoreModel : IDisposable
{
    public ReadOnlyReactiveProperty<int> Value => value;
    readonly ReactiveProperty<int> value = new(0);

    public void Add(int amount) => value.Value += amount;
    public void Dispose() => value.Dispose();
}

readonly CompositeDisposable disposables = new();

buttonView.Clicked
    .Take(1)
    .Subscribe(_ => sceneModel.RequestSceneChange(SceneType.Select))
    .AddTo(disposables);
```

## 参照

- https://github.com/Cysharp/R3
- https://github.com/Cysharp/ObservableCollections
- https://raw.githubusercontent.com/Cysharp/R3/main/README.md
- https://raw.githubusercontent.com/Cysharp/ObservableCollections/master/README.md
- https://raw.githubusercontent.com/Cysharp/R3/main/src/R3.Unity/Assets/R3.Unity/Runtime/UnityProviderInitializer.cs
- https://raw.githubusercontent.com/Cysharp/R3/main/src/R3.Unity/Assets/R3.Unity/Runtime/MonoBehaviourExtensions.cs
- https://raw.githubusercontent.com/Cysharp/R3/main/src/R3.Unity/Assets/R3.Unity/Runtime/ObserveOnExtensions.cs
- https://raw.githubusercontent.com/Cysharp/R3/main/src/R3.Unity/Assets/R3.Unity/Editor/ObservableTrackerWindow.cs
- ローカル参照
  - `references/r3-official-notes.md`
  - `references/r3-project-usage.md`
