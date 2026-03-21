using UnityEngine;

[System.Serializable]
public class StatUpgradeData
{
    public string statName;

    [Header("Limits")]
    public int maxLevel = 10;

    public enum UpgradeType
    {
        Formula,
        Custom
    }

    public UpgradeType upgradeType;

    // ===== FORMULA SETTINGS =====
    [Header("Formula Settings")]
    public int startingCost = 50;
    public float costMultiplierPercent = 0.2f; // 20%

    // ===== CUSTOM COSTS =====
    [Header("Custom Costs")]
    public int[] customCosts;

    public int GetCost(int currentLevel)
    {
        if (upgradeType == UpgradeType.Custom)
        {
            if (currentLevel < customCosts.Length)
                return customCosts[currentLevel];

            return -1;
        }

        // FORMULA:
        // current cost + starting cost * multiplier% * level

        float cost = startingCost;

        return Mathf.RoundToInt(startingCost * (1 + costMultiplierPercent * currentLevel * currentLevel));
    }
}