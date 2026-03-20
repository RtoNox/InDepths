using UnityEngine;

public class UpgradeShop : MonoBehaviour
{
    private PlayerCurrency currency;
    private SubmarineStats stats;

    public int baseUpgradeCost = 50;

    void Awake()
    {
        currency = GetComponent<PlayerCurrency>();
        stats = GetComponent<SubmarineStats>();
    }

    // === BUTTON FUNCTIONS ===

    public void UpgradeDamage()
    {
        TryUpgrade("damage", stats.damageLevel);
    }

    public void UpgradeAmmo()
    {
        TryUpgrade("ammo", stats.ammoLevel);
    }

    public void UpgradeSpeed()
    {
        TryUpgrade("speed", stats.speedLevel);
    }

    public void UpgradeVitality()
    {
        TryUpgrade("vitality", stats.vitalityLevel);
    }

    public void UpgradeOxygen()
    {
        TryUpgrade("oxygen", stats.oxygenLevel);
    }

    public void UpgradeArmStrength()
    {
        TryUpgrade("armstrength", stats.armStrengthLevel);
    }

    public void UpgradeFlashlightStrength()
    {
        TryUpgrade("flashlightstrength", stats.flashlightStrengthLevel);
    }

    public void UpgradeFlashlightBattery()
    {
        TryUpgrade("flashlightbattery", stats.flashlightBatteryLevel);
    }

    public void UpgradeStorage()
    {
        TryUpgrade("storage", stats.storageLevel);
    }

    // === CORE LOGIC ===

    void TryUpgrade(string stat, int currentLevel)
    {
        int cost = GetUpgradeCost(currentLevel);

        if (currency.SpendMoney(cost))
        {
            stats.UpgradeStat(stat);
            Debug.Log(stat + " upgraded!");
        }
        else
        {
            Debug.Log("Not enough money!");
        }
    }

    int GetUpgradeCost(int level)
    {
        return baseUpgradeCost * (level + 1); // scaling cost
    }
}