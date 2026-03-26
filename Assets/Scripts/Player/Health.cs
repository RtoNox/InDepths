using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth;
    public int currentHealth;

    private PlayerController playerController;
    private SubmarineStats stats;

    [Header("UI")]
    public Image healthBar;

    void Start()
    {
        playerController = GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError("PlayerController not found on Player.");
        }

        stats = GetComponent<SubmarineStats>();

        if (stats == null)
        {
            Debug.LogError("SubmarineStats not found on Player.");
        }

        UpdateMaxHealth();
    }

    void Update()
    {
        float fill = (float)currentHealth / maxHealth;
        healthBar.fillAmount = fill;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.name + " took " + amount + " damage! Current health: " + currentHealth);

        playerController.CancelResurfacer();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void UpdateMaxHealth()
    {
        int newMax = stats.GetMaxHealth();
        maxHealth = newMax;

        Debug.Log("Max Health updated to: " + maxHealth);
        ResetHealth(); // Reset to new max health
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        Debug.Log(gameObject.name + " health reset to max: " + maxHealth);  
    }

    void Die()
    {
        GameManager.Instance.OnPlayerDeath();
        Debug.Log("Player died!");
    }
}