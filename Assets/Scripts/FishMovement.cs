using UnityEngine;

public class FishMovement : MonoBehaviour
{
    [Header("Swimming Settings")]
    public float swimSpeed = 3f;
    
    [Header("Wandering Behavior")]
    public float minDirectionChangeTime = 2f;
    public float maxDirectionChangeTime = 5f;
    public float idleTime = 1f;
    
    [Header("Water Boundaries")]
    public float waterSurfaceY = -2f;
    
    [Header("Visual Effects")]
    public bool enableTailWag = true;
    public float tailWagSpeed = 5f;
    public float tailWagAmount = 15f;
    public Transform tailTransform;
    
    private Rigidbody2D rb;
    private Vector2 currentDirection;
    private Vector2 targetDirection;
    private float directionTimer;
    private bool isIdle = false;
    private float idleTimer;
    private float originalTailAngle;
    private SpriteRenderer spriteRenderer;
    private bool facingRight = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Set physics properties
        rb.gravityScale = 0f;
        rb.drag = 1f;
        
        // Initialize random direction
        currentDirection = Random.insideUnitCircle.normalized;
        targetDirection = currentDirection;
        
        SetNewDirectionTimer();
        
        // Store original tail angle
        if (tailTransform != null)
        {
            originalTailAngle = tailTransform.localEulerAngles.z;
        }
        
        // Ensure fish starts below water surface
        Vector3 startPos = transform.position;
        if (startPos.y > waterSurfaceY)
        {
            startPos.y = waterSurfaceY - 1f;
            transform.position = startPos;
        }
        
        // Set initial facing direction
        UpdateFacingDirection();
    }
    
    void Update()
    {
        // Enforce water surface boundary
        EnforceWaterSurface();
        
        if (!isIdle)
        {
            // Update direction change timer
            directionTimer -= Time.deltaTime;
            
            if (directionTimer <= 0)
            {
                // Start idle period
                isIdle = true;
                idleTimer = idleTime;
                rb.velocity = Vector2.zero;
                directionTimer = 0;
            }
        }
        else
        {
            // Idle period
            idleTimer -= Time.deltaTime;
            
            if (idleTimer <= 0)
            {
                // Choose new random direction
                SetRandomDirection();
                isIdle = false;
                SetNewDirectionTimer();
            }
        }
        
        // Smooth direction change
        SmoothDirectionChange();
        
        // Move in current direction
        if (!isIdle)
        {
            rb.velocity = currentDirection * swimSpeed;
        }
        
        // Update sprite facing direction
        UpdateFacingDirection();
        
        // Handle tail wag
        if (enableTailWag && tailTransform != null && !isIdle)
        {
            UpdateTailWag();
        }
    }
    
    void EnforceWaterSurface()
    {
        // If fish tries to go above water surface
        if (transform.position.y >= waterSurfaceY)
        {
            // Clamp position to water surface
            Vector3 pos = transform.position;
            pos.y = waterSurfaceY;
            transform.position = pos;
            
            // Reverse vertical velocity to push fish downward
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, -Mathf.Abs(rb.velocity.y));
            }
            
            // Force direction to have downward component
            if (targetDirection.y > 0)
            {
                targetDirection = new Vector2(targetDirection.x, -Mathf.Abs(targetDirection.y)).normalized;
                currentDirection = targetDirection;
            }
        }
    }
    
    void SetRandomDirection()
    {
        // Choose random direction
        float randomAngle = Random.Range(0f, 360f);
        targetDirection = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
        
        // If near surface, prevent upward movement
        float distanceToSurface = waterSurfaceY - transform.position.y;
        if (distanceToSurface < 1.5f && targetDirection.y > 0)
        {
            // Force direction to be downward or horizontal
            targetDirection.y = -Mathf.Abs(targetDirection.y);
            targetDirection.Normalize();
        }
    }
    
    void SmoothDirectionChange()
    {
        // Smoothly interpolate towards target direction
        currentDirection = Vector2.Lerp(currentDirection, targetDirection, Time.deltaTime * 3f);
        
        // Re-normalize to maintain direction
        if (currentDirection.magnitude > 0.01f)
        {
            currentDirection.Normalize();
        }
    }
    
    void UpdateFacingDirection()
    {
        if (spriteRenderer == null) return;
        
        // Flip sprite based on horizontal movement
        if (currentDirection.x > 0 && !facingRight)
        {
            facingRight = false;
            spriteRenderer.flipX = true;
        }
        else if (currentDirection.x < 0 && facingRight)
        {
            facingRight = true;
            spriteRenderer.flipX = false;
        }
    }
    
    void UpdateTailWag()
    {
        // Calculate wag based on speed
        float wagAngle = Mathf.Sin(Time.time * tailWagSpeed) * tailWagAmount;
        
        tailTransform.localEulerAngles = new Vector3(0, 0, originalTailAngle + wagAngle);
    }
    
    void SetNewDirectionTimer()
    {
        directionTimer = Random.Range(minDirectionChangeTime, maxDirectionChangeTime);
    }
    
    // Public methods
    public void SetDirection(Vector2 direction)
    {
        targetDirection = direction.normalized;
        isIdle = false;
        SetNewDirectionTimer();
    }
    
    public void FleeFrom(Vector3 dangerPosition)
    {
        Vector2 fleeDirection = (transform.position - dangerPosition).normalized;
        targetDirection = fleeDirection;
        isIdle = false;
        SetNewDirectionTimer();
    }
    
    public void SwimTo(Vector3 targetPosition)
    {
        Vector2 directionToTarget = (targetPosition - transform.position).normalized;
        targetDirection = directionToTarget;
        isIdle = false;
        SetNewDirectionTimer();
    }
    
    public void Stop()
    {
        isIdle = true;
        idleTimer = idleTime;
        rb.velocity = Vector2.zero;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw water surface line
        Gizmos.color = Color.cyan;
        Vector3 surfaceStart = new Vector3(transform.position.x - 10f, waterSurfaceY, 0);
        Vector3 surfaceEnd = new Vector3(transform.position.x + 10f, waterSurfaceY, 0);
        Gizmos.DrawLine(surfaceStart, surfaceEnd);
        
        // Draw direction line
        Gizmos.color = Color.green;
        if (Application.isPlaying)
        {
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)currentDirection * 2f);
        }
        else
        {
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * 2f);
        }
    }
}