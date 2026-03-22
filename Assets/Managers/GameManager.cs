using UnityEngine;
using TMPro;
using JetBrains.Annotations;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Data")]
    public int currentDay = 1;
    public Inventory inventory;
    public PlayerCurrency currency;
    public SubmarineStats submarineStats;
    public int debt = 10000000;
    public int moneyEarnedToday = 0;

    [Header("Game State")]
    public bool isGameOver = false;
    public bool hasWon = false;

    [Header("Boss Settings")]
    public bool isBossFightActive = false;
    public GameObject bossPrefab;
    private GameObject bossInstance;
    public bool isBossAlive = false;

    public Vector2 bossSpawnMin;
    public Vector2 bossSpawnMax;

    [Header("Day Results UI")]
    public TextMeshProUGUI dayCompleted;
    public TextMeshProUGUI moneyEarned;
    public TextMeshProUGUI itemsSold;
    public GameObject resultsPanel;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(Inventory inv, PlayerCurrency curr, SubmarineStats stats)
    {
        inventory = inv;
        currency = curr;
        submarineStats = stats;
    }

    public void SellItems()
    {
        int totalValue = 0;

        if (inventory != null)
        {
            foreach (Item item in inventory.items)
            {
                totalValue += item.value;
            }
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
        DespawnAllEntities();

        int itemCount = 0;

        if (inventory != null)
        {
            foreach (Item item in inventory.items)
            {
                itemCount++;
            }
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

        if (currentDay % 5 == 0)
        {
            float previousDebt = debt;
            debt = Mathf.RoundToInt(debt * 1.0001f);
        }

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

    // === DESPAWN SYSTEM ===
    public void DespawnAllEntities()
    {
        DespawnObject[] entities = FindObjectsOfType<DespawnObject>();

        foreach (DespawnObject obj in entities)
        {
            Destroy(obj.gameObject);
        }

        Debug.Log("All entities have been despawned.");
    }

    public void DisableAllSpawners()
    {
        GameObject[] spawners = GameObject.FindGameObjectsWithTag("Spawner");

        foreach (GameObject spawner in spawners)
        {
            spawner.SetActive(false);
        }

        Debug.Log("All spawners disabled.");
    }

    // === BOSS FIGHT SYSTEM ===
    public void StartBossFight()
    {
        if (isBossFightActive) return;

        isBossFightActive = true;

        Debug.Log("Boss Fight Started!");

        // 1. Despawn all enemies
        DespawnAllEntities();

        // 2. Disable all spawners
        DisableAllSpawners();

        // 3. Spawn the boss
        SpawnBoss();
    }

    public void SpawnBoss()
    {
        float randomX = Random.Range(bossSpawnMin.x, bossSpawnMax.x);
        float randomY = Random.Range(bossSpawnMin.y, bossSpawnMax.y);

        Vector2 spawnPos = new Vector2(randomX, randomY);

        bossInstance = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

        isBossAlive = true;

        Debug.Log("Boss spawned at: " + spawnPos);
    }

    public void OnBossDefeated()
    {
        isBossAlive = false;
        isBossFightActive = false;

        Debug.Log("Boss defeated!");

        EndBossFight();
    }

    public void EndBossFight()
    {
        Debug.Log("Boss fight ended. Player can leave.");
    }

    // === DEATH SYSTEM ===
    public void OnPlayerDeath()
    {
        if (isBossFightActive)
        {
            TriggerBadEnding();
            return;
        }

        inventory.ResetInventory();
        DespawnAllEntities();

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

    // === BAD ENDING TRIGGER ===
    public void TriggerBadEnding()
    {
        isGameOver = true;

        Debug.Log("Bad Ending...");
    }

    // === TRUE ENDING TRIGGER ===
    public void TrueEnding()
    {
        debt = 0;
        hasWon = true;

        Debug.Log("True Ending Achieved!");
    }
}