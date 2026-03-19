using UnityEngine;

public class SubmarineStats : MonoBehaviour
{
    [Header("Levels")]
    public int damageLevel = 0;
    public int ammoLevel = 0;
    public int speedLevel = 0;
    public int vitalityLevel = 0;
    public int oxygenLevel = 0;

    [Header("Base Values")]
    public int baseDamage = 20;
    public int baseAmmo = 10;
    public float baseSpeed = 5f;
    public int baseHealth = 100;
    public float baseOxygen = 100f;

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
            default:
                Debug.LogWarning("Invalid stat name: " + stat);
                return -1;
        }
    }
}