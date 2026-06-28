using LitMotion;
using UnityEngine;

namespace MyProject.View
{
    [DisallowMultipleComponent]
    public class ScaleBreathingAnimator : MonoBehaviour
    {
        [SerializeField] bool playOnEnable = true;

        [SerializeField, Min(0f)]
        float scaleAmplitude = 0.04f;

        [SerializeField, Min(0.01f)]
        float duration = 2.4f;

        Vector3 baseScale;
        MotionHandle handle;

        void Awake()
        {
            baseScale = transform.localScale;
        }

        void OnEnable()
        {
            if (playOnEnable)
            {
                Play();
            }
        }

        void OnDisable()
        {
            Stop();
        }

        public void Play()
        {
            handle.TryCancel();

            handle = LMotion.Create(Mathf.PI * 0.5f, Mathf.PI * 2.5f, duration)
                .WithEase(Ease.Linear)
                .WithLoops(-1, LoopType.Incremental)
                .Bind(angle =>
                {
                    var ratio = 1f + Mathf.Cos(angle) * scaleAmplitude;
                    transform.localScale = baseScale * ratio;
                })
                .AddTo(this);
        }

        public void Stop()
        {
            handle.TryCancel();
            transform.localScale = baseScale;
        }
    }
}
