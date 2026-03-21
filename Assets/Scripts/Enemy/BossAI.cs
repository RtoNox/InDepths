using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float wanderSpeed = 2f;
    public float chaseSpeed = 4f;
    public float smoothTurnSpeed = 180f;
    
    [Header("Vision Settings")]
    public float visionDistance = 15f;
    [Range(0, 360)]
    public float visionAngle = 90f;
    public LayerMask obstacleLayer;
    
    [Header("Boss Stats")]
    public int maxHealth = 1000;
    public float skillTriggerInterval = 30f; // Trigger a skill every 30 seconds
    
    [Header("Passive Block")]
    [Range(0, 100)]
    public float blockChance = 10f;
    public GameObject blockEffectPrefab;
    public Color blockColor = Color.cyan;
    
    [Header("Skill 1 - Crash (Enemy 2 Style)")]
    public float baseChaseSpeed = 4f;
    public float maxChaseSpeed = 15f;
    public float accelerationRate = 3f;
    public int crashDamage = 20;
    public int selfDamageAmount = 20;
    public float crashAttackRange = 2f;
    public float speedThresholdForSelfDamage = 10f;
    public float crashSkillDuration = 8f; // How long the crash skill lasts
    
    [Header("Skill 2 - Projectile (Enemy 3 Style)")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;
    public float projectileLifetime = 4f;
    public float shootCooldown = 0.8f;
    public int projectileDamage = 15;
    public float spiralBulletHellChance = 10f;
    public int spiralBulletCount = 12;
    public float projectileSkillDuration = 8f; // How long the projectile skill lasts
    
    [Header("Skill 3 - Summon")]
    public GameObject enemy1Prefab;
    public GameObject enemy2Prefab;
    public GameObject enemy3Prefab;
    public GameObject enemy4Prefab;
    public int totalSummonCount = 4;
    public float summonRadius = 5f;
    
    [Header("Visual Effects")]
    public GameObject skillSwitchEffectPrefab;
    public Color defaultColor = Color.white;
    public Color skill1Color = Color.red;
    public Color skill2Color = Color.blue;
    public Color skill3Color = Color.green;
    
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private EnemyHealth bossHealth;
    private Health playerHealth;
    
    // State management
    private bool isUsingSkill = false;
    private int currentSkill = 0; // 0 = default, 1 = crash, 2 = projectile, 3 = summon
    private float skillEndTime;
    private float lastSkillTriggerTime;
    
    // Movement variables
    private float currentAngle;
    private float targetAngle;
    private float directionChangeTimer;
    private float currentChaseSpeed;
    private float lastAttackTime;
    private bool isChasing = false;
    private bool isAttacking = false;
    
    // Skill 2 specific
    private float lastShootTime;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        bossHealth = GetComponent<EnemyHealth>();
        
        // Get player's Health component
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            if (playerHealth == null)
            {
                Debug.LogError("Player has no Health component!");
            }
        }
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            spriteRenderer.color = defaultColor;
        }
        
        if (bossHealth != null)
        {
            bossHealth.maxHealth = maxHealth;
            bossHealth.currentHealth = maxHealth;
        }
        
        currentChaseSpeed = chaseSpeed;
        lastSkillTriggerTime = Time.time;
        
        // Initialize angle
        currentAngle = Random.Range(0f, 360f);
        targetAngle = currentAngle;
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);
        
        SetRandomDirectionTimer();
    }
    
    void Update()
    {
        if (player == null || isAttacking) return;
        
        // Check if skill should trigger
        if (!isUsingSkill && Time.time >= lastSkillTriggerTime + skillTriggerInterval)
        {
            TriggerRandomSkill();
        }
        
        // Check if current skill has ended
        if (isUsingSkill && Time.time >= skillEndTime)
        {
            EndCurrentSkill();
        }
        
        // Check if player is in vision
        bool playerInSight = CheckPlayerInVision();
        
        if (playerInSight)
        {
            isChasing = true;
            
            // Execute behavior based on current state
            if (isUsingSkill)
            {
                switch (currentSkill)
                {
                    case 1: // Crash skill
                        ExecuteCrashSkill();
                        break;
                    case 2: // Projectile skill
                        ExecuteProjectileSkill();
                        break;
                    case 3: // Summon skill (only triggers once)
                        // Summon skill is handled in TriggerRandomSkill
                        NormalChase();
                        break;
                }
            }
            else
            {
                // Default state - Enemy 1 behavior
                ExecuteDefaultBehavior();
            }
            
            // Always face the player
            FacePlayer();
        }
        else
        {
            if (isChasing)
            {
                isChasing = false;
                SetRandomWanderDirection();
            }
            Wander();
        }
        
        // Smooth rotation
        SmoothRotate();
    }
    
    void TriggerRandomSkill()
    {
        // Randomly choose a skill (1, 2, or 3)
        currentSkill = Random.Range(1, 4);
        isUsingSkill = true;
        lastSkillTriggerTime = Time.time;
        
        // Set skill duration
        switch (currentSkill)
        {
            case 1:
                skillEndTime = Time.time + crashSkillDuration;
                currentChaseSpeed = baseChaseSpeed;
                Debug.Log($"Boss activated CRASH skill for {crashSkillDuration} seconds!");
                break;
            case 2:
                skillEndTime = Time.time + projectileSkillDuration;
                lastShootTime = Time.time;
                Debug.Log($"Boss activated PROJECTILE skill for {projectileSkillDuration} seconds!");
                break;
            case 3:
                skillEndTime = Time.time + 2f; // Short duration for summon
                StartCoroutine(SummonMinions());
                Debug.Log("Boss activated SUMMON skill!");
                break;
        }
        
        // Visual feedback for skill activation
        StartCoroutine(SkillActivationEffect());
    }
    
    IEnumerator SkillActivationEffect()
    {
        // Play effect
        if (skillSwitchEffectPrefab != null)
        {
            Instantiate(skillSwitchEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Change color based on skill
        if (spriteRenderer != null)
        {
            Color skillColor = defaultColor;
            switch (currentSkill)
            {
                case 1: skillColor = skill1Color; break;
                case 2: skillColor = skill2Color; break;
                case 3: skillColor = skill3Color; break;
            }
            spriteRenderer.color = skillColor;
        }
        
        yield return new WaitForSeconds(0.3f);
        
        // Keep color during skill, will reset when skill ends
    }
    
    void EndCurrentSkill()
    {
        isUsingSkill = false;
        currentSkill = 0;
        
        // Reset to default state
        currentChaseSpeed = chaseSpeed;
        
        // Reset color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = defaultColor;
        }
        
        Debug.Log("Boss returned to DEFAULT state!");
    }
    
    void ExecuteDefaultBehavior()
    {
        // Enemy 1 behavior - simple chase
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        targetAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        rb.velocity = transform.right * chaseSpeed;
        
        // Check for attack (Enemy 1 style)
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= crashAttackRange && Time.time >= lastAttackTime + 2f)
        {
            AttackPlayer();
        }
    }
    
    void AttackPlayer()
    {
        lastAttackTime = Time.time;
        
        // Deal damage to player
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(15); // Default damage
            Debug.Log($"Boss attacked player for 15 damage!");
        }
        
        // Dash back slightly
        StartCoroutine(DefaultAttackDashBack());
    }
    
    IEnumerator DefaultAttackDashBack()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;
        
        Vector2 dashDirection = -transform.right;
        Vector2 targetPosition = (Vector2)transform.position + (dashDirection * 2f);
        Vector2 startPosition = transform.position;
        
        float elapsed = 0f;
        float dashDuration = 0.2f;
        
        while (elapsed < dashDuration)
        {
            transform.position = Vector2.Lerp(startPosition, targetPosition, elapsed / dashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = targetPosition;
        yield return new WaitForSeconds(0.1f);
        isAttacking = false;
    }
    
    void ExecuteCrashSkill()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Accelerate over time
        currentChaseSpeed += accelerationRate * Time.deltaTime;
        currentChaseSpeed = Mathf.Min(currentChaseSpeed, maxChaseSpeed);
        
        // Chase player
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        rb.velocity = directionToPlayer * currentChaseSpeed;
        
        // Check for crash
        if (distanceToPlayer <= crashAttackRange && Time.time >= lastAttackTime + 1f)
        {
            CrashIntoPlayer();
        }
        
        // Visual speed indicator
        float speedPercentage = (currentChaseSpeed - baseChaseSpeed) / (maxChaseSpeed - baseChaseSpeed);
        Debug.DrawRay(transform.position, Vector3.up * 3f * speedPercentage, Color.red);
    }
    
    void CrashIntoPlayer()
    {
        lastAttackTime = Time.time;
        
        // Deal damage to player
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(crashDamage);
            Debug.Log($"Boss crashed into player for {crashDamage} damage!");
        }
        
        // Self damage if speed is high enough
        if (currentChaseSpeed >= speedThresholdForSelfDamage && bossHealth != null)
        {
            bossHealth.TakeDamage(selfDamageAmount);
            Debug.Log($"Boss took {selfDamageAmount} self damage from high-speed crash!");
        }
        
        // Dash back after crash
        StartCoroutine(CrashDashBack());
    }
    
    IEnumerator CrashDashBack()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;
        
        Vector2 dashDirection = -transform.right;
        Vector2 targetPosition = (Vector2)transform.position + (dashDirection * 3f);
        Vector2 startPosition = transform.position;
        
        float elapsed = 0f;
        float dashDuration = 0.3f;
        
        while (elapsed < dashDuration)
        {
            transform.position = Vector2.Lerp(startPosition, targetPosition, elapsed / dashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = targetPosition;
        
        // Reset speed after crash
        currentChaseSpeed = baseChaseSpeed;
        
        yield return new WaitForSeconds(0.2f);
        isAttacking = false;
    }
    
    void ExecuteProjectileSkill()
    {
        // Face the player
        FacePlayer();
        
        // Stop moving while shooting
        rb.velocity = Vector2.zero;
        
        // Shoot projectiles
        if (Time.time >= lastShootTime + shootCooldown)
        {
            lastShootTime = Time.time;
            
            // Check for spiral bullet hell chance
            float randomChance = Random.Range(0f, 100f);
            
            if (randomChance <= spiralBulletHellChance)
            {
                StartCoroutine(SpiralBulletHell());
                Debug.Log("Boss performed SPIRAL BULLET HELL!");
            }
            else
            {
                ShootProjectile(transform.right);
            }
        }
    }
    
    void ShootProjectile(Vector2 direction)
    {
        if (projectilePrefab == null) return;
        
        Vector2 firePoint = (Vector2)transform.position + direction * 1.5f;
        
        GameObject projectile = Instantiate(projectilePrefab, firePoint, Quaternion.identity);
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        BossProjectile projScript = projectile.AddComponent<BossProjectile>();
        projScript.Initialize(direction, projectileSpeed, projectileDamage, projectileLifetime);
    }
    
    IEnumerator SpiralBulletHell()
    {
        for (int i = 0; i < spiralBulletCount; i++)
        {
            float angle = (360f / spiralBulletCount) * i;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            ShootProjectile(direction);
            yield return new WaitForSeconds(0.05f);
        }
    }
    
    IEnumerator SummonMinions()
    {
        Debug.Log("Boss is summoning minions!");
        
        List<GameObject> enemyPrefabs = new List<GameObject>();
        
        if (enemy1Prefab != null) enemyPrefabs.Add(enemy1Prefab);
        if (enemy2Prefab != null) enemyPrefabs.Add(enemy2Prefab);
        if (enemy3Prefab != null) enemyPrefabs.Add(enemy3Prefab);
        if (enemy4Prefab != null) enemyPrefabs.Add(enemy4Prefab);
        
        if (enemyPrefabs.Count == 0)
        {
            Debug.LogError("No enemy prefabs assigned for summoning!");
            yield break;
        }
        
        for (int i = 0; i < totalSummonCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * summonRadius;
            Vector2 spawnPosition = (Vector2)transform.position + randomOffset;
            
            GameObject enemyToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            Instantiate(enemyToSpawn, spawnPosition, Quaternion.identity);
            
            yield return new WaitForSeconds(0.2f);
        }
        
        Debug.Log($"Boss summoned {totalSummonCount} minions!");
    }
    
    void NormalChase()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        targetAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        rb.velocity = transform.right * chaseSpeed;
    }
    
    void Wander()
    {
        directionChangeTimer -= Time.deltaTime;
        
        if (directionChangeTimer <= 0)
        {
            SetRandomWanderDirection();
            SetRandomDirectionTimer();
        }
        
        rb.velocity = transform.right * wanderSpeed;
    }
    
    void SetRandomWanderDirection()
    {
        targetAngle = Random.Range(0f, 360f);
    }
    
    void SetRandomDirectionTimer()
    {
        directionChangeTimer = Random.Range(2f, 5f);
    }
    
    bool CheckPlayerInVision()
    {
        if (player == null) return false;
        
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer > visionDistance)
            return false;
        
        float angleToPlayer = Vector2.Angle(transform.right, directionToPlayer);
        if (angleToPlayer > visionAngle * 0.5f)
            return false;
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
        if (hit.collider != null)
            return false;
        
        return true;
    }
    
    void FacePlayer()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        targetAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
    }
    
    void SmoothRotate()
    {
        float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);
        float maxRotation = smoothTurnSpeed * Time.deltaTime;
        float rotationThisFrame = Mathf.Clamp(angleDifference, -maxRotation, maxRotation);
        
        currentAngle += rotationThisFrame;
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);
    }
    
    // ===== BLOCK MECHANIC =====
    public bool TryBlock()
    {
        float randomValue = Random.Range(0f, 100f);
        bool blocked = randomValue <= blockChance;
        
        if (blocked)
        {
            StartCoroutine(BlockEffect());
            Debug.Log($"Boss BLOCKED the attack!");
        }
        
        return blocked;
    }
    
    IEnumerator BlockEffect()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = blockColor;
        }
        
        if (blockEffectPrefab != null)
        {
            GameObject effect = Instantiate(blockEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 0.5f);
        }
        
        Vector2 knockbackDirection = -transform.right;
        rb.velocity = knockbackDirection * 5f;
        
        yield return new WaitForSeconds(0.2f);
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isUsingSkill ? 
                (currentSkill == 1 ? skill1Color : currentSkill == 2 ? skill2Color : skill3Color) : 
                defaultColor;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Vector3 forward = transform.right;
        float halfAngle = visionAngle * 0.5f;
        
        Vector3 leftBoundary = Quaternion.Euler(0, 0, halfAngle) * forward * visionDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -halfAngle) * forward * visionDistance;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, crashAttackRange);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, summonRadius);
        
        #if UNITY_EDITOR
        string stateName = isUsingSkill ? 
            (currentSkill == 1 ? "CRASH SKILL" : currentSkill == 2 ? "PROJECTILE SKILL" : "SUMMON SKILL") : 
            "DEFAULT (Enemy 1)";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
            $"State: {stateName}\nHP: {bossHealth?.currentHealth}/{maxHealth}");
        #endif
    }
}

// Boss Projectile Script
public class BossProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private int damage;
    private float lifetime;
    private Rigidbody2D rb;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    public void Initialize(Vector2 shootDirection, float projectileSpeed, int damageAmount, float lifetimeDuration)
    {
        direction = shootDirection;
        speed = projectileSpeed;
        damage = damageAmount;
        lifetime = lifetimeDuration;
        
        rb.velocity = direction * speed;
        
        Destroy(gameObject, lifetime);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Boss projectile hit player for {damage} damage!");
            }
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}