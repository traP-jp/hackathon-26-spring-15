using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.View
{
    [RequireComponent(typeof(StandardTransitionAnimator))]
    public class StandardSliderView : ViewBase
    {
        public Observable<float> ValueChanged => valueChanged;
        public Observable<Unit> Clicked => handleClickObserver.Clicked.Select(_ => Unit.Default);
        public Observable<Unit> DoubleClicked => handleClickObserver.Clicked
            .Where(pointerEventData => pointerEventData.clickCount == 2)
            .Select(_ => Unit.Default);
        readonly Subject<float> valueChanged = new();

        [SerializeField] Slider slider;
        [SerializeField] PointerClickObserver handleClickObserver;
        StandardTransitionAnimator transitionAnimator;

        public override void Initialize()
        {
            transitionAnimator = GetComponent<StandardTransitionAnimator>();

            transitionAnimator.Initialize();

            slider.onValueChanged.RemoveListener(HandleValueChanged);
            slider.onValueChanged.AddListener(HandleValueChanged);

            gameObject.SetActive(false);
        }

        public override void Show()
        {
            gameObject.SetActive(true);
            transitionAnimator.Show();
        }

        public override void Hide()
        {
            transitionAnimator.Hide();
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await transitionAnimator.ShowAsync(ct);
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            await transitionAnimator.HideAsync(ct);
            gameObject.SetActive(false);
        }

        public void SetValue(float value)
        {
            slider.SetValueWithoutNotify(value);
        }

        void HandleValueChanged(float value)
        {
            valueChanged.OnNext(value);
        }

        void OnDestroy()
        {
            slider.onValueChanged.RemoveListener(HandleValueChanged);

            valueChanged.OnCompleted();
            valueChanged.Dispose();
        }
    }
}

