using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MyProject.View
{
    [RequireComponent(typeof(ViewAnimationTimeline))]
    public class ResultViewHub : SceneViewHubBase
    {
        ResultActionsObserver resultActionsObserver;
        ViewAnimationTimeline animationTimeline;

        public override void Initialize()
        {
            resultActionsObserver ??= new ResultActionsObserver();
            animationTimeline = GetComponent<ViewAnimationTimeline>();

            resultActionsObserver.Disable();
            animationTimeline.Initialize();
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await animationTimeline.ShowAsync(ct);
            resultActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            resultActionsObserver.Disable();
            await animationTimeline.HideAsync(ct);
            gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            resultActionsObserver.Dispose();
        }
    }
}
