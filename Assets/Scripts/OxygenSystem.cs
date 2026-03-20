using UnityEngine;

public class OxygenSystem : MonoBehaviour
{
    public Transform player;
    public float currentOxygen;
    private SubmarineStats stats;
    private Health health;

    public float drainRate = 1f;

    void Start()
    {
        stats = GetComponent<SubmarineStats>();
        health = GetComponent<Health>();

        currentOxygen = stats.GetMaxOxygen();
    }

    void Update()
    {
        currentOxygen -= drainRate * Time.deltaTime;

        if (currentOxygen <= 0)
        {
            health.TakeDamage(1); // suffocation damage
        }

        if (player.position.y >= 0) // above water
        {
            currentOxygen = stats.GetMaxOxygen();
        }
    }

    public void ResetOxygen()
    {
        currentOxygen = stats.GetMaxOxygen();
    }
}