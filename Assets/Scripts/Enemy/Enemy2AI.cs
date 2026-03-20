using UnityEngine;
using System.Collections;

public class Enemy2AI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float wanderSpeed = 2f;
    public float baseChaseSpeed = 4f;
    public float maxChaseSpeed = 10f; // Twice player's max speed (assuming player max is 5)
    public float accelerationRate = 2f; // How fast speed increases per second
    
    [Header("Vision Settings")]
    public float visionDistance = 12f; // Slightly longer vision than enemy1
    [Range(0, 360)]
    public float visionAngle = 70f; // Slightly wider vision
    public LayerMask obstacleLayer;
    
    [Header("Wander Settings")]
    public float minDirectionChangeInterval = 1f;
    public float maxDirectionChangeInterval = 4f;
    public float smoothTurnSpeed = 180f;
    
    [Header("Combat Settings")]
    public int baseDamage = 8; // Base damage when slow
    public int maxDamage = 15; // Max damage when at top speed
    public float attackCooldown = 1.2f; // Slightly faster attack rate
    public float attackRange = 1.5f;
    public float dashBackDistance = 2.5f;
    public float dashBackDuration = 0.25f;
    public int selfDamageAmount = 3; // Damage taken when crashing at high speed
    
    [Header("Speed Damage Scaling")]
    public AnimationCurve damageScalingCurve = AnimationCurve.Linear(0, 0, 1, 1); // Maps speed percentage to damage percentage
    
    private Transform player;
    private Rigidbody2D rb;
    private float currentAngle;
    private float targetAngle;
    private float directionChangeTimer;
    private float lastAttackTime;
    private bool isChasing = false;
    private bool isDashing = false;
    private Health playerHealth;
    private Health selfHealth;
    private float currentChaseSpeed;
    private float playerMaxSpeed; // We'll try to detect this
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        selfHealth = GetComponent<Health>();
        
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            
            // Try to detect player's max speed
            PlayerController playerMovement = player.GetComponent<PlayerController>();
            if (playerMovement != null)
            {
                // Assuming your player has a max speed variable - adjust this based on your player script
                playerMaxSpeed = playerMovement.moveSpeed; 
            }
            else
            {
                playerMaxSpeed = 5f; // Default fallback
            }
            
            // Set max speed to twice player's max
            maxChaseSpeed = playerMaxSpeed * 2f;
        }
        
        // Initialize with random direction
        currentAngle = Random.Range(0f, 360f);
        targetAngle = currentAngle;
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);
        
        currentChaseSpeed = baseChaseSpeed;
        SetRandomDirectionTimer();
    }
    
    void Update()
    {
        if (player == null || isDashing) return;
        
        // Check if player is in vision cone
        bool playerInSight = CheckPlayerInVision();
        
        // State management
        if (playerInSight)
        {
            if (!isChasing)
            {
                // Just started chasing - reset speed
                isChasing = true;
                currentChaseSpeed = baseChaseSpeed;
            }
            else
            {
                // Increase speed over time while chasing
                currentChaseSpeed += accelerationRate * Time.deltaTime;
                currentChaseSpeed = Mathf.Min(currentChaseSpeed, maxChaseSpeed);
            }
            
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
            // Switch to wander if player was chasing but now lost
            if (isChasing)
            {
                isChasing = false;
                currentChaseSpeed = baseChaseSpeed; // Reset speed
                SetRandomWanderDirection();
                SetRandomDirectionTimer();
            }
            
            Wander();
        }
        
        // Smooth rotation
        if (!isDashing)
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
        
        // Move towards player at current speed
        rb.velocity = transform.right * currentChaseSpeed;
        
        // Visual indicator of speed (optional)
        float speedPercentage = (currentChaseSpeed - baseChaseSpeed) / (maxChaseSpeed - baseChaseSpeed);
        Debug.DrawRay(transform.position, Vector3.up * 2f * speedPercentage, Color.red);
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
    
    int CalculateDamage()
    {
        // Calculate damage based on current speed
        float speedPercentage = (currentChaseSpeed - baseChaseSpeed) / (maxChaseSpeed - baseChaseSpeed);
        speedPercentage = Mathf.Clamp01(speedPercentage);
        
        // Apply damage scaling curve
        float damageScale = damageScalingCurve.Evaluate(speedPercentage);
        int damage = Mathf.RoundToInt(Mathf.Lerp(baseDamage, maxDamage, damageScale));
        
        return damage;
    }
    
    IEnumerator AttackAndDashBack()
    {
        isDashing = true;
        lastAttackTime = Time.time;
        
        // Stop movement
        rb.velocity = Vector2.zero;
        
        // Calculate damage based on current speed
        int damageToDeal = CalculateDamage();
        
        // Deal damage to player
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageToDeal);
            Debug.Log($"Enemy2 attacked for {damageToDeal} damage (Speed: {currentChaseSpeed:F1})");
        }
        
        // Self-damage if moving fast enough
        float speedThreshold = baseChaseSpeed + (maxChaseSpeed - baseChaseSpeed) * 0.5f; // 50% of max speed
        if (currentChaseSpeed >= speedThreshold && selfHealth != null)
        {
            selfHealth.TakeDamage(selfDamageAmount);
            Debug.Log($"Enemy2 took {selfDamageAmount} self-damage from high-speed crash!");
        }
        
        // Dash backwards
        Vector2 dashDirection = -transform.right;
        Vector2 startPosition = transform.position;
        Vector2 targetPosition = startPosition + (dashDirection * dashBackDistance);
        
        float elapsed = 0f;
        while (elapsed < dashBackDuration)
        {
            float t = elapsed / dashBackDuration;
            transform.position = Vector2.Lerp(startPosition, targetPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = targetPosition;
        
        // Reset speed after attack
        currentChaseSpeed = baseChaseSpeed;
        
        // Small pause
        yield return new WaitForSeconds(0.1f);
        
        isDashing = false;
    }
    
    // Optional: Add collision detection for high-speed crashes
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDashing) return;
        
        // If we hit something at high speed that's NOT the player, take self damage
        if (!collision.gameObject.CompareTag("Player") && isChasing)
        {
            float speedThreshold = baseChaseSpeed + (maxChaseSpeed - baseChaseSpeed) * 0.5f;
            if (currentChaseSpeed >= speedThreshold && selfHealth != null)
            {
                selfHealth.TakeDamage(selfDamageAmount);
                Debug.Log($"Enemy2 crashed into obstacle at high speed! Took {selfDamageAmount} damage");
            }
        }
    }
    
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
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw speed indicator in editor
        if (Application.isPlaying)
        {
            float speedPercentage = (currentChaseSpeed - baseChaseSpeed) / (maxChaseSpeed - baseChaseSpeed);
            Gizmos.color = Color.Lerp(Color.green, Color.red, speedPercentage);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
        }
    }
}