using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Isometric Settings")]
    public float height = 10f;
    public float distance = 16f;
    public float pitchAngle = 30f;
    public float smoothSpeed = 6f;

    [Header("Bounds (optional)")]
    public bool useBounds = false;
    public Vector2 minBounds, maxBounds;

    Vector3 offset;
    Quaternion fixedRotation;

    void Awake()
    {
        // Classic isometric: 45 degrees down, rotated 45 on Y
        fixedRotation = Quaternion.Euler(pitchAngle, 45f, 0f);
        offset = fixedRotation * new Vector3(0f, 0f, -distance);
        offset.y = height;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;

        if (useBounds)
        {
            desired.x = Mathf.Clamp(desired.x, minBounds.x, maxBounds.x);
            desired.z = Mathf.Clamp(desired.z, minBounds.y, maxBounds.y);
        }

        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        transform.rotation = fixedRotation;
    }

    public void SnapToTarget()
    {
        if (target == null) return;
        transform.position = target.position + offset;
        transform.rotation = fixedRotation;
    }
}