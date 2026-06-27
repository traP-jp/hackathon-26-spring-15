---
name: layer-director
description: Director層を実装するためのskill
---

# Director Layer

## 目的

DirectorをPresenter相当の制御層として扱い、ViewとModelの橋渡しに責務を限定した実装を行う。

## Directorの本質

DirectorはMVRPにおけるPresenterに相当し、イベントと状態遷移のオーケストレーションを担当する。しかし、Presenterとするには担当する責務が広いため、Directorという名前になっている。Viewから通知されたイベントをModelの操作へ変換し、Modelの状態変化をViewの表示更新へ接続する。具体的な処理は担当せず、順序制御に集中する。シーン全体の起動とライフサイクルは、Sceneにひとつ置かれるEntryPointへ集約する。

## ルール

- `MonoBehaviour` は継承しない
- 他の層への依存は `View` と `Model` のみ許可する。
- 主責務は「順序制御」と「層間接続」に限定し、実処理はドメインロジックや表示ロジックなどへ委譲する。
- EntryPointを起点に初期化順序を固定し、シーンごとの責務を分離する。
