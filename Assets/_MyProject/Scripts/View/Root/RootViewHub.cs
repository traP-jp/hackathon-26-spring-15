using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace MyProject.View
{
    public class RootViewHub : MonoBehaviour
    {
        [SerializeField] StandardSliderView audioSlider;
        [SerializeField] AudioButtonView audioButton;

        [SerializeField] ViewBase[] views;

        public void Initialize()
        {
            gameObject.SetActive(true);

            foreach (var view in views)
            {
                view.Initialize();
                view.Show();
            }

            BindAudioViews();
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
