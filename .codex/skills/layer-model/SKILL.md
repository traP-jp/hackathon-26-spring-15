---
name: layer-model
description: Model層を実装・整理するためのskill。Pure C#のドメインロジック、状態遷移、Portインターフェース、ゲームルールを扱うときに使う。
---

# Model Layer

## 目的

Modelをドメインの中心として扱い、ゲームのルールと状態遷移を可能な限り閉じ込める。

## Modelの本質

Modelは従来のDomain層に相当する。ゲームのルールと状態遷移を集約し、`MonoBehaviour` に依存せず、外部I/OやUI都合を持ち込まない。外部システムとの境界が必要な場合はModel内にインターフェースを定義し、実装はInfrastructureへ委譲する。Modelは「何が正しいか」を決める層であり、「どう表示するか」「どう保存するか」は決めない。ただし、Unity都合でModelに閉じ込めづらい処理や、Model層に置くメリットが薄い処理はViewへ置くことを許容する。

## ルール

- `MonoBehaviour` は継承しない。
- 他の層へは依存しない。
- 外部I/Oの境界はインターフェースをModel内に定義し、実装詳細はInfrastructureに委譲する。
- Unity APIや外部ライブラリは必要な範囲でのみ使う。
- ドメイン判断はメソッド内に閉じ込め、呼び出し元へドメインを漏らさない。
