# R3 Official Notes

R3公式READMEとUnity公式パッケージ実装から、Unity実装時に判断が分かれやすい点だけを要約する。

## 1. エラー時に自動停止しない設計

- R3は `OnError` ではなく `OnErrorResume` を採用し、例外発生時も購読は自動解除されない。
- 終了通知は `OnCompleted(Result)` で統一される。
- 例外でストリームを止めたい場合は `OnErrorResumeAsFailure()` を使って明示的に完了扱いへ変換する。

## 2. Subject/ReactivePropertyの終了契約

- `Subject`, `BehaviorSubject`, `ReactiveProperty`, `ReplaySubject`, `ReplayFrameSubject` が用意されている。
- これらは `Dispose()` 時に `OnCompleted` を呼ぶ設計になっている。
- `ReactiveProperty` は `.Value` の同値代入で通知しない。強制通知したい場合は `OnNext(value)` を使う。
- `ReactiveProperty` は `ReadOnlyReactiveProperty` と組み合わせ、private可変 + public readonly公開の形を推奨。

## 3. TimeProvider / FrameProvider中心の時間モデル

- 時間系オペレーターは `IScheduler` ではなく `TimeProvider` を使う。
- フレーム系は `FrameProvider` で扱い、`IntervalFrame` / `DelayFrame` などが提供される。
- Providerを明示しない場合は `ObservableSystem.DefaultTimeProvider` / `DefaultFrameProvider` が使われる。

## 4. Unity統合の重要ポイント

- `UnityProviderInitializer` により、Unity起動時の既定Providerは `UnityTimeProvider.Update` / `UnityFrameProvider.Update` に設定される。
- `AddTo(Component)` は Unity 2022.2+ かつ active な `MonoBehaviour` では `destroyCancellationToken` を利用する。
- 未活性などで `destroyCancellationToken` に頼れないケースは `ObservableDestroyTrigger` を自動付与して破棄監視する。
- `ObserveOnMainThread()` / `SubscribeOnMainThread()` は `UnityFrameProvider.Update` を使用する。
- `UnityEventExtensions` / `UnityUIComponentExtensions` により `AsObservable` / `OnClickAsObservable` などの橋渡しAPIが使える。

## 5. Observable Tracker

- Editor拡張として `Window/Observable Tracker` が提供される。
- 購読リーク追跡には有効だが、Tracking/StackTrace有効化には性能コストがあるため、調査時のみ有効化する。

## 6. ObservableCollections連携

- R3公式READMEでは、コレクション差分通知の用途として `ObservableCollections.R3` の利用が案内されている。
- `ObservableCollections` は `ObservableList<T>`, `ObservableDictionary<TKey, TValue>` などを提供し、`ReadOnlyObservableCollection<T>` を中心に扱う。
- `ObservableCollections.R3` で `ObserveChanged`, `ObserveAdd`, `ObserveRemove`, `ObserveReplace`, `ObserveMove`, `ObserveReset`, `ObserveClear`, `ObserveReverse`, `ObserveSort`, `ObserveCountChanged` を購読できる。
- `CreateView(...)` で同期View（`ISynchronizedView`）を作成でき、Transformは追加時に1回だけ適用される設計。
- View/NotifyCollectionChanged系は内部イベント購読を保持するため、不要になったら `Dispose()` が必要。
- `ObservableList<T>.ToNotifyCollectionChangedSlim()` は高速・省メモリだが、`AddRange` など範囲操作をサポートしない制約がある。

## 参照URL

- https://github.com/Cysharp/R3
- https://github.com/Cysharp/ObservableCollections
- https://raw.githubusercontent.com/Cysharp/R3/main/README.md
- https://raw.githubusercontent.com/Cysharp/ObservableCollections/master/README.md
- https://raw.githubusercontent.com/Cysharp/R3/main/src/R3.Unity/Assets/R3.Unity/Runtime/UnityProviderInitializer.cs
- https://raw.githubusercontent.com/Cysharp/R3/main/src/R3.Unity/Assets/R3.Unity/Runtime/MonoBehaviourExtensions.cs
- https://raw.githubusercontent.com/Cysharp/R3/main/src/R3.Unity/Assets/R3.Unity/Runtime/ObserveOnExtensions.cs
- https://raw.githubusercontent.com/Cysharp/R3/main/src/R3.Unity/Assets/R3.Unity/Runtime/UnityEventExtensions.cs
- https://raw.githubusercontent.com/Cysharp/R3/main/src/R3.Unity/Assets/R3.Unity/Runtime/UnityUIComponentExtensions.cs
- https://raw.githubusercontent.com/Cysharp/R3/main/src/R3.Unity/Assets/R3.Unity/Editor/ObservableTrackerWindow.cs
