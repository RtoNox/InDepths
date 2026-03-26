using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class FlashlightController : MonoBehaviour
{
    [Header("References")]
    public Light2D light2D;
    public SubmarineStats stats;

    [Header("Base Settings")]
    public float baseRange = 5f;
    public float baseIntensity = 1f;

    [Header("Battery")]
    public float drainRate = 1f; // per second
    public float currentBattery;
    public float maxBattery;

    [Header("State")]
    public bool isOn = false;

    [Header("UI")]
    public Image batteryBar;
    public Color filledColor = Color.green;
    public Color emptyColor = Color.red;

    void Start()
    {
        if (light2D == null)
            light2D = GetComponentInChildren<Light2D>();

        if (stats == null)
            stats = GetComponent<SubmarineStats>();

        light2D.enabled = false;
        maxBattery = stats.GetFlashlightBatteryCapacity();
        currentBattery = maxBattery;
    }

    void Update()
    {
        if (isOn)
        {
            DrainBattery();
        }

        UpdateFlashlight();
    }

    void DrainBattery()
    {
        if (currentBattery > 0)
        {
            currentBattery -= drainRate * Time.deltaTime;

            if (currentBattery <= 0)
            {
                currentBattery = 0;
                TurnOff();
            }
        }
    }

    void UpdateFlashlight()
    {
        if (light2D == null) return;

        float strength = stats.GetFlashlightStrength();
        float batteryPercent = currentBattery / maxBattery;
        batteryBar.fillAmount = batteryPercent;
        batteryBar.color = Color.Lerp(emptyColor, filledColor, batteryPercent);

        // Range scales with upgrades
        light2D.pointLightOuterRadius = baseRange + strength * 2f;

        // Base intensity from upgrades
        float targetIntensity = baseIntensity + strength * 0.2f;

        if (batteryPercent > 0.2f)
        {
            light2D.intensity = targetIntensity;
        }
        else
        {
            // Dim when low battery
            float dimmed = Mathf.Lerp(0.2f, targetIntensity, batteryPercent / 0.2f);

            // Light flicker
            float flicker = Random.Range(0.6f, 1.1f);

            light2D.intensity = dimmed * flicker;
        }
    }

    // === PUBLIC CONTROLS ===

    public void Toggle()
    {
        if (currentBattery <= 0) return;

        isOn = !isOn;
        light2D.enabled = isOn;
    }

    public void TurnOff()
    {
        isOn = false;
        light2D.enabled = false;
    }

    public void RefillBattery()
    {
        currentBattery = maxBattery;
        Debug.Log("Battery refilled!");
    }

    public float GetBatteryPercent()
    {
        return currentBattery / maxBattery;
    }
}