using UnityEngine;
using TMPro;
using JetBrains.Annotations;
using UnityEngine.UIElements;

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

    [Header("Day Results UI")]
    public TextMeshProUGUI dayCompleted;
    public TextMeshProUGUI moneyEarned;
    public TextMeshProUGUI itemsSold;
    public GameObject resultsPanel;

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
        CheckWinCondition();

        int itemCount = 0;

        foreach (Item item in inventory.items)
        {
            itemCount++;
        }

        dayCompleted.text = "Day " + currentDay + " Completed!";
        moneyEarned.text = "Money Earned: $" + moneyEarnedToday;
        itemsSold.text = "Items Sold: " + itemCount;

        SellItems();
    }

    public void CloseResultsUI()
    {
        resultsPanel.SetActive(false);
    }

    public void StartNewDay()
    {
        dayCompleted.text = "";
        moneyEarned.text = "";
        currentDay++;

        // Reset daily tracker
        moneyEarnedToday = 0;
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
        inventory.ResetInventory();

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