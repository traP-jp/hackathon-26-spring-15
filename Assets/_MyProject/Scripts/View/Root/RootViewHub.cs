using R3;
using UnityEngine;

namespace MyProject.View
{
    public class RootViewHub : MonoBehaviour
    {
        [SerializeField] StandardSliderView audioSlider;
        [SerializeField] AudioButtonView audioButton;
        [SerializeField] AudioClip bgmClip;

        [SerializeField] ViewBase[] views;

        bool hasStartedBgm;

        public void Initialize()
        {
            gameObject.SetActive(true);

            foreach (var view in views)
            {
                view.Initialize();
                view.Show();
            }

            BindAudioViews();
            PlayBgmOnce();
        }

        void PlayBgmOnce()
        {
            if (hasStartedBgm || bgmClip == null)
            {
                return;
            }

            var audioPlayer = AudioPlayerView.Instance;
            if (audioPlayer == null)
            {
                return;
            }

            audioPlayer.PlayBgm(bgmClip);
            hasStartedBgm = true;
        }

        void BindAudioViews()
        {
            var audioPlayer = AudioPlayerView.Instance;

            audioSlider.ValueChanged
                .Subscribe(audioPlayer.SetVolume)
                .AddTo(this);
            audioSlider.DoubleClicked
                .Subscribe(_ => audioPlayer.ResetVolume())
                .AddTo(this);
            audioButton.VolumeRequested
                .Subscribe(audioPlayer.SetVolume)
                .AddTo(this);
            audioPlayer.Volume
                .Subscribe(value =>
                {
                    audioSlider.SetValue(value);
                    audioButton.SetVolume(value);
                })
                .AddTo(this);
        }
    }
}
