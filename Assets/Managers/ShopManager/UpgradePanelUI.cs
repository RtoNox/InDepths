using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradePanelUI : MonoBehaviour
{
    public string statName;

    [Header("UI")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText;
    public Button upgradeButton;

    private UpgradeShop shop;
    private SubmarineStats stats;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        shop = FindObjectOfType<UpgradeShop>();
        stats = player.GetComponent<SubmarineStats>();

        upgradeButton.onClick.AddListener(OnUpgradeClicked);

        RefreshUI();
    }

    public void RefreshUI()
    {
        int level = GetStatLevel();
        int cost = GetCost(level);

        levelText.text = "LVL " + level;
        costText.text = cost < 0 ? "MAX" : "$" + cost;

        upgradeButton.interactable = cost >= 0;
    }

    void OnUpgradeClicked()
    {
        shop.TryUpgrade(statName);
        RefreshUI(); // update after purchase
    }

    int GetStatLevel()
    {
        switch (statName.ToLower())
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

    int GetCost(int level)
    {
        foreach (var data in shop.upgradeDataList)
        {
            if (data.statName.ToLower() == statName.ToLower())
            {
                if (level >= data.maxLevel)
                    return -1;

                return data.GetCost(level);
            }
        }

        return -1;
    }
}