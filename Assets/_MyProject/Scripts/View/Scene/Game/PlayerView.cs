using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyProject.View
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerView : ViewBase
    {
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _boostSpeed = 6f;
        [SerializeField] private float _jumpHeight = 2f;
        [SerializeField] private int _maxHp = 100;
        [SerializeField] private float _invincibleTime = 1.5f;

        public int _hp {get; private set;}

        private Rigidbody2D _rb;
        private bool _isBoost;
        private bool _isInvincible;

        public override void Initialize()
        {
            _rb = GetComponent<Rigidbody2D>();
            _hp = _maxHp;
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

            StartCoroutine(InvincibleCoroutine());
        }

        // 無敵時間管理
        private IEnumerator InvincibleCoroutine()
        {
            _isInvincible = true;

            yield return new WaitForSeconds(_invincibleTime);

            _isInvincible = false;
        }
    }
}
