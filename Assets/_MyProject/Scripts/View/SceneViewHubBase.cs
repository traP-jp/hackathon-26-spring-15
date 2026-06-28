using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MyProject.View
{
    public abstract class SceneViewHubBase : MonoBehaviour
    {
        /// <summary>
        /// 初期化処理
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// 表示処理
        /// </summary>
        public abstract UniTask ShowAsync(CancellationToken ct);

        /// <summary>
        /// 非表示処理
        /// </summary>
        public abstract UniTask HideAsync(CancellationToken ct);

        protected void PlaySe(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            AudioPlayerView.Instance?.PlaySe(clip);
        }

    }
}
