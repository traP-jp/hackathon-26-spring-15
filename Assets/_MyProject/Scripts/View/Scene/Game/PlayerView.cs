using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerView : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _boostSpeed = 6f;
    [SerializeField] private float _jumpPower = 8f;
    [SerializeField] private int _maxHp = 100;
    [SerializeField] private float _invincibleTime = 1.5f;

    public int _hp {get; private set;}

    private Rigidbody2D _rb;
    private bool _isBoost;
    private bool _isInvincible;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _hp = _maxHp;
    }

    // Update is called once per frame
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

        _rb.AddForce(Vector2.up * _jumpPower, ForceMode2D.Impulse);
    }

    // ダメージ管理
    public void Damage(int damage)
    {
        if (_isInvincible) return;

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
