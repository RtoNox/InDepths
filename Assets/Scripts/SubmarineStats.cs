using UnityEngine;

public class SubmarineStats : MonoBehaviour
{
    [Header("Levels")]
    public int damageLevel = 1;
    public int ammoLevel = 1;
    public int speedLevel = 1;
    public int vitalityLevel = 1;
    public int oxygenLevel = 1;

    [Header("Base Values")]
    public int baseDamage = 2;
    public int baseAmmo = 3;
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
}