using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Unity.Cinemachine;

namespace MyProject.View
{
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CameraFollowView : ViewBase
    {
        [SerializeField] PlayerView player;
        [SerializeField] Transform followTarget;
        [SerializeField] Vector3 shakeVelocity = new(1.2f, 0.8f, 0f);

        readonly CompositeDisposable disposables = new();
        CinemachineImpulseSource impulseSource;

        public override void Initialize()
        {
            disposables.Clear();
            impulseSource = GetComponent<CinemachineImpulseSource>();

            player.Damaged
                .Subscribe(_ => ShakeCamera())
                .AddTo(disposables);
            gameObject.SetActive(false);
        }

        public override void Show()
        {
            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
        }

        public override UniTask ShowAsync(CancellationToken ct)
        {
            Show();
            return UniTask.CompletedTask;
        }

        public override UniTask HideAsync(CancellationToken ct)
        {
            Hide();
            return UniTask.CompletedTask;
        }

        void LateUpdate()
        {
            var position = followTarget.position;
            position.x = player.transform.position.x;
            followTarget.position = position;
        }

        public void ShakeCamera()
        {
            impulseSource.GenerateImpulseWithVelocity(shakeVelocity);
        }

        void OnDestroy()
        {
            disposables.Dispose();
        }
    }
}
