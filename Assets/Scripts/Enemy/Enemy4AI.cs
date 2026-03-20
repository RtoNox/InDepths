using UnityEngine;
using System.Collections;

public class Enemy4AI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float wanderSpeed = 2f;
    public float chaseSpeed = 4f;
    
    [Header("Vision Settings")]
    public float visionDistance = 10f;
    [Range(0, 360)]
    public float visionAngle = 60f;
    public LayerMask obstacleLayer;
    
    [Header("Wander Settings")]
    public float minDirectionChangeInterval = 1f;
    public float maxDirectionChangeInterval = 4f;
    public float smoothTurnSpeed = 180f;
    
    [Header("Combat Settings")]
    public int damageAmount = 10;
    public float attackCooldown = 2f;
    public float attackRange = 1.5f;
    public float dashBackDistance = 3f;
    public float dashBackDuration = 0.3f;
    
    [Header("Block Settings")]
    [Range(0, 100)]
    public float blockChance = 25f; // 25% chance to block
    public GameObject blockEffectPrefab; // Optional visual effect when blocking
    public AudioClip blockSound; // Optional block sound effect
    public Color blockColor = Color.cyan; // Color when blocking
    
    private Transform player;
    private Rigidbody2D rb;
    private float currentAngle;
    private float targetAngle;
    private float directionChangeTimer;
    private float lastAttackTime;
    private bool isChasing = false;
    private bool isAttacking = false;
    private bool isBlocking = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 originalScale;
    private Health enemyHealth;
    private AudioSource audioSource;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyHealth = GetComponent<Health>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null && (blockSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        
        originalScale = transform.localScale;
        
        currentAngle = Random.Range(0f, 360f);
        targetAngle = currentAngle;
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);
        
        SetRandomDirectionTimer();
    }
    
    void Update()
    {
        if (player == null || isAttacking) return;
        
        // Check if player is in vision cone
        bool playerInSight = CheckPlayerInVision();
        
        // State management
        if (playerInSight)
        {
            isChasing = true;
            
            // Check if in attack range
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(AttackAndDashBack());
            }
            else
            {
                ChasePlayer();
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
            
            Wander();
        }
        
        // Smooth rotation
        if (!isAttacking)
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
    
    void ChasePlayer()
    {
        // Calculate angle to player
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        targetAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        
        // Move towards player
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
    
    IEnumerator AttackAndDashBack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // Stop movement
        rb.velocity = Vector2.zero;
        
        // Deal damage to player
        if (player != null)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                Debug.Log($"Enemy4 dealt {damageAmount} damage!");
            }
        }
        
        // Calculate dash back direction
        Vector2 dashDirection = -transform.right;
        Vector2 targetPosition = (Vector2)transform.position + (dashDirection * dashBackDistance);
        
        // Perform the dash
        float elapsedTime = 0;
        Vector2 startingPosition = transform.position;
        
        while (elapsedTime < dashBackDuration)
        {
            transform.position = Vector2.Lerp(startingPosition, targetPosition, elapsedTime / dashBackDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.position = targetPosition;
        
        // Small pause
        yield return new WaitForSeconds(0.2f);
        
        isAttacking = false;
    }
    
    // ===== BLOCK MECHANIC =====
    // This method should be called when the player attacks this enemy
    public bool TryBlock()
    {
        // Calculate block chance
        float randomValue = Random.Range(0f, 100f);
        bool blocked = randomValue <= blockChance;
        
        if (blocked)
        {
            StartCoroutine(BlockEffect());
            Debug.Log($"Enemy4 BLOCKED the attack! (Chance: {blockChance}%, Rolled: {randomValue})");
        }
        else
        {
            Debug.Log($"Enemy4 failed to block. (Chance: {blockChance}%, Rolled: {randomValue})");
        }
        
        return blocked;
    }
    
    IEnumerator BlockEffect()
    {
        isBlocking = true;
        
        // Visual feedback
        if (spriteRenderer != null)
        {
            spriteRenderer.color = blockColor;
        }
        
        // Spawn block effect
        if (blockEffectPrefab != null)
        {
            GameObject effect = Instantiate(blockEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 0.5f);
        }
        
        // Play block sound
        if (audioSource != null && blockSound != null)
        {
            audioSource.PlayOneShot(blockSound);
        }
        
        // Optional: Add a knockback effect
        Vector2 knockbackDirection = -transform.right;
        rb.velocity = knockbackDirection * 5f;
        
        // Wait for block animation
        yield return new WaitForSeconds(0.2f);
        
        // Reset visual
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        isBlocking = false;
    }
    
    // Optional: Add a visual indicator for block chance in the inspector
    void OnDrawGizmosSelected()
    {
        // Draw vision cone
        Vector3 forward = transform.right;
        float halfAngle = visionAngle * 0.5f;
        
        Vector3 leftBoundary = Quaternion.Euler(0, 0, halfAngle) * forward * visionDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -halfAngle) * forward * visionDistance;
        
        Gizmos.color = isChasing ? Color.red : Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        
        // Draw arc
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
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw block chance text (optional - shows in scene view)
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, $"Block: {blockChance}%");
        #endif
    }
}