using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.View
{
    [RequireComponent(typeof(PointerEventObserver))]
    [RequireComponent(typeof(ButtonAnimator))]
    [RequireComponent(typeof(StandardTransitionAnimator))]
    public class AudioButtonView : ViewBase
    {
        public Observable<float> VolumeRequested => volumeRequested;
        readonly Subject<float> volumeRequested = new();

        readonly CompositeDisposable disposables = new();

        [SerializeField] Image iconImage;
        [SerializeField] Sprite volumeOnIcon;
        [SerializeField] Sprite volumeOffIcon;

        PointerEventObserver pointerEventObserver;
        ButtonAnimator buttonAnimator;
        StandardTransitionAnimator transitionAnimator;
        float currentVolume;
        float? savedVolume;

        public override void Initialize()
        {
            disposables.Clear();
            savedVolume = null;

            pointerEventObserver = GetComponent<PointerEventObserver>();
            buttonAnimator = GetComponent<ButtonAnimator>();
            transitionAnimator = GetComponent<StandardTransitionAnimator>();

            pointerEventObserver.Clicked
                .Subscribe(_ => HandleClicked())
                .AddTo(disposables);

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

        public void SetVolume(float volume)
        {
            currentVolume = volume;
            iconImage.sprite = volume <= 0f ? volumeOffIcon : volumeOnIcon;

            if (savedVolume.HasValue && volume > 0f)
            {
                savedVolume = null;
            }
        }

        void HandleClicked()
        {
            if (savedVolume.HasValue)
            {
                var restoreVolume = savedVolume.Value;
                savedVolume = null;
                volumeRequested.OnNext(restoreVolume);
                return;
            }

            savedVolume = currentVolume;
            volumeRequested.OnNext(0f);
        }

        void OnDestroy()
        {
            disposables.Dispose();

            volumeRequested.OnCompleted();
            volumeRequested.Dispose();
        }
    }
}

