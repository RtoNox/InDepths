using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    public PlayerController playerController;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerController component not found on Player.");
            }
        }
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

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        Debug.Log(gameObject.name + " health reset to max: " + maxHealth);  
    }

    void Die()
    {
        Debug.Log("Player died!");
    }
}