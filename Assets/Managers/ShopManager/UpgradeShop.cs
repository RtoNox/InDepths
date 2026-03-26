using UnityEngine;
using System.Collections.Generic;

public class UpgradeShop : MonoBehaviour
{
    [Header("Player Data")]
    public PlayerCurrency currency;
    public SubmarineStats stats;
    public Health health;

    [Header("Upgrade Data")]
    public List<StatUpgradeData> upgradeDataList;

    private Dictionary<string, StatUpgradeData> upgradeDataDict;

    void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        currency = player.GetComponent<PlayerCurrency>();
        stats = player.GetComponent<SubmarineStats>();
        health = player.GetComponent<Health>();

        upgradeDataDict = new Dictionary<string, StatUpgradeData>();

        foreach (var data in upgradeDataList)
        {
            upgradeDataDict[data.statName.ToLower()] = data;
        }
    }

    // === BUTTON FUNCTIONS ===

    public void UpgradeDamage() => TryUpgrade("damage");
    public void UpgradeAmmo() => TryUpgrade("ammo");
    public void UpgradeSpeed() => TryUpgrade("speed");
    public void UpgradeVitality() => TryUpgrade("vitality");
    public void UpgradeOxygen() => TryUpgrade("oxygen");
    public void UpgradeArmStrength() => TryUpgrade("armstrength");
    public void UpgradeFlashlightStrength() => TryUpgrade("flashlightstrength");
    public void UpgradeFlashlightBattery() => TryUpgrade("flashlightbattery");
    public void UpgradeStorage() => TryUpgrade("storage");

    // === CORE LOGIC ===

    public void TryUpgrade(string stat)
    {
        stat = stat.ToLower();

        if (!upgradeDataDict.ContainsKey(stat))
        {
            Debug.LogWarning("No upgrade data for: " + stat);
            return;
        }

        StatUpgradeData data = upgradeDataDict[stat];
        int currentLevel = GetStatLevel(stat);

        // Max level check
        if (currentLevel >= data.maxLevel)
        {
            Debug.Log(stat + " is MAX level!");
            return;
        }

        int cost = data.GetCost(currentLevel);

        if (currency.SpendMoney(cost))
        {
            stats.UpgradeStat(stat);
            Debug.Log(stat + " upgraded! Cost: " + cost);

            if (stat == "vitality")
            {
                health.UpdateMaxHealth();
            }
        }
        else
        {
            Debug.Log("Not enough money!");
        }
    }

    int GetStatLevel(string stat)
    {
        switch (stat)
        {
            case "damage": return stats.damageLevel;
            case "ammo": return stats.ammoLevel;
            case "speed": return stats.speedLevel;
            case "vitality": return stats.vitalityLevel;
            case "oxygen": return stats.oxygenLevel;
            case "armstrength": return stats.armStrengthLevel;
            case "flashlightstrength": return stats.flashlightStrengthLevel;
            case "flashlightbattery": return stats.flashlightBatteryLevel;
            case "storage": return stats.storageLevel;

            default: return 0;
        }
    }
}