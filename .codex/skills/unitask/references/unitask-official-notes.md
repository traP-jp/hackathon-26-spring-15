# UniTask Official Notes

UniTask公式READMEとUnity公式Scripting APIから、実装判断に直結する事項のみを要約する。

## 1. キャンセル伝播を最優先にする

- UniTask公式は、`CancellationToken` をメソッド末尾引数で受け、ルートから末端まで渡すことを推奨している。
- `WithCancellation(ct)` / `ToUniTask(..., cancellationToken: ct)` / `UniTask.Delay(..., cancellationToken: ct)` のように、待機点へ明示的に渡す。
- キャンセル検知時は `OperationCanceledException` が上流へ伝播する前提で設計する。

## 2. Fire-and-Forgetは境界で限定する

- 公式は `async void` 非推奨。
- Fire-and-Forget用途は `UniTaskVoid` か `.Forget()` を使う。
- 未処理例外は `UniTaskScheduler.UnobservedTaskException` に流れるため、どこで捨てるかを意識して使う。

## 3. Timeoutはトークンベースで扱う

- 公式は `.Timeout` より、`CancellationToken` を渡す方式を推奨している。
- UniTaskでは `CancelAfterSlim` や `TimeoutController` が提示されている。
- 複数キャンセル要因は `CancellationTokenSource.CreateLinkedTokenSource(...)` で統合できる。

## 4. Unityライフサイクルトークンを使い分ける

- Unity公式Scripting APIには `MonoBehaviour.destroyCancellationToken` があり、破棄時にキャンセルされる。
- `Application.exitCancellationToken` はEditor再生終了/アプリ終了に連動する。
- 破棄直前参照の注意点があるため、必要な場合は事前にトークンを保持して使う。

## 5. 実装時の注意点

- `UniTask` は `ValueTask` 相当の制約があり、多重awaitを避ける。
- `cancelImmediately: true` はコスト増のため、即時性が必要な箇所に限定する。
- `OperationCanceledException` と通常例外を同列に握りつぶさない。

## 参照URL

- https://github.com/Cysharp/UniTask
- https://raw.githubusercontent.com/Cysharp/UniTask/2.5.10/README.md
- https://docs.unity3d.com/ScriptReference/MonoBehaviour-destroyCancellationToken.html
- https://docs.unity3d.com/ScriptReference/Application-exitCancellationToken.html

