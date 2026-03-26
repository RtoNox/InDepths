using UnityEngine;

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

        if (depth > safeDepth)
        {
            float excessDepth = depth - safeDepth;

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