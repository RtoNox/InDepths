using UnityEngine;
using System.Collections;

public class Enemy3AI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float wanderSpeed = 2f;
    public float strafeSpeed = 3f;
    public float maintainDistanceSpeed = 4f;
    
    [Header("Vision Settings")]
    public float visionDistance = 12f;
    public float preferredDistance = 8f;
    public float tooCloseDistance = 5f;
    [Range(0, 360)]
    public float visionAngle = 90f;
    public LayerMask obstacleLayer;
    
    [Header("Wander Settings")]
    public float minDirectionChangeInterval = 1f;
    public float maxDirectionChangeInterval = 4f;
    public float smoothTurnSpeed = 180f;
    
    [Header("Combat Settings")]
    public int damageAmount = 8;
    public float attackCooldown = 2f;
    public float chargeTime = 0.5f;
    public float attackRange = 10f;
    
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float projectileLifetime = 3f;
    public Transform firePoint;
    
    [Header("Visual Effects")]
    public GameObject chargeEffectPrefab;
    public Color chargeColor = Color.red;
    
    private Transform player;
    private Rigidbody2D rb;
    private float currentAngle;
    private float targetAngle;
    private float directionChangeTimer;
    private float lastAttackTime;
    private bool isChasing = false;
    private bool isCharging = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private GameObject currentChargeEffect;
    private Vector3 originalScale;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        
        originalScale = transform.localScale;
        
        // Create fire point if not assigned
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.parent = transform;
            fp.transform.localPosition = new Vector3(1f, 0f, 0f);
            firePoint = fp.transform;
        }
        
        currentAngle = Random.Range(0f, 360f);
        targetAngle = currentAngle;
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);
        
        SetRandomDirectionTimer();
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Always face player while charging
        if (isCharging)
        {
            FacePlayer();
            currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, smoothTurnSpeed * Time.deltaTime / 360f);
            transform.rotation = Quaternion.Euler(0, 0, currentAngle);
        }
        
        bool playerInSight = CheckPlayerInVision();
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (playerInSight && distanceToPlayer <= attackRange)
        {
            isChasing = true;
            
            if (!isCharging)
            {
                FacePlayer();
                MaintainDistance();
            }
            
            // Start charging attack if cooldown is ready
            if (Time.time >= lastAttackTime + attackCooldown && !isCharging)
            {
                StartCoroutine(ChargeAndShoot());
            }
        }
        else
        {
            if (isChasing)
            {
                isChasing = false;
                SetRandomWanderDirection();
                SetRandomDirectionTimer();
            }
            
            if (!isCharging)
            {
                Wander();
            }
        }
        
        // Smooth rotation when not charging
        if (!isCharging)
        {
            SmoothRotate();
        }
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
    
    void MaintainDistance()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        
        if (distanceToPlayer < tooCloseDistance)
        {
            rb.velocity = -directionToPlayer * maintainDistanceSpeed;
        }
        else if (distanceToPlayer > preferredDistance)
        {
            rb.velocity = directionToPlayer * maintainDistanceSpeed;
        }
        else
        {
            Vector2 perpendicular = new Vector2(-directionToPlayer.y, directionToPlayer.x);
            if (Random.value > 0.5f)
                perpendicular = -perpendicular;
                
            rb.velocity = perpendicular * strafeSpeed;
        }
    }
    
    IEnumerator ChargeAndShoot()
    {
        isCharging = true;
        rb.velocity = Vector2.zero;
        
        // Spawn charge effect if available
        if (chargeEffectPrefab != null)
        {
            currentChargeEffect = Instantiate(chargeEffectPrefab, firePoint.position, Quaternion.identity, transform);
        }
        
        // Charge timer with gradual glow
        float chargeTimer = 0f;
        while (chargeTimer < chargeTime)
        {
            // Calculate charge progress (0 to 1)
            float chargeProgress = chargeTimer / chargeTime;
            
            // Gradual glow effect - interpolate color
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(originalColor, chargeColor, chargeProgress);
                
                // Optional pulsing effect
                float pulse = 1f + Mathf.Sin(chargeTimer * 30f) * (0.1f + chargeProgress * 0.2f);
                transform.localScale = originalScale * pulse;
            }
            
            // Keep aiming at player during charge
            FacePlayer();
            currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, smoothTurnSpeed * Time.deltaTime / 360f);
            transform.rotation = Quaternion.Euler(0, 0, currentAngle);
            
            chargeTimer += Time.deltaTime;
            yield return null;
        }
        
        // Shoot the projectile at the player's CURRENT position (when charge finishes)
        ShootProjectile();
        
        // Reset visual effects immediately
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
            transform.localScale = originalScale;
        }
        
        if (currentChargeEffect != null)
        {
            Destroy(currentChargeEffect);
        }
        
        lastAttackTime = Time.time;
        isCharging = false;
    }
    
    void ShootProjectile()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogError("Projectile prefab or fire point not assigned!");
            return;
        }
        
        // Calculate direction to player's CURRENT position at the moment of firing
        Vector2 directionToPlayer = (player.position - firePoint.position).normalized;
        
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        // Rotate projectile to face direction
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // Get or add projectile component
        EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
        
        if (projScript != null)
        {
            projScript.Initialize(directionToPlayer, projectileSpeed, damageAmount, projectileLifetime);
        }
        else
        {
            projScript = projectile.AddComponent<EnemyProjectile>();
            projScript.Initialize(directionToPlayer, projectileSpeed, damageAmount, projectileLifetime);
        }
        
        Debug.Log("Enemy3 fired at player position! Direction: " + directionToPlayer);
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
        Debug.DrawRay(transform.position, Quaternion.Euler(0, 0, targetAngle) * Vector3.right * 2f, Color.green, 1f);
    }
    
    void SetRandomDirectionTimer()
    {
        directionChangeTimer = Random.Range(minDirectionChangeInterval, maxDirectionChangeInterval);
    }
    
    void SmoothRotate()
    {
        float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);
        float maxRotation = smoothTurnSpeed * Time.deltaTime;
        float rotationThisFrame = Mathf.Clamp(angleDifference, -maxRotation, maxRotation);
        
        currentAngle += rotationThisFrame;
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);
    }
    
    void OnDrawGizmosSelected()
    {
        Vector3 forward = transform.right;
        float halfAngle = visionAngle * 0.5f;
        
        Vector3 leftBoundary = Quaternion.Euler(0, 0, halfAngle) * forward * visionDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -halfAngle) * forward * visionDistance;
        
        Gizmos.color = isCharging ? Color.magenta : (isChasing ? Color.red : Color.yellow);
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        
        int segments = 20;
        Vector3 previousPoint = transform.position + leftBoundary;
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = Mathf.Lerp(-halfAngle, halfAngle, t) * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
            Vector3 point = transform.position + direction * visionDistance;
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, preferredDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, tooCloseDistance);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

// ===== FIXED PROJECTILE SCRIPT =====
public class EnemyProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private int damage;
    private float lifetime;
    private Rigidbody2D rb;
    private Collider2D projectileCollider;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        projectileCollider = GetComponent<Collider2D>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }
        
        // Make sure the projectile has a collider
        if (projectileCollider == null)
        {
            // Add a circle collider by default if no collider exists
            projectileCollider = gameObject.AddComponent<CircleCollider2D>();
            Debug.Log("Added CircleCollider2D to projectile");
        }
        
        // Set the collider as trigger for better detection
        projectileCollider.isTrigger = true;
    }
    
    public void Initialize(Vector2 shootDirection, float projectileSpeed, int damageAmount, float lifetimeDuration)
    {
        direction = shootDirection;
        speed = projectileSpeed;
        damage = damageAmount;
        lifetime = lifetimeDuration;
        
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
        
        // Ignore collision with the enemy that shot it
        GameObject shooter = GameObject.FindObjectOfType<Enemy3AI>()?.gameObject;
        if (shooter != null && projectileCollider != null)
        {
            Collider2D shooterCollider = shooter.GetComponent<Collider2D>();
            if (shooterCollider != null)
            {
                Physics2D.IgnoreCollision(projectileCollider, shooterCollider);
            }
        }
        
        // Ignore collision with other projectiles (optional)
        // gameObject.layer = LayerMask.NameToLayer("Projectile");
        
        Destroy(gameObject, lifetime);
        
        Debug.Log($"Projectile initialized: Direction={direction}, Speed={speed}, Damage={damage}");
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Projectile triggered with: {other.gameObject.name}, Tag: {other.tag}");
        
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Projectile hit player for {damage} damage!");
            }
            else
            {
                Debug.LogError("Player has no Health component!");
            }
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacles") || 
                 other.CompareTag("Obstacle"))
        {
            Debug.Log("Projectile hit obstacle");
            Destroy(gameObject);
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Projectile collided with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        
        if (collision.gameObject.CompareTag("Player"))
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Projectile hit player for {damage} damage!");
            }
            else
            {
                Debug.LogError("Player has no Health component!");
            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacles") || 
                 collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Projectile hit obstacle");
            Destroy(gameObject);
        }
    }
    
    // Visual debug in scene view
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}