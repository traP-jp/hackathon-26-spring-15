using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Unity.Cinemachine;

namespace MyProject.View
{
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CameraFollowView : ViewBase
    {
        [SerializeField] Transform player;
        [SerializeField] Transform followTarget;

        CinemachineImpulseSource impulseSource;

        public override void Initialize()
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
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
            position.x = player.position.x;
            followTarget.position = position;
        }

        public void ShakeCamera()
        {
            impulseSource.GenerateImpulse();
        }
    }
}
