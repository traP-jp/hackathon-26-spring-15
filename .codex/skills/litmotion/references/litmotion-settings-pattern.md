# LitMotion Settings Pattern

## 1. 設定の持ち方

- `SerializableMotionSettings<TValue, TOptions>` は「Inspector調整が必要な場合」の手段。
- 固定値で十分なら `LMotion.Create(start, end, duration)` + `With...` の直書きでよい。
- Inspector公開は最小限にし、調整しない項目はコード側に閉じる。

## 2. 再生の基本形

```csharp
var handle = LMotion.Create(settings)
    .BindToLocalScaleXYZ(transform)
    .AddTo(this);
await handle.ToUniTask(ct);
```

- `AddTo(this)` でオブジェクト破棄時にモーションを自動的に管理する。
- `ToUniTask(ct)` で待機とキャンセルを連携する。

## 3. キャンセルと再入

- 再入する可能性がある処理は、同種 `MotionHandle` をフィールドで保持する。
- 新規再生前に `TryCancel()` して、古い再生を停止してから作り直す。
- 挙動が必要な場合だけ `CancelBehavior` と `cancelAwaitOnMotionCanceled` を明示する。

## 4. 複数モーションの組み方

- 同時再生: `UniTask.WhenAll`
- 順次再生: `LSequence`
- `LSequence` に追加するのは「未再生かつ有限ループ」のモーションのみ。

## 5. 最小チェックリスト

- `LMotion.Create` 起点になっているか。
- `Bind` の戻り値を `MotionHandle` として扱っているか。
- `CancellationToken` を待機へ渡しているか。
- 不要な項目までInspector公開していないか。
