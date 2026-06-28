using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace MyProject.View
{
    [RequireComponent(typeof(ViewAnimationTimeline))]
    public class ResultViewHub : SceneViewHubBase
    {
        public Observable<Unit> Retry => resultActionsObserver.Retry.Select(_ =>
        {
            PlaySe(gameStartSeClip);
            return Unit.Default;
        });
        public Observable<Unit> Quit => resultActionsObserver.Quit.Select(_ =>
        {
            PlaySe(quitSeClip);
            return Unit.Default;
        });

        [SerializeField] AudioClip gameStartSeClip;
        [SerializeField] AudioClip quitSeClip;
        [SerializeField] ResultTextView resultTextView;

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

        public void SetScore(int score)
        {
            resultTextView?.SetScore(score);
        }

        void OnDestroy()
        {
            resultActionsObserver.Dispose();
        }
    }
}
