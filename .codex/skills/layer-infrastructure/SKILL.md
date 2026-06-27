---
name: layer-infrastructure
description: Infrastructure層を実装するためのskill
---

# Infrastructure Layer

## 目的

Modelで定義したインターフェースを実装し、保存・通信などの技術的関心をドメインから隔離する。

## Infrastructureの本質

Infrastructureは技術詳細を担当する層であり、Modelの抽象境界を具体実装へ変換する。責務は「外部I/Oを実行すること」であって、ドメインルールを決めることではない。呼び出し側にライブラリ固有の都合や保存形式の都合を漏らさず、技術変更の影響をこの層で閉じる。

## ルール

- `MonoBehaviour` は継承しない
- 他の層への依存は `Model` に限定する。
- 実装クラスはModelのインターフェースを満たし、呼び出し側へ技術都合を漏らさない。
- ドメイン判断は行わず、必要な変換とI/O実行に責務を限定する。
