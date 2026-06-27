---
name: layer-composition-root
description: CompositionRoot層を実装するためのskill
---

# Composition Root Layer

## 目的

依存解決と起動順序の責務を一箇所に集約し、全層を疎結合のまま組み立てる。

## CompositionRootの本質

CompositionRootはDIコンテナを使って依存性の解決を行い、EntryPointのライフサイクルを起動する層である。設計上は特権的に全層へ依存できるが、責務は「組み立て」に限定する。ドメインロジックや表示ロジックを置く場所ではなく、参照解決と登録ミスの防止を担う。

## ルール

- `MonoBehaviour` は基本的に継承する。
- `CompositionRoot` は全層へ依存してよいが、依存登録と起動制御以外のロジックは実装しない。
- 登録メソッドは責務単位で分割する。基本的には層ごとに分ける。
- EntryPointは `RegisterEntryPoint<T>` で登録する。EntryPointは基本的に1つのみとする。
- Port実装は `.As<I...>()` で契約に紐づけ、具象依存を呼び出し側へ漏らさない。
- Lifetimeは必要が無ければ基本的には `Singleton` を使う。
