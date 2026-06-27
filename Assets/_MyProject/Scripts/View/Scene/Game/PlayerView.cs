using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerView : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _boostSpeed = 6f;
    [SerializeField] private float _jumpPower = 8f;

    private Rigidbody2D _rb;
    private bool _isBoost;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
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

    public void OnBoost(InputAction.CallbackContext context)
    {
        _isBoost = context.ReadValueAsButton();
    }

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
}
