using UnityEngine;
using System.Collections;

public class Enemy1AI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float wanderSpeed = 2f;
    public float chaseSpeed = 4f;
    
    [Header("Vision Settings")]
    public float visionDistance = 10f;
    [Range(0, 360)]
    public float visionAngle = 60f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;
    
    [Header("Wandering Settings")]
    public float wanderInterval = 3f;
    public float wanderRadius = 5f;
    [Header("Smooth Wandering")]
    public float smoothTurnSpeed = 180f; // Degrees per second
    
    [Header("Combat Settings")]
    public int damageAmount = 10;
    public float attackCooldown = 2f;
    public float dashBackDistance = 3f;
    public float dashBackDuration = 0.3f;
    public float attackRange = 1.5f;
    
    private Transform player;
    private Rigidbody2D rb;
    private float currentAngle;
    private float targetAngle;
    private float wanderTimer;
    private bool isChasing = false;
    private float lastAttackTime;
    private Health playerHealth;
    private bool isAttacking = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        if (player != null)
            playerHealth = player.GetComponent<Health>();
        
        // Initialize with random direction
        currentAngle = Random.Range(0f, 360f);
        targetAngle = currentAngle;
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);
        
        wanderTimer = wanderInterval;
    }
    
    void Update()
    {
        // Don't do anything if attacking
        if (isAttacking)
            return;
        
        // Check if player is in vision cone
        bool playerInSight = CheckPlayerInVision();
        
        // State machine
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
            isChasing = false;
            Wander();
        }
        
        // Smooth rotation (applied in both states except attacking)
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
        
        float angleToPlayer = Vector2.Angle(GetFacingDirection(), directionToPlayer);
        if (angleToPlayer > visionAngle / 2f)
            return false;
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
        if (hit.collider != null)
            return false;
        
        return true;
    }
    
    Vector2 GetFacingDirection()
    {
        return transform.right;
    }
    
    void ChasePlayer()
    {
        // Set target angle to face player
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        targetAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        
        // Move towards player
        rb.velocity = transform.right * chaseSpeed;
    }
    
    void Wander()
    {
        wanderTimer -= Time.deltaTime;
        
        if (wanderTimer <= 0)
        {
            SetNewWanderDirection();
            wanderTimer = wanderInterval;
        }
        
        // Move forward in current direction
        rb.velocity = transform.right * wanderSpeed;
    }
    
    void SetNewWanderDirection()
    {
        // Set a new random target angle
        targetAngle = Random.Range(0f, 360f);
        
        // Visual debug
        Debug.DrawRay(transform.position, Quaternion.Euler(0, 0, targetAngle) * Vector3.right * 2f, Color.green, wanderInterval);
    }
    
    void SmoothRotate()
    {
        // Smoothly rotate towards target angle
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
        
        // Stop current movement
        rb.velocity = Vector2.zero;
        
        // Deal damage to player
        if (playerHealth != null && player != null)
        {
            playerHealth.TakeDamage(damageAmount);
            Debug.Log("Monster dealt " + damageAmount + " damage!");
        }
        
        // Calculate dash back direction (opposite of facing direction)
        Vector2 dashDirection = -GetFacingDirection();
        
        // Calculate target position for dash
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
        
        // Small pause after dashing
        yield return new WaitForSeconds(0.2f);
        
        isAttacking = false;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw vision cone
        Gizmos.color = isChasing ? Color.red : Color.yellow;
        
        Vector3 forward = GetFacingDirection();
        float halfAngle = visionAngle / 2f;
        
        Vector3 leftBoundary = Quaternion.Euler(0, 0, halfAngle) * forward * visionDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -halfAngle) * forward * visionDistance;
        
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
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, visionDistance);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}