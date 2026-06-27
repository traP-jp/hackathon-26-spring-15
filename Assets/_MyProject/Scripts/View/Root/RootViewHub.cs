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

        readonly List<ViewBase> views = new();

        public void Initialize()
        {
            gameObject.SetActive(true);

            RegisterViews();
            foreach (var view in views)
            {
                view.Initialize();
                view.Show();
            }

            BindAudioViews();
        }

        void RegisterViews()
        {
            views.Clear();
            views.Add(audioSlider);
            views.Add(audioButton);
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
