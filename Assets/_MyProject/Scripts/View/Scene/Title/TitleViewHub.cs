using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MyProject.View
{
    [RequireComponent(typeof(ViewAnimationTimeline))]
    public class TitleViewHub : SceneViewHubBase
    {
        TitleActionsObserver titleActionsObserver;
        ViewAnimationTimeline animationTimeline;

        public override void Initialize()
        {
            titleActionsObserver ??= new TitleActionsObserver();
            animationTimeline = GetComponent<ViewAnimationTimeline>();

            titleActionsObserver.Disable();
            animationTimeline.Initialize();
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await animationTimeline.ShowAsync(ct);
            titleActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            titleActionsObserver.Disable();
            await animationTimeline.HideAsync(ct);
            gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            titleActionsObserver.Dispose();
        }
    }
}
