using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float fallMultiplier;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBuffer = 0.2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask groundMask;

    [Header("Juice")]
    [SerializeField] private Transform body;
    [SerializeField] private ParticleSystem jumpFx;
    [SerializeField] private float squashDuration = 0.25f;
    [SerializeField] private float squashY = 0.7f;
    [SerializeField] private float minAirTime = 0.1f;

    private Rigidbody _rb;
    private Vector2 _input;
    private float _airTime;
    private Coroutine _squashRoutine;
    private Vector3 _bodyRestPos;

    private bool _isGrounded, _wasGrounded;
    private bool _jumpQueue;

    private float _lastPressedJumpTime;
    private float _lastGroundedTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _bodyRestPos = body.localPosition;
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

        if (!_isGrounded) _airTime += Time.deltaTime;
        else
        {
            if (!_wasGrounded && _airTime > minAirTime) StartSquash();
            _airTime = 0f;
        }
        _wasGrounded = _isGrounded;
    }

    private void FixedUpdate()
    {
        // Ground Check
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        // Applying Body Movement
        Vector3 movement = moveSpeed * new Vector3(_input.x, 0, _input.y);
        _rb.linearVelocity = new Vector3(movement.x, _rb.linearVelocity.y, movement.z);

        if(_jumpQueue)
        {
            jumpFx.Play();
            _jumpQueue = false;
            float gravity = -Physics.gravity.y;
            float v0 = Mathf.Sqrt(2 * gravity * jumpHeight);

            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            _rb.AddForce(Vector3.up * _rb.mass * v0, ForceMode.Impulse);
        }

        if (_rb.linearVelocity.y < 0f)
            _rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);
    }

    private void StartSquash()
    {
        if (_squashRoutine != null) StopCoroutine(_squashRoutine);

        _squashRoutine = StartCoroutine(Squash());
    }

    private IEnumerator Squash()
    {
        float t = 0f;
        while (t < squashDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / squashDuration);
            float eased = EaseOut(progress);
            float y = Mathf.Lerp(squashY, 1f, eased);
            float xz = 1f / Mathf.Sqrt(y);

            body.localScale = new Vector3(xz, y, xz);
            yield return null;
        }

        body.localScale = Vector3.one;
    }

    private static float EaseOut(float p)
    {
        return 1f - (1f - p) * (1f - p);
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
