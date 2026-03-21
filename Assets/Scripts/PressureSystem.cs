using UnityEngine;

public class PressureSystem : MonoBehaviour
{
    public Transform player;
    private SubmarineStats stats;
    private Health health;

    public float damageRate = 5f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        stats = GetComponent<SubmarineStats>();
        health = GetComponent<Health>();
    }

    void Update()
    {
        float depth = Mathf.Abs(player.position.y);
        float safeDepth = stats.GetMaxSafeDepth();

        if (depth > safeDepth)
        {
            float excessDepth = depth - safeDepth;

            float damage = excessDepth * 0.1f; // scales with depth
            health.TakeDamage(1 + (Mathf.FloorToInt(damage * Time.deltaTime)));
        }
    }
}