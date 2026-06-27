using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace MyProject.View
{
    [RequireComponent(typeof(PointerEventObserver))]
    [RequireComponent(typeof(ButtonAnimator))]
    [RequireComponent(typeof(StandardTransitionAnimator))]
    public class StandardButtonView : ViewBase
    {
        public Observable<Unit> Clicked => pointerEventObserver.Clicked.Select(_ => Unit.Default);

        PointerEventObserver pointerEventObserver;
        ButtonAnimator buttonAnimator;
        StandardTransitionAnimator transitionAnimator;

        public override void Initialize()
        {
            pointerEventObserver = GetComponent<PointerEventObserver>();
            buttonAnimator = GetComponent<ButtonAnimator>();
            transitionAnimator = GetComponent<StandardTransitionAnimator>();

            transitionAnimator.Initialize();

            gameObject.SetActive(false);
        }

        public override void Show()
        {
            gameObject.SetActive(true);
            transitionAnimator.Show();
            buttonAnimator.Play();
        }

        public override void Hide()
        {
            buttonAnimator.Stop();
            transitionAnimator.Hide();
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await transitionAnimator.ShowAsync(ct);
            buttonAnimator.Play();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            buttonAnimator.Stop();
            await transitionAnimator.HideAsync(ct);
            gameObject.SetActive(false);
        }
    }
}

