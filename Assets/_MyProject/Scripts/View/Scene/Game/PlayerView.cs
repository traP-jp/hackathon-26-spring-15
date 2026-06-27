using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyProject.View
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerView : ViewBase
    {
        public Observable<Unit> Damaged => damaged;
        readonly Subject<Unit> damaged = new();

        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _boostSpeed = 6f;
        [SerializeField] private float _jumpHeight = 2f;
        [SerializeField] private int _maxHp = 100;
        [SerializeField, Min(0.01f)] private float _invincibleTime = 1.5f;
        [SerializeField, Min(0.01f)] private float _damageFlashDuration = 0.12f;
        [SerializeField, Range(0f, 1f)] private float _invincibleDarkAmount = 0.45f;
        [SerializeField, Min(0.01f)] private float _invincibleBlinkInterval = 0.08f;

        public int _hp {get; private set;}

        private Rigidbody2D _rb;
        private PlayerInput _playerInput;
        private SpriteRenderer[] _spriteRenderers = Array.Empty<SpriteRenderer>();
        private Color[] _baseSpriteColors = Array.Empty<Color>();
        private CancellationTokenSource _invincibleCts;
        private MotionHandle _invincibleVisualHandle;
        private bool _isBoost;
        private bool _isInvincible;

        public override void Initialize()
        {
            _rb = GetComponent<Rigidbody2D>();
            _playerInput = GetComponent<PlayerInput>();
            CacheSpriteColors();
            _hp = _maxHp;
            SetInputEnabled(false);
            gameObject.SetActive(false);
        }

        public override void Show()
        {
            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            CancelInvincible();
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

        public void SetInputEnabled(bool enabled)
        {
            _playerInput ??= GetComponent<PlayerInput>();

            if (enabled)
            {
                _playerInput.enabled = true;
                _playerInput.ActivateInput();
                return;
            }

            _isBoost = false;
            _playerInput.DeactivateInput();
            _playerInput.enabled = false;
        }

        void Update()
        {
            float xVelocity = _moveSpeed;

            if(_isBoost)
            {
                xVelocity = _boostSpeed;
            }

            _rb.linearVelocity = new Vector2(xVelocity, _rb.linearVelocity.y);
        }

        // 加速
        public void OnBoost(InputAction.CallbackContext context)
        {
            _isBoost = context.ReadValueAsButton();
        }

        // ジャンプ
        public void OnJump(InputAction.CallbackContext context)
        {
            if(!context.performed) return;

            bool isGround = Physics2D.Raycast(
                (Vector2)transform.position + Vector2.down * 0.51f,
                Vector2.down,
                0.05f);

            if(!isGround) return;

            float jumpSpeed = Mathf.Sqrt(2f * Mathf.Abs(Physics2D.gravity.y * _rb.gravityScale) * _jumpHeight);
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpSpeed);
        }

        // ダメージ管理
        public void Damage(int damage, GimmickType type)
        {
            if (_isInvincible) return;

            // ダッシュしていない時に当たる
            if(type == GimmickType.OnlyWhenNotDashing && _isBoost) return;

            // ダッシュ中に当たる
            if(type == GimmickType.OnlyWhenDashing && !_isBoost) return;

            _hp -= damage;
            _hp = Mathf.Clamp(_hp, 0, _maxHp);

            damaged.OnNext(Unit.Default);
            StartInvincible();
        }

        // 無敵時間管理
        private void StartInvincible()
        {
            CancelInvincible();

            _invincibleCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            InvincibleAsync(_invincibleCts).Forget();
        }

        private async UniTaskVoid InvincibleAsync(CancellationTokenSource cts)
        {
            _isInvincible = true;

            try
            {
                PlayInvincibleVisual();
                await UniTask.Delay(TimeSpan.FromSeconds(_invincibleTime), cancellationToken: cts.Token);
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
            }
            finally
            {
                if (_invincibleCts == cts)
                {
                    _invincibleCts = null;
                    _isInvincible = false;
                    _invincibleVisualHandle.TryCancel();
                    ResetSpriteColors();
                }

                cts.Dispose();
            }
        }

        private void PlayInvincibleVisual()
        {
            _invincibleVisualHandle.TryCancel();

            _invincibleVisualHandle = LMotion.Create(0f, _invincibleTime, _invincibleTime)
                .WithEase(Ease.Linear)
                .Bind(ApplyInvincibleVisual)
                .AddTo(this);
        }

        private void ApplyInvincibleVisual(float elapsed)
        {
            var flashRate = Mathf.Clamp01(1f - elapsed / _damageFlashDuration);
            var blinkRate = Mathf.PingPong(elapsed / _invincibleBlinkInterval, 1f);
            var darkRate = Mathf.Max(flashRate, blinkRate) * _invincibleDarkAmount;

            for (var i = 0; i < _spriteRenderers.Length; i++)
            {
                var baseColor = _baseSpriteColors[i];
                var color = Color.Lerp(baseColor, Color.black, darkRate);
                color.a = baseColor.a;
                _spriteRenderers[i].color = color;
            }
        }

        private void CancelInvincible()
        {
            _invincibleCts?.Cancel();
            _invincibleCts = null;

            _isInvincible = false;
            _invincibleVisualHandle.TryCancel();
            ResetSpriteColors();
        }

        private void CacheSpriteColors()
        {
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            _baseSpriteColors = new Color[_spriteRenderers.Length];

            for (var i = 0; i < _spriteRenderers.Length; i++)
            {
                _baseSpriteColors[i] = _spriteRenderers[i].color;
            }
        }

        private void ResetSpriteColors()
        {
            for (var i = 0; i < _spriteRenderers.Length; i++)
            {
                _spriteRenderers[i].color = _baseSpriteColors[i];
            }
        }

        void OnDestroy()
        {
            CancelInvincible();
            damaged.OnCompleted();
            damaged.Dispose();
        }
    }
}
