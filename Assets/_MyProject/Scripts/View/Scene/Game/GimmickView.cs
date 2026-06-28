using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MyProject.View
{
    public class GimmickView : ViewBase
    {
        [SerializeField] private GimmickType _type;
        [SerializeField] private int _damage = 10;
        bool hasPassed;
        bool hasFailed;

        public override void Initialize()
        {
            hasPassed = false;
            hasFailed = false;
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

        public bool TryPass(float playerPositionX, out bool cleared)
        {
            cleared = false;

            if (hasPassed || playerPositionX <= transform.position.x)
            {
                return false;
            }

            hasPassed = true;
            cleared = !hasFailed;
            return true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerView>(out var player))
            {
                if (player.IsDamageCondition(_type))
                {
                    hasFailed = true;
                }

                player.Damage(_damage, _type);
            }
        }
    }
}
