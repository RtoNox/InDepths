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

    public float damageRate = 1f; // damage tick rate when out of oxygen
    private float tickTimer;

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
        if (player.position.y < -0.2f) // only drain oxygen underwater
            currentOxygen -= drainRate * Time.deltaTime;

        float fill = currentOxygen / maxOxygen;
        oxygenBar.fillAmount = fill;

        if (currentOxygen <= 0)
        {
            tickTimer -= Time.deltaTime;

            if (tickTimer <= 0f)
            {
                health.TakeDamage(1);
                tickTimer = damageRate;
            }
        }

        if (player.position.y >= -0.2f) // above water
        {
            if (currentOxygen < maxOxygen)
            {
                currentOxygen += 0.2f; // replenish oxygen above water
                Mathf.Clamp(currentOxygen, 0, maxOxygen);
            }
        }
    }

    public void ResetOxygen()
    {
        maxOxygen = stats.GetMaxOxygen();
        currentOxygen = maxOxygen;
    }
}