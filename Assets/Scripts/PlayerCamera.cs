using System;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothing;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float lookahead = 1f;

    Rigidbody _targetRb;
    Vector3 _aim;

    private void Start()
    {
        if (target == null)
        {
            enabled = false;
            Debug.LogWarning("Target is empty!");
            return;
        }

        _targetRb = target.GetComponent<Rigidbody>();
        _aim = target.position;

        transform.position = _aim + offset;
        transform.LookAt(_aim);
    }

    private void LateUpdate()
    {
        Vector3 vel = _targetRb.linearVelocity;
        vel.y = 0;
        Vector3 wantedAim = target.position + (vel * lookahead);

        float blend = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
        _aim = Vector3.Lerp(_aim, wantedAim, blend);

        transform.position = _aim + offset;
        transform.LookAt(_aim);
    }
}
