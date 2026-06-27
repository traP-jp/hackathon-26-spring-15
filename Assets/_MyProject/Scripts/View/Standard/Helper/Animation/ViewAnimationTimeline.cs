using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MyProject.View
{
    /// <summary>
    /// 対象のViewとアニメーション再生タイミングの組の集合から、アニメーションのタイムラインを作成するクラス。
    /// </summary>
    public class ViewAnimationTimeline : ViewBase
    {
        [Serializable]
        class TimedViewAnimation
        {
            [field: SerializeField]
            public ViewBase View { get; private set; }

            [field: SerializeField, Min(0f)]
            public float ShowStartSeconds { get; private set; }

            [field: SerializeField, Min(0f)]
            public float HideStartSeconds { get; private set; }
        }

        [Header("View Timelines")]
        [SerializeField] List<TimedViewAnimation> viewAnimations = new();

        readonly List<TimedViewAnimation> validAnimations = new();

        Func<CancellationToken, UniTask> playShowTimelineAsync = _ => UniTask.CompletedTask;
        Func<CancellationToken, UniTask> playHideTimelineAsync = _ => UniTask.CompletedTask;

        public override void Initialize()
        {
            validAnimations.Clear();

            foreach (var timedAnimation in viewAnimations)
            {
                if (timedAnimation == null)
                {
                    Debug.LogWarning($"[{nameof(ViewAnimationTimeline)}] TimedViewAnimation is null.", this);
                    continue;
                }

                if (timedAnimation.View == null)
                {
                    Debug.LogWarning($"[{nameof(ViewAnimationTimeline)}] View is not assigned.", this);
                    continue;
                }

                timedAnimation.View.Initialize();
                validAnimations.Add(timedAnimation);
            }

            playShowTimelineAsync = BuildTimelineTask(validAnimations, timed => timed.ShowStartSeconds, PlayShowAsync);
            playHideTimelineAsync = BuildTimelineTask(validAnimations, timed => timed.HideStartSeconds, PlayHideAsync);
        }

        public override void Show()
        {
            foreach (var timedAnimation in validAnimations)
            {
                timedAnimation.View.Show();
            }
        }

        public override void Hide()
        {
            foreach (var timedAnimation in validAnimations)
            {
                timedAnimation.View.Hide();
            }
        }

        /// <summary>
        /// Show Timelineを再生する。
        /// </summary>
        public override UniTask ShowAsync(CancellationToken ct)
        {
            return playShowTimelineAsync(ct);
        }

        /// <summary>
        /// Hide Timelineを再生する。
        /// </summary>
        public override UniTask HideAsync(CancellationToken ct)
        {
            return playHideTimelineAsync(ct);
        }

        Func<CancellationToken, UniTask> BuildTimelineTask
        (
            List<TimedViewAnimation> timeline,
            Func<TimedViewAnimation, float> getStartSeconds,
            Func<ViewBase, CancellationToken, UniTask> playAsync
        )
        {
            if (timeline.Count == 0)
            {
                return _ => UniTask.CompletedTask;
            }

            var timedPlays = new List<Func<CancellationToken, UniTask>>(timeline.Count);

            foreach (var timedAnimation in timeline)
            {
                var view = timedAnimation.View;
                var startSeconds = Mathf.Max(0f, getStartSeconds(timedAnimation));

                timedPlays.Add(ct => PlayViewAtAsync(view, startSeconds, playAsync, ct));
            }

            return ct =>
            {
                var playTasks = new UniTask[timedPlays.Count];
                for (var i = 0; i < timedPlays.Count; i++)
                {
                    playTasks[i] = timedPlays[i](ct);
                }

                return UniTask.WhenAll(playTasks);
            };
        }

        static async UniTask PlayViewAtAsync
        (
            ViewBase view,
            float startSeconds,
            Func<ViewBase, CancellationToken, UniTask> playAsync,
            CancellationToken ct
        )
        {
            ct.ThrowIfCancellationRequested();

            if (startSeconds > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(startSeconds), cancellationToken: ct);
            }

            await playAsync(view, ct);
        }

        static UniTask PlayShowAsync(ViewBase view, CancellationToken ct)
        {
            return view.ShowAsync(ct);
        }

        static UniTask PlayHideAsync(ViewBase view, CancellationToken ct)
        {
            return view.HideAsync(ct);
        }
    }
}
