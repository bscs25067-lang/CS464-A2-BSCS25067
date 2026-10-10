using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;

    private Rigidbody _rb;
    private bool _isGrounded;
    private bool _jumpBuffer;
    private Vector2 _input;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        _input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        _input = Vector2.Normalize(_input);

        if (_isGrounded && Input.GetKey(KeyCode.Space))
            _jumpBuffer = true;
    }

    private void FixedUpdate()
    {
        Vector3 movement = moveSpeed * new Vector3(_input.x, 0, _input.y);
        _rb.linearVelocity = new Vector3(movement.x, _rb.linearVelocity.y, movement.z);

        if(_jumpBuffer)
        {
            _jumpBuffer = false;
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
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
