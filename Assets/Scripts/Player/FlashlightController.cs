using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlashlightController : MonoBehaviour
{
    public Light2D light2D;
    public SubmarineStats stats;
    public float baseRange = 5f;
    public float baseIntensity = 1f;

    void Start()
    {
        if (light2D == null)
        {
            light2D = GetComponentInChildren<Light2D>();
        }
        if (stats == null)
        {
            stats = GetComponent<SubmarineStats>();
        }
    }

    void FixedUpdate()
    {
        UpdateFlashlight();
    }

    void UpdateFlashlight()
    {
        float strength = stats.GetFlashlightStrength();

        // Increase range based on upgrades
        light2D.pointLightOuterRadius = baseRange + strength * 2f;

        // Optional: increase intensity slightly
        light2D.intensity = baseIntensity + strength * 0.2f;
    }
}