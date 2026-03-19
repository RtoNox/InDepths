using UnityEngine;

public class SubmarineStats : MonoBehaviour
{
    [Header("Levels")]
    public int damageLevel = 0;
    public int ammoLevel = 0;
    public int speedLevel = 0;
    public int vitalityLevel = 0;
    public int oxygenLevel = 0;
    public int armStrengthLevel = 0;
    public int flashlightStrengthLevel = 0;
    public int flashlightBatteryLevel = 0;
    public int storageLevel = 0; // increases inventory capacity

    [Header("Base Values")]
    public int baseDamage = 20;
    public int baseAmmo = 10;
    public float baseSpeed = 5f;
    public int baseHealth = 100;
    public float baseOxygen = 100f;
    public float baseFlashlightBattery = 100f;
    public int baseStorage = 0; // base inventory slots

    // === CALCULATED STATS ===

    public int GetDamage()
    {
        return baseDamage + (damageLevel / 2);
    }

    public int GetMaxAmmo()
    {
        return baseAmmo + ammoLevel; // +1 per level
    }

    public float GetSpeed()
    {
        return baseSpeed + (speedLevel * 0.5f);
    }

    public int GetMaxHealth()
    {
        return baseHealth + (vitalityLevel * 20);
    }

    public float GetMaxOxygen()
    {
        return baseOxygen + (oxygenLevel * 25f);
    }

    public float GetMaxSafeDepth()
    {
        // Pressure resistance system
        return vitalityLevel * 50f; // example scaling
    }

    public float GetArmStrength()
    {
        return armStrengthLevel;
    }

    public float GetFlashlightStrength()
    {
        return flashlightStrengthLevel; // enhances visibility range
    }

    public float GetFlashlightBatteryCapacity()
    {
        return baseFlashlightBattery + (flashlightBatteryLevel * 20f); // increases battery life
    }

    public int GetStorageCapacity()
    {
        return (storageLevel * 5); // +5 inventory slots per level
    }

    public int UpgradeStat(string stat)
    {
        switch (stat.ToLower())
        {
            case "damage":
                damageLevel++;
                return damageLevel;
            case "ammo":
                ammoLevel++;
                return ammoLevel;
            case "speed":
                speedLevel++;
                return speedLevel;
            case "vitality":
                vitalityLevel++;
                return vitalityLevel;
            case "oxygen":
                oxygenLevel++;
                return oxygenLevel;
            case "armstrength":
                armStrengthLevel++;
                return armStrengthLevel;
            case "flashlightstrength":
                flashlightStrengthLevel++;
                return flashlightStrengthLevel;
            case "flashlightbattery":
                flashlightBatteryLevel++;
                return flashlightBatteryLevel;
            case "storage":
                storageLevel++;
                return storageLevel;
            default:
                Debug.LogWarning("Invalid stat name: " + stat);
                return -1;
        }
    }
}