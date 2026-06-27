using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MyProject.View
{
    [RequireComponent(typeof(ViewAnimationTimeline))]
    public class SelectViewHub : SceneViewHubBase
    {
        SelectActionsObserver selectActionsObserver;
        ViewAnimationTimeline animationTimeline;

        public override void Initialize()
        {
            selectActionsObserver ??= new SelectActionsObserver();
            animationTimeline = GetComponent<ViewAnimationTimeline>();

            selectActionsObserver.Disable();
            animationTimeline.Initialize();
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await animationTimeline.ShowAsync(ct);
            selectActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            selectActionsObserver.Disable();
            await animationTimeline.HideAsync(ct);
            gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            selectActionsObserver.Dispose();
        }
    }
}
