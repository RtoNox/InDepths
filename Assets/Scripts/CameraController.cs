using UnityEngine;
using UnityEngine.Rendering.Universal;

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

    [Header("Depth Darkness Settings")]
    public Light2D globalLight; // your "Darkness"

    public float surfaceY = 0f;
    public float maxDepthY = -100f;

    [Header("Colors")]
    public Color surfaceColor = new Color(0.2f, 0.6f, 0.8f); // light blue
    public Color deepColor = Color.black;

    public Color surfaceLightColor = Color.white;
    public Color deepLightColor = new Color(0.05f, 0.05f, 0.1f);

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

        UpdateDepthDarkness(); // Gradually desaturate colors based on depth
    }

    void UpdateDepthDarkness()
    {
        if (player == null || cam == null) return;

        float playerY = player.position.y;

        // Normalize depth (0 = surface, 1 = deepest)
        float t = Mathf.InverseLerp(surfaceY, maxDepthY, playerY);

        // Clamp just in case
        t = Mathf.Clamp01(t);

        // Camera background color
        cam.backgroundColor = Color.Lerp(surfaceColor, deepColor, t);

        // Global light color
        if (globalLight != null)
        {
            globalLight.color = Color.Lerp(surfaceLightColor, deepLightColor, t);
        }
    }
}