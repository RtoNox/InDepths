using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Camera Settings")]
    public float maxDistance = 3f;

    [Header("Mouse Smooth")]
    public float offsetSmoothSpeed = 1f;

    private Camera cam;
    private Vector2 currentOffset;

    void Awake()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Mouse world position
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        // Direction from player mouse
        Vector2 targetOffset = mouseWorld - player.position;

        // Clamp to circular boundary
        targetOffset = Vector2.ClampMagnitude(targetOffset, maxDistance);

        // Smooth ONLY the offset
        currentOffset = Vector2.Lerp(
            currentOffset,
            targetOffset,
            offsetSmoothSpeed * Time.deltaTime
        );

        // Camera position is set to player position
        Vector3 finalPosition = player.position + (Vector3)currentOffset;
        finalPosition.z = -10f;

        transform.position = finalPosition;
    }
}