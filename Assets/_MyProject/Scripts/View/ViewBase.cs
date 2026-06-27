using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MyProject.View
{
    /// <summary>
    /// Viewの基底クラス。
    /// 全てのViewは基本的にこのクラスを継承する。
    /// </summary>
    public abstract class ViewBase : MonoBehaviour
    {
        /// <summary>
        /// 初期化処理
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// 表示処理
        /// </summary>
        public abstract void Show();

        /// <summary>
        /// 非表示処理
        /// </summary>
        public abstract void Hide();

        /// <summary>
        /// アニメーション表示処理
        /// </summary>
        public abstract UniTask ShowAsync(CancellationToken ct);

        /// <summary>
        /// アニメーション非表示処理
        /// </summary>
        public abstract UniTask HideAsync(CancellationToken ct);
    }
}
