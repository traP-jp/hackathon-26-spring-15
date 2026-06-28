using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace MyProject.View
{
    [RequireComponent(typeof(StandardTransitionAnimator))]
    public class ResultTextView : ViewBase
    {
        [SerializeField] TMP_Text text;
        StandardTransitionAnimator animator;

        public override void Initialize()
        {
            animator = GetComponent<StandardTransitionAnimator>();
            animator.Initialize();
            SetScore(0);
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

        public void SetScore(int score)
        {
            text.text = $"SCORE: {score}";
        }
    }
}
