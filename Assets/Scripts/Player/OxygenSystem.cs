using UnityEngine;
using UnityEngine.UI;

public class OxygenSystem : MonoBehaviour
{
    public Transform player;
    private SubmarineStats stats;
    private Health health;

    [Header("Oxygen Settings")]
    public float currentOxygen;
    private float maxOxygen;
    public float drainRate = 1f;

    [Header("UI")]
    public Image oxygenBar;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        stats = GetComponent<SubmarineStats>();
        health = GetComponent<Health>();
        ResetOxygen();
    }

    void Update()
    {
        currentOxygen -= drainRate * Time.deltaTime;

        float fill = (float)currentOxygen / maxOxygen;
        oxygenBar.fillAmount = fill;

        if (currentOxygen <= 0)
        {
            health.TakeDamage(1); // suffocation damage
        }

        if (player.position.y >= 0) // above water
        {
            ResetOxygen();
        }
    }

    public void ResetOxygen()
    {
        maxOxygen = stats.GetMaxOxygen();
        currentOxygen = maxOxygen;
    }
}