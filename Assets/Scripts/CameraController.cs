using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Camera Settings")]
    public float maxDistance = 2f;

    [Header("Smooth Settings")]
    public float baseSmoothSpeed = 1f;
    public float maxSmoothSpeed = 3f;

    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Mouse position in world
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 direction = mouseWorld - player.position;
        float distance = direction.magnitude;

        // Clamp to circular boundary
        Vector2 clampedOffset = Vector2.ClampMagnitude(direction, maxDistance);

        // Final camera position
        Vector3 targetPosition = player.position + (Vector3)clampedOffset;
        targetPosition.z = -10f; // keep camera at correct depth

        float t = distance / maxDistance;
        float currentSmoothSpeed = Mathf.Lerp(baseSmoothSpeed, maxSmoothSpeed, t);

        // Smooth movement
        transform.position = Vector3.Lerp(transform.position, targetPosition, currentSmoothSpeed * Time.deltaTime);
    }
}