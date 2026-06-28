using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace MyProject.View
{
    [RequireComponent(typeof(StandardTransitionAnimator))]
    public class HealthView : ViewBase
    {
        [SerializeField] TMP_Text text;
        StandardTransitionAnimator animator;

        public override void Initialize()
        {
            text ??= GetComponentInChildren<TMP_Text>(true);
            animator = GetComponent<StandardTransitionAnimator>();

            if (text == null)
            {
                throw new InvalidOperationException($"{nameof(HealthView)}: Text が設定されていません。");
            }

            animator.Initialize();
            SetHealth(0);
            gameObject.SetActive(false);
        }

        public override void Show()
        {
            gameObject.SetActive(true);
            animator.Show();
        }

        public override void Hide()
        {
            animator.Hide();
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await animator.ShowAsync(ct);
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            await animator.HideAsync(ct);
            gameObject.SetActive(false);
        }

        public void SetHealth(int health)
        {
            text.text = $"HP: {health}";
        }
    }
}
