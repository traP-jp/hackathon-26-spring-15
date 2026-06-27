# LitMotion Advanced Features

`LMotion.Create(...).Bind...` の基本形以外で、実務で効く機能だけを要約する。

## 1. Sequence で複数モーションを合成

- 主用途: 複数演出の時系列制御（順次/同時/任意時刻挿入）
- 主要API: `LSequence.Create()`, `Append()`, `Join()`, `Insert()`, `AppendInterval()`, `Run()`
- 注意点: 再生中モーションと無限ループモーションは `Sequence` に追加できない（例外）

## 2. MotionHandle の高度制御

- 主用途: 再入制御、デバッグ、手動時間制御
- 主要API:
  - 安全停止: `TryCancel()`, `TryComplete()`, `IsActive()`
  - 状態確認: `IsPlaying()`, `Duration`, `TotalDuration`, `CompletedLoops`
  - 動的制御: `PlaybackSpeed`, `Time`, `Preserve()`
- 注意点: `Preserve()` 後は自動破棄されないため、`Cancel()` か `AddTo()` で明示管理する

## 3. ManualMotionDispatcher で手動更新

- 主用途: 独自ループ/テスト/決定論的更新
- 主要API: `new ManualMotionDispatcher()`, `WithScheduler(dispatcher.Scheduler)`, `dispatcher.Update(deltaTime)`
- 注意点: 通常運用では過剰。標準 `MotionScheduler` で困るケースに限定する

## 4. async/await の詳細制御

- 主用途: キャンセル方針を明示した待機
- 主要API: `await handle`, `ToValueTask(...)`, `ToAwaitable(...)`, `ToUniTask(...)`
- 追加設定: `CancelBehavior`, `cancelAwaitOnMotionCanceled`
- 注意点: Unity実運用では公式的にも `UniTask` 連携が第一候補

## 5. カスタム型対応（Custom Adapter）

- 主用途: 独自型の補間・ドメイン固有モーション
- 主要API: `IMotionAdapter<T, TOptions>`, `IMotionOptions`, `LMotion.Create<T, TOptions, TAdapter>(...)`
- 注意点:
  - Adapterは状態を持たない（structで最小実装）
  - Burst対応のため `RegisterGenericJobType` が必要

## 6. Punch / Shake（特殊モーション）

- 主用途: ヒット感・UI反応などの振動演出
- 主要API: `LMotion.Punch.Create(...)`, `LMotion.Shake.Create(...)`
- 追加設定: `WithFrequency(...)`, `WithDampingRatio(...)`, `WithRandomSeed(...)`（Shake）
- 注意点: 第2引数は `endValue` ではなく `strength`

## 7. バインド最適化（GC削減）

- 主用途: 高頻度更新時のGC抑制
- 主要API: `Bind(target, (x, t) => ...)`（状態渡し）
- 注意点: クロージャ付き `Bind(x => ...)` はキャプチャでGCが発生しうる

## 8. AddTo / CompositeMotionHandle で寿命管理

- 主用途: 破棄連動の安全な停止、複数Handleの一括管理
- 主要API: `AddTo(gameObject|component)`, `CompositeMotionHandle`
- 注意点: View再入時の二重再生防止には `MotionHandle` 保持 + `TryCancel()` を併用する

## 9. エディタ再生と LitMotion.Animation

- 主用途: Edit ModeでのプレビューとInspector主体のアニメーション構築
- 関連機能:
  - `EditorApplication.update` ベースのエディタ再生
  - `EditorMotionScheduler.Update`
  - `LitMotion.Animation` パッケージ（Parallel/Sequential, Debugプレビュー）
- 注意点: コード主導運用でも、検証・試作フェーズでは有効