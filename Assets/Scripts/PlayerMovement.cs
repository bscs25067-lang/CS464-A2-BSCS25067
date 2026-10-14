using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float fallMultiplier;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBuffer = 0.2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask groundMask;

    private Rigidbody _rb;
    private Vector2 _input;

    private bool _isGrounded;
    private bool _jumpQueue;

    private float _lastPressedJumpTime;
    private float _lastGroundedTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Move Input
        _input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        _input = Vector2.ClampMagnitude(_input, 1);

        // Jump Input & Buffering
        if(Input.GetKeyDown(KeyCode.Space)) _lastPressedJumpTime = Time.time;
        bool wantJump = Time.time - _lastPressedJumpTime <= jumpBuffer;

        // Coyote Jumping
        if (_isGrounded && !_jumpQueue && _rb.linearVelocity.y <= 0f) _lastGroundedTime = Time.time;
        bool canJump = Time.time - _lastGroundedTime <= coyoteTime;

        if(canJump && wantJump && !_jumpQueue)
        {
            _jumpQueue = true;
            _lastGroundedTime = -1f;
            _lastPressedJumpTime = -1f;
        }

        // Ground Check
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
    }

    private void FixedUpdate()
    {
        Vector3 movement = moveSpeed * new Vector3(_input.x, 0, _input.y);
        _rb.linearVelocity = new Vector3(movement.x, _rb.linearVelocity.y, movement.z);

        if(_jumpQueue)
        {
            _jumpQueue = false;
            float gravity = -Physics.gravity.y;
            float v0 = Mathf.Sqrt(2 * gravity * jumpForce);

            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.y);
            _rb.AddForce(Vector3.up * jumpForce * _rb.mass * v0, ForceMode.Impulse);
        }

        if (_rb.linearVelocity.y < 0f)
            _rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);
    }

    private void OnDrawGizmos()
    {
        if (groundCheck == null) return;

        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.GetContact(0).normal.y > 0.5f)
            _isGrounded = true;
    }
    private void OnCollisionExit(Collision collision)
    {
        _isGrounded = false;
    }
}
