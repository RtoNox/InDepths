using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Data")]
    public int currentSaveSlot = -1;
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

    [Header("Game Over UI")]
    public GameObject gameOverScreen;
    public GameObject playerHUD;

    [Header("Debt UI")]
    public TextMeshProUGUI debtAmount;

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
        DisableAllSpawners();

        int itemCount = 0;

        if (inventory != null)
        {
            foreach (Item item in inventory.items)
            {
                itemCount++;
            }
        }

        resultsPanel.SetActive(true);
        itemsSold.text = "Items Sold: " + itemCount;
        dayCompleted.text = "Day " + currentDay + " Completed!";

        SellItems();
        moneyEarned.text = "Money Earned: $" + moneyEarnedToday;
        debtAmount.text = "DEBT\n$" + debt;

        SaveGame();
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
            debt += Mathf.RoundToInt(debt * 0.0001f);
        }

        // Reset daily tracker
        moneyEarnedToday = 0;

        EnableAllSpawners();
    }

    // === PAY DEBT ===
    public void PayDebt(int amount)
    {
        if (amount <= 0)
        {
            Debug.Log("Invalid amount!");
            return;
        }

        amount = Mathf.Min(amount, debt); // prevent overpaying

        if (currency.money >= amount)
        {
            currency.SpendMoney(amount);
            debt -= amount;

            Debug.Log("Paid: $" + amount + " Remaining debt: $" + debt);
        }
        else
        {
            Debug.Log("Not enough money!");
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

    public void EnableAllSpawners()
    {
        GameObject[] spawners = GameObject.FindGameObjectsWithTag("Spawner");
        foreach (GameObject spawner in spawners)
        {
            spawner.SetActive(true);
        }
        Debug.Log("All spawners enabled.");
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

    // === PLAYER DEATH SYSTEM ===
    public void OnPlayerDeath()
    {
        if (isBossFightActive)
        {
            TriggerBadEnding();
            return;
        }

        inventory.ResetInventory();

        Debug.Log("Player died. Restarting day " + currentDay);

        // Show Game Over UI instead of instant reload
        ShowGameOverUI();
    }

    void ShowGameOverUI()
    {
        isGameOver = true;
        gameOverScreen.SetActive(true);
        playerHUD.SetActive(false);

        Time.timeScale = 0f; // pause game
    }

    public void RetryDay()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        
        SaveGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // IMPORTANT reset
        isGameOver = false;

        SaveGame();
        SceneManager.LoadScene("MainMenu");
    }

    // === SAVE/LOAD SYSTEM ===
    public void SaveGame()
    {
        if (currentSaveSlot == -1)
        {
            Debug.Log("No save slot selected!");
            return;
        }

        SaveData data = new SaveData();

        data.currentDay = currentDay;
        data.money = currency.money;
        data.debt = debt;

        data.damageLevel = submarineStats.damageLevel;
        data.ammoLevel = submarineStats.ammoLevel;
        data.speedLevel = submarineStats.speedLevel;
        data.vitalityLevel = submarineStats.vitalityLevel;
        data.oxygenLevel = submarineStats.oxygenLevel;
        data.armStrengthLevel = submarineStats.armStrengthLevel;
        data.flashlightStrengthLevel = submarineStats.flashlightStrengthLevel;
        data.flashlightBatteryLevel = submarineStats.flashlightBatteryLevel;
        data.storageLevel = submarineStats.storageLevel;

        SaveSystem.SaveGame(data, currentSaveSlot);
    }

    public void LoadGame(int slot)
    {
        SaveData data = SaveSystem.LoadGame(slot);

        if (data == null) return;

        currentDay = data.currentDay;
        debt = data.debt;

        currency.money = data.money;

        submarineStats.damageLevel = data.damageLevel;
        submarineStats.ammoLevel = data.ammoLevel;
        submarineStats.speedLevel = data.speedLevel;
        submarineStats.vitalityLevel = data.vitalityLevel;
        submarineStats.oxygenLevel = data.oxygenLevel;
        submarineStats.armStrengthLevel = data.armStrengthLevel;
        submarineStats.flashlightStrengthLevel = data.flashlightStrengthLevel;
        submarineStats.flashlightBatteryLevel = data.flashlightBatteryLevel;
        submarineStats.storageLevel = data.storageLevel;
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