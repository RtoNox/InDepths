using UnityEngine;

public class OxygenSystem : MonoBehaviour
{
    public float currentOxygen;
    private SubmarineStats stats;
    private Health health;

    public float drainRate = 5f;

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
    }

    public void ResetOxygen()
    {
        currentOxygen = stats.GetMaxOxygen();
    }
}