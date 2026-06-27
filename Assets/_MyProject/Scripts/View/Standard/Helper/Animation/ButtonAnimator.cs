using System;
using System.Collections.Generic;
using LitMotion;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.View
{
    [RequireComponent(typeof(PointerEventObserver))]
    [DisallowMultipleComponent]
    public class ButtonAnimator : MonoBehaviour
    {
        [Serializable]
        class ColorRuleSettings
        {
            [field: SerializeField]
            public ColorRule Rule { get; private set; } = ColorRule.Darken;

            [field: SerializeField, Min(0f)]
            public float Amount { get; private set; } = 0.08f;

            [field: SerializeField]
            public Color CustomColor { get; private set; } = new(0.92f, 0.92f, 0.92f, 1f);
        }

        [Serializable]
        class IdleSettings
        {
            [field: SerializeField]
            public bool Enabled { get; private set; } = true;

            [field: SerializeField, Min(0f)]
            public float ScaleAmplitude { get; private set; } = 0.04f;

            [field: SerializeField, Min(0.01f)]
            public float Duration { get; private set; } = 2.4f;
        }

        [Serializable]
        class HoverSettings
        {
            [field: SerializeField, Min(1f)]
            public float ScaleMultiplier { get; private set; } = 1.08f;

            [field: SerializeField, Min(0.01f)]
            public float Duration { get; private set; } = 0.08f;

            [field: SerializeField]
            public Ease Ease { get; private set; } = ViewAppearance.DefaultEase;

            [field: SerializeField]
            public ColorRuleSettings SpriteColor { get; private set; } = new();

            [field: SerializeField]
            public ColorRuleSettings TextColor { get; private set; } = new();
        }

        [Serializable]
        class PressSettings
        {
            [field: SerializeField]
            public Vector2 ScaleMultiplier { get; private set; } = new(1.14f, 0.94f);

            [field: SerializeField, Min(0.01f)]
            public float Duration { get; private set; } = 0.06f;

            [field: SerializeField]
            public Ease Ease { get; private set; } = ViewAppearance.DefaultEase;

            [field: SerializeField]
            public ColorRuleSettings SpriteColor { get; private set; } = new();

            [field: SerializeField]
            public ColorRuleSettings TextColor { get; private set; } = new();
        }

        [Serializable]
        class ReleaseSettings
        {
            [field: SerializeField]
            public Vector2 OvershootScaleMultiplier { get; private set; } = new(0.96f, 1.04f);

            [field: SerializeField, Min(0.01f)]
            public float Duration { get; private set; } = 0.12f;

            [field: SerializeField]
            public Ease Ease { get; private set; } = ViewAppearance.DefaultEase;
        }

        [Header("References")]
        [SerializeField] SpriteRenderer[] spriteTargets = Array.Empty<SpriteRenderer>();
        [SerializeField] TMP_Text[] textTargets = Array.Empty<TMP_Text>();
        [SerializeField] Graphic[] graphicTargets = Array.Empty<Graphic>();

        [Header("Idle")]
        [SerializeField] IdleSettings idle = new();

        [Header("Hover")]
        [SerializeField] HoverSettings hover = new();

        [Header("Press")]
        [SerializeField] PressSettings press = new();

        [Header("Release")]
        [SerializeField] ReleaseSettings release = new();

        PointerEventObserver pointerObserver;

        Vector3 baseScale;
        Color[] spriteBaseColors = Array.Empty<Color>();
        Color[] textBaseColors = Array.Empty<Color>();
        Color[] graphicBaseColors = Array.Empty<Color>();

        bool isPlaying;
        bool isHovered;
        bool isPressed;

        MotionHandle scaleHandle;
        MotionHandle idleHandle;
        readonly List<MotionHandle> colorHandles = new();

        enum ColorRule
        {
            Darken,
            Brighten,
            CustomColor,
        }

        enum VisualState
        {
            Normal,
            Hover,
            Pressed,
        }

        void Awake()
        {
            pointerObserver = GetComponent<PointerEventObserver>();
            baseScale = transform.localScale;

            CacheBaseColors();
            SubscribePointerEvents();
        }

        void OnEnable()
        {
            isPlaying = false;
            isHovered = false;
            isPressed = false;

            CancelAllMotions();
            ApplyBaseVisuals();
        }

        void OnDisable()
        {
            isPlaying = false;
            CancelAllMotions();
            ApplyBaseVisuals();
        }

        public void Play()
        {
            isPlaying = true;
            isHovered = false;
            isPressed = false;

            CancelAllMotions();
            ApplyBaseVisuals();
            TryPlayIdleLoop();
        }

        public void Stop()
        {
            isPlaying = false;
            isHovered = false;
            isPressed = false;

            CancelAllMotions();
            ApplyBaseVisuals();
        }

        void SubscribePointerEvents()
        {
            pointerObserver.PointerEntered.Subscribe(_ =>
            {
                if (!isPlaying)
                {
                    return;
                }

                isHovered = true;
                if (isPressed)
                {
                    return;
                }

                PlayState(VisualState.Hover);
            }).AddTo(this);

            pointerObserver.PointerExited.Subscribe(_ =>
            {
                if (!isPlaying)
                {
                    return;
                }

                isHovered = false;
                if (isPressed)
                {
                    return;
                }

                PlayState(VisualState.Normal);
            }).AddTo(this);

            pointerObserver.PointerDown.Subscribe(_ =>
            {
                if (!isPlaying)
                {
                    return;
                }

                isPressed = true;
                PlayState(VisualState.Pressed);
            }).AddTo(this);

            pointerObserver.PointerUp.Subscribe(_ =>
            {
                if (!isPlaying)
                {
                    return;
                }

                if (!isPressed)
                {
                    return;
                }

                isPressed = false;
                PlayRelease(ResolveTargetState());
            }).AddTo(this);
        }

        void CacheBaseColors()
        {
            spriteBaseColors = new Color[spriteTargets.Length];
            for (var i = 0; i < spriteTargets.Length; i++)
            {
                spriteBaseColors[i] = spriteTargets[i].color;
            }

            textBaseColors = new Color[textTargets.Length];
            for (var i = 0; i < textTargets.Length; i++)
            {
                textBaseColors[i] = textTargets[i].color;
            }

            graphicBaseColors = new Color[graphicTargets.Length];
            for (var i = 0; i < graphicTargets.Length; i++)
            {
                graphicBaseColors[i] = graphicTargets[i].color;
            }
        }

        VisualState ResolveTargetState()
        {
            if (isPressed)
            {
                return VisualState.Pressed;
            }

            return isHovered ? VisualState.Hover : VisualState.Normal;
        }

        void PlayState(VisualState state)
        {
            StopIdleLoop();

            switch (state)
            {
                case VisualState.Pressed:
                    PlayScale(GetPressedScale(), press.Duration, press.Ease);
                    PlayColors(press.SpriteColor, press.TextColor, press.Duration, press.Ease);
                    return;

                case VisualState.Hover:
                    PlayScale(GetHoverScale(), hover.Duration, hover.Ease);
                    PlayColors(hover.SpriteColor, hover.TextColor, hover.Duration, hover.Ease);
                    return;

                default:
                    PlayScale(baseScale, hover.Duration, hover.Ease, TryPlayIdleLoop);
                    PlayBaseColors(hover.Duration, hover.Ease);
                    return;
            }
        }

        void PlayRelease(VisualState state)
        {
            StopIdleLoop();
            StopScaleMotion();

            var targetScale = state == VisualState.Hover ? GetHoverScale() : baseScale;
            var overshootScale = MultiplyXY(targetScale, release.OvershootScaleMultiplier);
            var halfDuration = release.Duration * 0.5f;

            scaleHandle = LMotion.Create(transform.localScale, overshootScale, halfDuration)
                .WithEase(release.Ease)
                .WithOnComplete(() =>
                {
                    scaleHandle = LMotion.Create(overshootScale, targetScale, halfDuration)
                        .WithEase(release.Ease)
                        .WithOnComplete(() =>
                        {
                            if (state == VisualState.Normal)
                            {
                                TryPlayIdleLoop();
                            }
                        })
                        .Bind(value => transform.localScale = value)
                        .AddTo(this);
                })
                .Bind(value => transform.localScale = value)
                .AddTo(this);

            if (state == VisualState.Hover)
            {
                PlayColors(hover.SpriteColor, hover.TextColor, release.Duration, release.Ease);
                return;
            }

            PlayBaseColors(release.Duration, release.Ease);
        }

        void PlayScale(Vector3 target, float duration, Ease ease, Action onComplete = null)
        {
            StopScaleMotion();

            scaleHandle = LMotion.Create(transform.localScale, target, duration)
                .WithEase(ease)
                .WithOnComplete(onComplete)
                .Bind(value => transform.localScale = value)
                .AddTo(this);
        }

        void TryPlayIdleLoop()
        {
            if (!isPlaying)
            {
                return;
            }

            if (!idle.Enabled)
            {
                return;
            }

            StopIdleLoop();

            idleHandle = LMotion.Create(Mathf.PI * 0.5f, Mathf.PI * 2.5f, idle.Duration)
                .WithEase(Ease.Linear)
                .WithLoops(-1, LoopType.Incremental)
                .Bind(angle =>
                {
                    var ratio = 1f + Mathf.Cos(angle) * idle.ScaleAmplitude;
                    transform.localScale = baseScale * ratio;
                })
                .AddTo(this);
        }

        void PlayColors(ColorRuleSettings spriteRule, ColorRuleSettings textRule, float duration, Ease ease)
        {
            StopColorMotions();

            for (var i = 0; i < spriteTargets.Length; i++)
            {
                var target = spriteTargets[i];
                var to = ResolveColor(spriteBaseColors[i], spriteRule);
                var handle = LMotion.Create(target.color, to, duration)
                    .WithEase(ease)
                    .Bind(value => target.color = value)
                    .AddTo(this);
                colorHandles.Add(handle);
            }

            for (var i = 0; i < textTargets.Length; i++)
            {
                var target = textTargets[i];
                var to = ResolveColor(textBaseColors[i], textRule);
                var handle = LMotion.Create(target.color, to, duration)
                    .WithEase(ease)
                    .Bind(value => target.color = value)
                    .AddTo(this);
                colorHandles.Add(handle);
            }

            for (var i = 0; i < graphicTargets.Length; i++)
            {
                var target = graphicTargets[i];
                var to = ResolveColor(graphicBaseColors[i], spriteRule);
                var handle = LMotion.Create(target.color, to, duration)
                    .WithEase(ease)
                    .Bind(value => target.color = value)
                    .AddTo(this);
                colorHandles.Add(handle);
            }
        }

        void PlayBaseColors(float duration, Ease ease)
        {
            StopColorMotions();

            for (var i = 0; i < spriteTargets.Length; i++)
            {
                var target = spriteTargets[i];
                var to = spriteBaseColors[i];
                var handle = LMotion.Create(target.color, to, duration)
                    .WithEase(ease)
                    .Bind(value => target.color = value)
                    .AddTo(this);
                colorHandles.Add(handle);
            }

            for (var i = 0; i < textTargets.Length; i++)
            {
                var target = textTargets[i];
                var to = textBaseColors[i];
                var handle = LMotion.Create(target.color, to, duration)
                    .WithEase(ease)
                    .Bind(value => target.color = value)
                    .AddTo(this);
                colorHandles.Add(handle);
            }

            for (var i = 0; i < graphicTargets.Length; i++)
            {
                var target = graphicTargets[i];
                var to = graphicBaseColors[i];
                var handle = LMotion.Create(target.color, to, duration)
                    .WithEase(ease)
                    .Bind(value => target.color = value)
                    .AddTo(this);
                colorHandles.Add(handle);
            }
        }

        Color ResolveColor(Color baseColor, ColorRuleSettings settings)
        {
            return settings.Rule switch
            {
                ColorRule.Darken => Darken(baseColor, settings.Amount),
                ColorRule.Brighten => Brighten(baseColor, settings.Amount),
                ColorRule.CustomColor => settings.CustomColor,
                _ => baseColor,
            };
        }

        static Color Darken(Color color, float amount)
        {
            var ratio = Mathf.Clamp01(1f - amount);
            return new Color(color.r * ratio, color.g * ratio, color.b * ratio, color.a);
        }

        static Color Brighten(Color color, float amount)
        {
            var ratio = Mathf.Clamp01(amount);
            return new Color(
                Mathf.Lerp(color.r, 1f, ratio),
                Mathf.Lerp(color.g, 1f, ratio),
                Mathf.Lerp(color.b, 1f, ratio),
                color.a);
        }

        Vector3 GetHoverScale()
        {
            return baseScale * hover.ScaleMultiplier;
        }

        Vector3 GetPressedScale()
        {
            return MultiplyXY(baseScale, press.ScaleMultiplier);
        }

        static Vector3 MultiplyXY(Vector3 baseValue, Vector2 multiplier)
        {
            return new Vector3(baseValue.x * multiplier.x, baseValue.y * multiplier.y, baseValue.z);
        }

        void ApplyBaseVisuals()
        {
            transform.localScale = baseScale;

            for (var i = 0; i < spriteTargets.Length; i++)
            {
                spriteTargets[i].color = spriteBaseColors[i];
            }

            for (var i = 0; i < textTargets.Length; i++)
            {
                textTargets[i].color = textBaseColors[i];
            }

            for (var i = 0; i < graphicTargets.Length; i++)
            {
                graphicTargets[i].color = graphicBaseColors[i];
            }
        }

        void CancelAllMotions()
        {
            StopScaleMotion();
            StopIdleLoop();
            StopColorMotions();
        }

        void StopScaleMotion()
        {
            scaleHandle.TryCancel();
        }

        void StopIdleLoop()
        {
            idleHandle.TryCancel();
        }

        void StopColorMotions()
        {
            foreach (var colorHandle in colorHandles)
            {
                colorHandle.TryCancel();
            }

            colorHandles.Clear();
        }
    }
}
