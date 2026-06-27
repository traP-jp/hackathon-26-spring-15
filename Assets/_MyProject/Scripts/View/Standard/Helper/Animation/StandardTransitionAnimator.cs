using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.View
{
    [DisallowMultipleComponent]
    public class StandardTransitionAnimator : MonoBehaviour
    {
        [Serializable]
        class MoveSettings
        {
            [field: SerializeField, Min(0f)]
            public float Distance { get; private set; } = 0f;

            [field: SerializeField]
            public float AngleDegrees { get; private set; } = 0f;

            [field: SerializeField]
            public Ease Ease { get; private set; } = ViewAppearance.DefaultEase;
        }

        [Serializable]
        class RotationSettings
        {
            [field: SerializeField]
            public float AngleDegrees { get; private set; } = 0f;

            [field: SerializeField]
            public Ease Ease { get; private set; } = ViewAppearance.DefaultEase;
        }

        [Serializable]
        class ScaleSettings
        {
            [field: SerializeField, Min(0f)]
            public float Multiplier { get; private set; } = 1f;

            [field: SerializeField]
            public Ease Ease { get; private set; } = ViewAppearance.DefaultEase;
        }

        [Serializable]
        class FadeSettings
        {
            [field: SerializeField]
            public bool IsFade { get; private set; } = true;

            [field: SerializeField]
            public Ease Ease { get; private set; } = ViewAppearance.DefaultEase;
        }

        [Serializable]
        class PhaseSettings
        {
            [field: SerializeField, Min(0f)]
            public float DurationSeconds { get; private set; } = 0.3f;

            [field: SerializeField]
            public bool UseCanvasGroupForFade { get; private set; } = true;

            [field: SerializeField]
            public MoveSettings Move { get; private set; } = new();

            [field: SerializeField]
            public RotationSettings Rotation { get; private set; } = new();

            [field: SerializeField]
            public ScaleSettings Scale { get; private set; } = new();

            [field: SerializeField]
            public FadeSettings Fade { get; private set; } = new();
        }

        [Header("Show")]
        [SerializeField] PhaseSettings showSettings = new();

        [Header("Hide")]
        [SerializeField] PhaseSettings hideSettings = new();

        FadeTarget selfCanvasGroupFadeTarget;
        readonly List<FadeTarget> childFadeTargets = new();

        RectTransform rectTransform;
        bool usesAnchoredPosition;
        Vector3 basePosition;
        Quaternion baseRotation;
        Vector3 baseScale;

        MotionHandle moveHandle;
        MotionHandle rotationHandle;
        MotionHandle fadeHandle;
        MotionHandle scaleHandle;

        public void Initialize()
        {
            var selfCanvasGroup = GetComponent<CanvasGroup>();
            selfCanvasGroupFadeTarget = CreateCanvasGroupFadeTarget(selfCanvasGroup);

            rectTransform = transform as RectTransform;
            usesAnchoredPosition = rectTransform != null;
            basePosition = usesAnchoredPosition ? rectTransform.anchoredPosition3D : transform.localPosition;
            baseRotation = transform.localRotation;
            baseScale = transform.localScale;

            CacheChildFadeTargets();
            CancelRunningMotions();
        }

        public UniTask ShowAsync(CancellationToken ct) =>
             PlayPhaseAsync(showSettings, PhaseType.Show, ct);

        public UniTask HideAsync(CancellationToken ct) =>
             PlayPhaseAsync(hideSettings, PhaseType.Hide, ct);

        public void Show()
        {
            ApplyPhaseEnd(showSettings, PhaseType.Show);
        }

        public void Hide()
        {
            ApplyPhaseEnd(hideSettings, PhaseType.Hide);
        }

        UniTask PlayPhaseAsync(PhaseSettings settings, PhaseType phaseType, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            CancelRunningMotions();

            var duration = settings.DurationSeconds;
            var tasks = new List<UniTask>();

            moveHandle = CreateMoveMotion(settings.Move, phaseType, duration).AddTo(this);
            tasks.Add(moveHandle.ToUniTask(CancelBehavior.Cancel, false, ct));

            rotationHandle = CreateRotationMotion(settings.Rotation, phaseType, duration).AddTo(this);
            tasks.Add(rotationHandle.ToUniTask(CancelBehavior.Cancel, false, ct));

            scaleHandle = CreateScaleMotion(settings.Scale, phaseType, duration).AddTo(this);
            tasks.Add(scaleHandle.ToUniTask(CancelBehavior.Cancel, false, ct));

            if (settings.Fade.IsFade)
            {
                bool useCanvasGroup = settings.UseCanvasGroupForFade && selfCanvasGroupFadeTarget.IsAlive();

                fadeHandle = CreateFadeMotion(useCanvasGroup, settings.Fade, phaseType, duration).AddTo(this);
                tasks.Add(fadeHandle.ToUniTask(CancelBehavior.Cancel, false, ct));
            }

            return UniTask.WhenAll(tasks);
        }

        void ApplyPhaseEnd(PhaseSettings settings, PhaseType phaseType)
        {
            CancelRunningMotions();

            SetCurrentPosition(GetMoveTarget(settings.Move, phaseType));
            transform.localRotation = GetRotationTarget(settings.Rotation, phaseType);
            transform.localScale = GetScaleTarget(settings.Scale, phaseType);

            if (!settings.Fade.IsFade)
            {
                return;
            }

            bool useCanvasGroup = settings.UseCanvasGroupForFade && selfCanvasGroupFadeTarget.IsAlive();
            var fadeTargets = useCanvasGroup ? new List<FadeTarget> { selfCanvasGroupFadeTarget } : childFadeTargets;
            ApplyFadeProgress(fadeTargets, 1f, phaseType);
        }

        MotionHandle CreateMoveMotion(MoveSettings settings, PhaseType phaseType, float duration)
        {
            return LMotion.Create(GetMoveStart(settings, phaseType), GetMoveTarget(settings, phaseType), duration)
                .WithEase(settings.Ease)
                .Bind(SetCurrentPosition);
        }

        Vector3 GetMoveStart(MoveSettings settings, PhaseType phaseType)
        {
            return phaseType == PhaseType.Hide ? basePosition : GetMoveHiddenPosition(settings);
        }

        Vector3 GetMoveTarget(MoveSettings settings, PhaseType phaseType)
        {
            return phaseType == PhaseType.Hide ? GetMoveHiddenPosition(settings) : basePosition;
        }

        Vector3 GetMoveHiddenPosition(MoveSettings settings)
        {
            var radian = settings.AngleDegrees * Mathf.Deg2Rad;
            var direction = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0f);
            var offset = direction * settings.Distance;
            return basePosition + offset;
        }

        void SetCurrentPosition(Vector3 value)
        {
            if (usesAnchoredPosition)
            {
                rectTransform.anchoredPosition3D = value;
                return;
            }

            transform.localPosition = value;
        }

        MotionHandle CreateFadeMotion(bool useCanvasGroup, FadeSettings settings, PhaseType phaseType, float duration)
        {
            var fadeTargets = useCanvasGroup ? new List<FadeTarget> { selfCanvasGroupFadeTarget } : childFadeTargets;
            return LMotion.Create(0f, 1f, duration)
                .WithEase(settings.Ease)
                .Bind(progress => ApplyFadeProgress(fadeTargets, progress, phaseType));
        }

        MotionHandle CreateRotationMotion(RotationSettings settings, PhaseType phaseType, float duration)
        {
            return LMotion.Create(GetRotationStart(settings, phaseType), GetRotationTarget(settings, phaseType), duration)
                .WithEase(settings.Ease)
                .Bind(value => transform.localRotation = value);
        }

        Quaternion GetRotationStart(RotationSettings settings, PhaseType phaseType)
        {
            return phaseType == PhaseType.Hide ? baseRotation : GetHiddenRotation(settings);
        }

        Quaternion GetRotationTarget(RotationSettings settings, PhaseType phaseType)
        {
            return phaseType == PhaseType.Hide ? GetHiddenRotation(settings) : baseRotation;
        }

        Quaternion GetHiddenRotation(RotationSettings settings)
        {
            var offset = Quaternion.Euler(0f, 0f, settings.AngleDegrees);
            return baseRotation * offset;
        }

        MotionHandle CreateScaleMotion(ScaleSettings settings, PhaseType phaseType, float duration)
        {
            return LMotion.Create(GetScaleStart(settings, phaseType), GetScaleTarget(settings, phaseType), duration)
                .WithEase(settings.Ease)
                .Bind(value => transform.localScale = value);
        }

        Vector3 GetScaleStart(ScaleSettings settings, PhaseType phaseType)
        {
            return phaseType == PhaseType.Hide ? baseScale : GetHiddenScale(settings);
        }

        Vector3 GetScaleTarget(ScaleSettings settings, PhaseType phaseType)
        {
            return phaseType == PhaseType.Hide ? GetHiddenScale(settings) : baseScale;
        }

        Vector3 GetHiddenScale(ScaleSettings settings)
        {
            return baseScale * settings.Multiplier;
        }

        void CancelRunningMotions()
        {
            moveHandle.TryCancel();
            rotationHandle.TryCancel();
            fadeHandle.TryCancel();
            scaleHandle.TryCancel();
        }

        void CacheChildFadeTargets()
        {
            childFadeTargets.Clear();

            var graphics = GetComponentsInChildren<Graphic>(true);
            foreach (var graphic in graphics)
            {
                childFadeTargets.Add(CreateGraphicFadeTarget(graphic));
            }

            var spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var spriteRenderer in spriteRenderers)
            {
                childFadeTargets.Add(CreateSpriteFadeTarget(spriteRenderer));
            }
        }

        void ApplyFadeProgress(IReadOnlyList<FadeTarget> fadeTargets, float progress, PhaseType phaseType)
        {
            foreach (var target in fadeTargets)
            {
                var from = phaseType == PhaseType.Hide ? target.BaseAlpha : 0f;
                var to = phaseType == PhaseType.Hide ? 0f : target.BaseAlpha;

                var value = Mathf.Lerp(from, to, progress);
                target.ApplyAlpha(value);
            }
        }

        static FadeTarget CreateCanvasGroupFadeTarget(CanvasGroup target)
        {
            if (target == null)
            {
                return CreateInvalidFadeTarget();
            }

            return new FadeTarget(
                () => target != null,
                value => target.alpha = value,
                target.alpha);
        }

        static FadeTarget CreateGraphicFadeTarget(Graphic target)
        {
            if (target == null)
            {
                return CreateInvalidFadeTarget();
            }

            return new FadeTarget(
                () => target != null,
                value =>
                {
                    var color = target.color;
                    color.a = value;
                    target.color = color;
                },
                target.color.a);
        }

        static FadeTarget CreateSpriteFadeTarget(SpriteRenderer target)
        {
            if (target == null)
            {
                return CreateInvalidFadeTarget();
            }

            return new FadeTarget(
                () => target != null,
                value =>
                {
                    var color = target.color;
                    color.a = value;
                    target.color = color;
                },
                target.color.a);
        }

        static FadeTarget CreateInvalidFadeTarget()
        {
            return new FadeTarget(() => false, _ => { }, 0f);
        }

        readonly struct FadeTarget
        {
            public Func<bool> IsAlive { get; }
            public float BaseAlpha { get; }

            readonly Action<float> writeAlpha;

            public FadeTarget(Func<bool> isAlive, Action<float> writeAlpha, float baseAlpha)
            {
                IsAlive = isAlive;
                BaseAlpha = baseAlpha;
                this.writeAlpha = writeAlpha;
            }

            public void ApplyAlpha(float alpha)
            {
                if (!IsAlive())
                {
                    return;
                }

                writeAlpha(alpha);
            }
        }

        enum PhaseType
        {
            Show,
            Hide
        }
    }
}
