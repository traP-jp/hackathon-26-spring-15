---
name: unitask
description: UniTaskを使ってUnityの非同期処理を実装・整理するためのskill
---

# UniTask Async

## 目的

UniTaskの公式思想に沿って、非同期処理をシンプルかつ安全に実装する。

## 公式思想に沿った基本原則

- 非同期メソッドの戻り値は `UniTask` / `UniTask<T>` を基本とする。
- `CancellationToken` は末尾引数で受け取り、ルートから末端まで必ず伝播する。
- Fire-and-Forget は `Forget()` を使う。
- 例外処理ではキャンセル（`OperationCanceledException`）と障害（それ以外）を分離して扱う。
- タイムアウトは `CancellationToken` ベースで制御し、外側から結果を捨てるだけの実装を避ける。

## 実装フロー

1. ルートトークンを決める
   - EntryPointやMonoBehaviourのライフサイクルに紐づく `CancellationToken` を起点にする。
2. シグネチャを揃える
   - 非同期メソッドは `FooAsync(..., CancellationToken ct)` 形式に統一する。
3. 待機点へトークンを渡す
   - `Delay` / `ToUniTask` / `WithCancellation` / 下位 `Async` 呼び出しへ `ct` を渡す。
4. 並列実行を組む
   - 同時実行は `UniTask.WhenAll(...)`、順次実行は素直に `await` で連結する。
5. Fire-and-Forget境界を限定する
   - イベント購読など「呼び元でawaitできない場所」のみ `Forget()` を許可する。

## ベストプラクティス

- 即時完了パスは `UniTask.CompletedTask` / `UniTask.FromResult(...)` を使う。
- 同期的に中断すべき処理の入口で `ct.ThrowIfCancellationRequested()` を呼ぶ。
- タイムアウトと手動キャンセルを併用する場合は `CancellationTokenSource.CreateLinkedTokenSource(...)` を使う。
- `UniTask` は多重awaitできない前提で設計し、使い回しが必要なら別手段（再生成/専用API）を使う。

## 禁止/注意

- `async void` を新規追加しない。
- `CancellationToken` を受け取るメソッドで、下位呼び出しにトークンを渡し漏らさない。
- 理由がない限り有効な `CancellationToken` を渡すようにする（例: `CancellationToken.None` を引数にするのは避ける）。
- `Forget()` を乱用しない。完了監視不要の境界用途に限定する。
- `Timeout` / `TimeoutWithoutException` を第一選択にしない（まずはトークン伝播で中断可能にする）。

## 最小例

```csharp
public async UniTask LoadAndShowAsync(CancellationToken ct)
{
    ct.ThrowIfCancellationRequested();

    var (a, b) = await UniTask.WhenAll(
        LoadAAsync(ct),
        LoadBAsync(ct));

    await ShowAsync(a, b, ct);
}
```

## 参照

- https://github.com/Cysharp/UniTask
- https://docs.unity3d.com/ScriptReference/MonoBehaviour-destroyCancellationToken.html
- https://docs.unity3d.com/ScriptReference/Application-exitCancellationToken.html
- ローカル参照
  - `references/unitask-official-notes.md`
  - `references/unitask-project-usage.md`

