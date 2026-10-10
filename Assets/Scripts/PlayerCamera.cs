using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothing;
    [SerializeField] private Vector3 offset;

    private void LateUpdate()
    {
        if (target == null)
        {
            enabled = false;
            Debug.LogWarning("Target is empty!");
            return;
        }

        Vector3 wanted = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, wanted, smoothing * Time.deltaTime);

        transform.LookAt(target);
    }
}
