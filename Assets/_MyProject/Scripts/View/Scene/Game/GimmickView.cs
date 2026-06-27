using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MyProject.View
{
    public class GimmickView : ViewBase
    {
        [SerializeField] private GimmickType _type;
        [SerializeField] private int _damage = 10;

        public override void Initialize()
        {
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(other.TryGetComponent<PlayerView>(out var player))
            {
                player.Damage(_damage, _type);
            }
        }
    }
}
