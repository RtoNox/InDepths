using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Day System")]
    public int currentDay = 1;

    [Header("Player Data")]
    private Inventory inventory;
    private PlayerCurrency currency;
    public int debt = 10000000;

    [Header("Run Data")]
    public int moneyEarnedToday = 0;

    [Header("Game State")]
    public bool isGameOver = false;
    public bool hasWon = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Initialize(Inventory inv, PlayerCurrency curr)
    {
        inventory = inv;
        currency = curr;
    }

    public void SellItems()
    {
        int totalValue = 0;

        foreach (Item item in inventory.items)
        {
            totalValue += item.value;
        }

        moneyEarnedToday = totalValue;
        currency.AddMoney(totalValue);
        inventory.ResetInventory();

        Debug.Log("Sold everything for: " + totalValue);
    }

    // === DAY PROGRESSION ===
    public void EndDay()
    {
        SellItems();
        currentDay++;

        // Reset daily tracker
        moneyEarnedToday = 0;

        CheckWinCondition();
    }

    // === PAY DEBT ===
    public void PayDebt(int amount)
    {
        if (currency.money >= amount)
        {
            currency.SpendMoney(amount);
            debt -= amount;
        }

        CheckWinCondition();
    }

    // === DEATH SYSTEM ===
    public void OnPlayerDeath()
    {
        // Lose all progress for the day
        inventory.ResetInventory();

        // Reset player/inventory handled elsewhere
        Debug.Log("Player died. Restarting day " + currentDay);
    }

    // === WIN CONDITION ===
    void CheckWinCondition()
    {
        if (debt <= 0)
        {
            hasWon = true;
            Debug.Log("You Win!");
        }
    }

    // === TRUE ENDING TRIGGER ===
    public void TrueEnding()
    {
        debt = 0;
        hasWon = true;

        Debug.Log("True Ending Achieved!");
    }
}