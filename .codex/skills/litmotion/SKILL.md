---
name: litmotion
description: LitMotionを使ってアニメーションを実装するためのskill
---

# LitMotion Animation

## 目的

アニメーションを、LitMotionの公式思想に沿ってシンプルに実装する。

## 公式思想に沿った基本原則

- エントリーポイントは `LMotion.Create(...)` に統一する。
- 設定は `With...`、適用は `Bind...` で構成し、読みやすさを優先する。
- `Bind` の戻り値 `MotionHandle` を必ず保持し、完了/キャンセルを制御する。
- 非同期待機は `await handle` か `ToUniTask(ct)` を使い、`CancellationToken` を渡す。
- パラメータ管理は `SerializableMotionSettings<TValue, TOptions>` でInspectorに出すことができるが、不必要な項目まで表示することになるため基本的には避け、必要な場合のみ使用する。

## 実装フロー

1. 設定を置く  
    - 調整が必要な値だけ Inspector に公開し、不要項目は公開しない。
2. 再生を実装する  
    - `LMotion.Create(settings) -> Bind -> AddTo(this) -> ToUniTask(ct)` を基本形にする。
3. 複数モーションを組む  
    - 同時は `UniTask.WhenAll`、順次は `LSequence` を使う。

## ベストプラクティス

- `AddTo(this)` でライフサイクルと連動させ、破棄済みオブジェクトの再生を防ぐ。
- 再入しうる再生メソッドでは、既存 `MotionHandle` を `TryCancel()` してから再作成する。
- `Scheduler` や `CancelBehavior` は必要になったときだけ明示する。
- ループやディレイを増やす前に、まず最小構成で意図を満たす。
- Inspector公開は最小限にし、調整しない値はコード側に閉じる。

## 禁止/注意

- `LSequence` に再生中Handleや無限ループMotionを追加しない。
- `CancellationToken` を待機に渡さない実装を追加しない。

## 最小例

```csharp
var handle = LMotion.Create(settings)
    .BindToLocalScaleXYZ(transform)
    .AddTo(this);
await handle.ToUniTask(ct);
```

## 参照

- https://annulusgames.github.io/LitMotion/articles/ja/overview.html
- ローカル参照  
    - `references/litmotion-settings-pattern.md`
    - `references/litmotion-advanced-features.md`
