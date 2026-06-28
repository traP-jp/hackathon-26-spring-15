using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Volume = UnityEngine.Rendering.Volume;
using VolumeComponent = UnityEngine.Rendering.VolumeComponent;
using VolumeProfile = UnityEngine.Rendering.VolumeProfile;

namespace MyProject.View
{
    public class GamePostProcessView : ViewBase
    {
        [SerializeField] Volume postProcessVolume;
        [Header("Damage Post Process")]
        [SerializeField, Range(0f, 1f)] float damageVignetteIntensity = 0.45f;
        [SerializeField, Range(0f, 1f)] float damageChromaticAberrationIntensity = 0.55f;
        [SerializeField, Range(-1f, 1f)] float damageLensDistortionIntensity = -0.25f;
        [SerializeField, Min(0.01f)] float damagePostProcessDuration = 0.25f;
        [Header("Low Health Post Process")]
        [SerializeField, Range(0f, 1f)] float lowHealthThreshold = 0.35f;
        [SerializeField, Range(0f, 1f)] float lowHealthVignetteIntensity = 0.22f;
        [SerializeField, Range(0f, 1f)] float lowHealthPulseIntensity = 0.08f;
        [SerializeField, Min(0f)] float lowHealthPulseSpeed = 4f;
        [SerializeField, Min(0.01f)] float lowHealthTransitionDuration = 0.25f;
        [Header("Boost Post Process")]
        [SerializeField, Range(0f, 1f)] float boostMotionBlurIntensity = 0.55f;
        [SerializeField, Range(0f, 0.2f)] float boostMotionBlurClamp = 0.08f;
        [SerializeField, Min(0.01f)] float boostMotionBlurTransitionDuration = 0.12f;

        Vignette vignette;
        ChromaticAberration chromaticAberration;
        LensDistortion lensDistortion;
        MotionBlur motionBlur;
        MotionHandle damagePostProcessHandle;
        MotionHandle lowHealthPostProcessHandle;
        MotionHandle boostMotionBlurHandle;
        int maxHealth;
        float damagePostProcessRate;
        float lowHealthPostProcessRate;
        float boostMotionBlurRate;

        public override void Initialize()
        {
            InitializePostProcess();
            ResetState();
        }

        public override void Show()
        {
            ApplyPostProcess();
        }

        public override void Hide()
        {
            ResetState();
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

        public void PlayDamage()
        {
            if (vignette == null || chromaticAberration == null || lensDistortion == null)
            {
                return;
            }

            damagePostProcessHandle.TryCancel();
            damagePostProcessHandle = LMotion.Create(1f, 0f, damagePostProcessDuration)
                .WithEase(Ease.OutQuad)
                .Bind(value =>
                {
                    damagePostProcessRate = value;
                    ApplyPostProcess();
                })
                .AddTo(this);
        }

        public void SetHealth(int health)
        {
            if (health > maxHealth)
            {
                maxHealth = health;
            }

            if (maxHealth <= 0 || vignette == null)
            {
                return;
            }

            float healthRate = Mathf.Clamp01((float)health / maxHealth);
            float targetRate = healthRate > 0f && healthRate <= lowHealthThreshold ? 1f : 0f;

            lowHealthPostProcessHandle.TryCancel();
            lowHealthPostProcessHandle = LMotion.Create(lowHealthPostProcessRate, targetRate, lowHealthTransitionDuration)
                .WithEase(Ease.OutQuad)
                .Bind(value =>
                {
                    lowHealthPostProcessRate = value;
                    ApplyPostProcess();
                })
                .AddTo(this);
        }

        public void SetBoosting(bool isBoosting)
        {
            if (motionBlur == null)
            {
                return;
            }

            float targetRate = isBoosting ? 1f : 0f;

            boostMotionBlurHandle.TryCancel();
            boostMotionBlurHandle = LMotion.Create(boostMotionBlurRate, targetRate, boostMotionBlurTransitionDuration)
                .WithEase(Ease.OutQuad)
                .Bind(value =>
                {
                    boostMotionBlurRate = value;
                    ApplyPostProcess();
                })
                .AddTo(this);
        }

        public void ResetState()
        {
            maxHealth = 0;
            damagePostProcessHandle.TryCancel();
            lowHealthPostProcessHandle.TryCancel();
            boostMotionBlurHandle.TryCancel();
            damagePostProcessRate = 0f;
            lowHealthPostProcessRate = 0f;
            boostMotionBlurRate = 0f;
            ApplyPostProcess();
        }

        void Update()
        {
            ApplyPostProcess();
        }

        void InitializePostProcess()
        {
            postProcessVolume ??= FindFirstObjectByType<Volume>();

            if (postProcessVolume == null)
            {
                Debug.LogWarning($"{nameof(GamePostProcessView)}: Global Volume が見つからないため、ポストプロセス演出を無効化します。");
                return;
            }

            VolumeProfile profile = postProcessVolume.profile;
            vignette = GetOrAddPostProcess<Vignette>(profile);
            chromaticAberration = GetOrAddPostProcess<ChromaticAberration>(profile);
            lensDistortion = GetOrAddPostProcess<LensDistortion>(profile);
            motionBlur = GetOrAddPostProcess<MotionBlur>(profile);

            vignette.active = true;
            vignette.color.overrideState = true;
            vignette.center.overrideState = true;
            vignette.intensity.overrideState = true;
            vignette.smoothness.overrideState = true;
            vignette.rounded.overrideState = true;
            vignette.color.value = Color.red;
            vignette.center.value = new Vector2(0.5f, 0.5f);
            vignette.smoothness.value = 0.45f;
            vignette.rounded.value = false;

            chromaticAberration.active = true;
            chromaticAberration.intensity.overrideState = true;

            lensDistortion.active = true;
            lensDistortion.intensity.overrideState = true;
            lensDistortion.xMultiplier.overrideState = true;
            lensDistortion.yMultiplier.overrideState = true;
            lensDistortion.center.overrideState = true;
            lensDistortion.scale.overrideState = true;
            lensDistortion.xMultiplier.value = 1f;
            lensDistortion.yMultiplier.value = 1f;
            lensDistortion.center.value = new Vector2(0.5f, 0.5f);
            lensDistortion.scale.value = 1f;

            motionBlur.active = true;
            motionBlur.mode.overrideState = true;
            motionBlur.quality.overrideState = true;
            motionBlur.intensity.overrideState = true;
            motionBlur.clamp.overrideState = true;
            motionBlur.mode.value = MotionBlurMode.CameraOnly;
            motionBlur.quality.value = MotionBlurQuality.Low;
            motionBlur.clamp.value = boostMotionBlurClamp;
        }

        static T GetOrAddPostProcess<T>(VolumeProfile profile) where T : VolumeComponent
        {
            return profile.TryGet(out T component) ? component : profile.Add<T>(true);
        }

        void ApplyPostProcess()
        {
            if (vignette == null || chromaticAberration == null || lensDistortion == null || motionBlur == null)
            {
                return;
            }

            float pulseRate = lowHealthPostProcessRate > 0f
                ? Mathf.PingPong(Time.time * lowHealthPulseSpeed, 1f)
                : 0f;
            float lowHealthIntensity = lowHealthPostProcessRate
                * (lowHealthVignetteIntensity + lowHealthPulseIntensity * pulseRate);
            float damageVignette = damageVignetteIntensity * damagePostProcessRate;

            vignette.intensity.value = Mathf.Clamp01(Mathf.Max(lowHealthIntensity, damageVignette));
            chromaticAberration.intensity.value = Mathf.Clamp01(damageChromaticAberrationIntensity * damagePostProcessRate);
            lensDistortion.intensity.value = Mathf.Clamp(damageLensDistortionIntensity * damagePostProcessRate, -1f, 1f);
            motionBlur.intensity.value = Mathf.Clamp01(boostMotionBlurIntensity * boostMotionBlurRate);
            motionBlur.clamp.value = boostMotionBlurClamp;
        }

        void OnDestroy()
        {
            damagePostProcessHandle.TryCancel();
            lowHealthPostProcessHandle.TryCancel();
            boostMotionBlurHandle.TryCancel();
        }
    }
}
