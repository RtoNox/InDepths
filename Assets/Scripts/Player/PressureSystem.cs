using UnityEngine;
using UnityEngine.UI;

public class PressureSystem : MonoBehaviour
{
    public Transform player;
    private SubmarineStats stats;
    private Health health;

    [Header("Damage Settings")]
    public float baseTickInterval = 1f; // 1 damage per second at minimum
    public float minTickInterval = 0.2f; // cap so it doesn't go insane
    public float depthScaling = 0.02f; // how fast tick speeds up
    private float tickTimer;

    [Header("UI")]
    public Image depthBar;
    public Image pressureBar;
    public Color filledColor;
    public Color emptyColor;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        stats = GetComponent<SubmarineStats>();
        health = GetComponent<Health>();

        tickTimer = baseTickInterval;
    }

    void Update()
    {
        float depth = Mathf.Abs(player.position.y);
        float safeDepth = stats.GetMaxSafeDepth();

        float depthFill = 1f - (depth / safeDepth);
        depthBar.fillAmount = depthFill;
        depthBar.color = Color.Lerp(emptyColor, filledColor, depthFill);

        if (depth > safeDepth)
        {
            float excessDepth = depth - safeDepth;
            float maxPressureDepth = (baseTickInterval - minTickInterval) / depthScaling; // depth at which tick rate hits minimum

            float pressureFill =  excessDepth / maxPressureDepth;
            pressureBar.fillAmount = pressureFill;

            // Faster damage tick rate the deeper you go
            float tickInterval = baseTickInterval - (excessDepth * depthScaling);

            // Clamp so it doesn't become ridiculous
            tickInterval = Mathf.Clamp(tickInterval, minTickInterval, baseTickInterval);

            tickTimer -= Time.deltaTime;

            if (tickTimer <= 0f)
            {
                health.TakeDamage(1);
                tickTimer = tickInterval;
            }
        }
        else
        {
            // Reset timer when safe
            tickTimer = baseTickInterval;
        }
    }
}