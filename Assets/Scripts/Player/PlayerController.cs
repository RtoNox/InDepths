using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        ArmCollect,
        TorpedoShoot,
        SpeedBoost
    }

    public PlayerState currentState = PlayerState.SpeedBoost;

    private SubmarineStats stats;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 5f;
    public float drag = 4f;
    public float surfaceY = 0f; // y = 0 is water surface
    public float maxDepthY = -10000f; // max depth player can go

    [Header("Robot Arm")]
    public Transform armTransform; // optional (can be null)
    public float armExtendDistance = 2f;
    public float armExtendSpeed = 10f;

    [Header("Flashlight")]
    public GameObject flashlight;
    public FlashlightController flashlightController;

    private bool isExtending = false;
    private Vector3 armStartPos;
    private Vector3 armTargetPos;
    private Rigidbody2D rb;
    private Vector2 movement;

    [Header("Item Collection")]
    public float collectRange = 3f;
    public float collectAngle = 15f; // cone angle
    public LayerMask itemLayer; // Layer for items to collect

    private Inventory inventory;
    public Item heldItem;

    [Header("Torpedo Shooting")]
    public GameObject projectilePrefab;
    private int damageAmount;
    public float projectileSpeed = 10f;
    public float projectileLifetime = 3f;
    public Transform firePoint;
    public float fireRate = 1f;
    private float nextFireTime = 0f;
    public int torpedoesRemaining = 10;

    [Header("Game UI")]
    public TextMeshProUGUI storageText;
    public TextMeshProUGUI torpedoText;

    [Header("Shop")]
    public ShopUIController shopUI;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inventory = GetComponent<Inventory>();
        stats = GetComponent<SubmarineStats>();
        shopUI = FindObjectOfType<ShopUIController>();

        GameManager.Instance.Initialize(
            GetComponent<Inventory>(),
            GetComponent<PlayerCurrency>()
        );

        if (armTransform != null)
        {
            armStartPos = armTransform.localPosition;
        }

        if (firePoint == null)
        {
            firePoint = transform; // default to player position if not set
        }
    }

    void Update()
    {
        if (shopUI != null && shopUI.IsOpen())
        {
            return;
        }

        // Get input (WASD)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 direction = (mouseWorld - transform.position).normalized;

        if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);

        movement = movement.normalized;

        if (flashlight != null)
        {
            if (Input.GetMouseButtonDown(1))
            {
                flashlightController.Toggle();
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            flashlight.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }

        // Update Game UI
        UpdateStorageUI();
        torpedoText.text = "Torpedoes: " + torpedoesRemaining;

        if (Input.GetKeyDown("1"))
        {
            currentState = PlayerState.ArmCollect;
        }
        if (Input.GetKeyDown("2"))
        {
            currentState = PlayerState.TorpedoShoot;
        }
        if (Input.GetKeyDown("3"))
        {
            currentState = PlayerState.SpeedBoost;
        }

        if (currentState == PlayerState.ArmCollect && Input.GetMouseButtonDown(0))
        {
            StartArmAction();
        }

        if (currentState == PlayerState.TorpedoShoot && (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime))
        {
            if (torpedoesRemaining <= 0)
            {
                Debug.Log("No torpedoes remaining! Return to base to resupply.");
                return;
            }

            ShootTorpedo();
            nextFireTime = Time.time + fireRate;
        }

        if (armTransform != null && isExtending)
        {
            armTransform.localPosition = Vector3.Lerp(
                armTransform.localPosition,
                armTargetPos,
                armExtendSpeed * Time.deltaTime
            );

            if (Vector3.Distance(armTransform.localPosition, armTargetPos) < 0.05f)
            {
                // retract
                armTransform.localPosition = armStartPos;
                isExtending = false;
            }
        }

        if (transform.position.y >= -0.2f)
        {
            if (Input.GetKeyDown(KeyCode.F) && !shopUI.IsOpen())
            {
                GameManager.Instance.EndDay();
                shopUI.OpenShop();
                torpedoesRemaining = 10; // Resupply torpedoes when visiting shop
                flashlightController.RefillBattery(); // Refill flashlight battery when visiting shop
            }
        }
    }

    void FixedUpdate()
    {
        if (currentState == PlayerState.SpeedBoost)
        {
            moveSpeed = stats.GetSpeed();
        }
        else
        {
            moveSpeed = 5f; // default speed for other states
        }

        // Smooth underwater-like movement
        Vector2 targetVelocity = movement * moveSpeed;
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        // Apply drag when no input
        if (movement.magnitude == 0)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, drag * Time.fixedDeltaTime);
        }

        // Surface limit: Prevent moving above water
        if (transform.position.y > surfaceY || transform.position.y < maxDepthY)
        {
            // Clamp position
            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);

            // Stop upward movement
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f);
            }
        }
    }

    void StartArmAction()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 direction = (mouseWorld - transform.position).normalized;

        // Set target position
        if (armTransform != null)
        {
            armTargetPos = armStartPos + (Vector3)(direction * armExtendDistance);
            isExtending = true;
        }

        // STILL collect even if no sprite exists
        CollectItems(direction);
    }
    void CollectItems(Vector2 forward)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, collectRange, itemLayer);

        foreach (Collider2D hit in hits)
        {
            Vector2 directionToItem = (hit.transform.position - transform.position).normalized;

            float angle = Vector2.Angle(forward, directionToItem);

            if (angle <= collectAngle)
            {
                Item item = hit.GetComponent<Item>();

                if (item != null)
                {
                    TryCollect(item);
                }
            }
        }
    }

    void TryCollect(Item item)
    {
        if (stats.GetArmStrength() <= item.weight)
        {
            Debug.Log("Cannot collect item: " + item.itemName + " is too heavy for your arm strength.");
            return;
        }

        // PRIORITY 1: Put into storage if possible
        if (inventory.HasSpace())
        {
            inventory.AddItem(item);
            item.OnCollected();
            return;
        }
        else if (heldItem == null) // PRIORITY 2: Hold in hand if empty
        {
            heldItem = item;
            item.OnCollected();
            return;
        }
        else // PRIORITY 3: Cannot collect, inventory full and already holding an item
        {
            Debug.Log("Cannot collect item: Return to base and sell items to free up space.");
        }
    }

    void UpdateStorageUI()
    {
        if (storageText == null || inventory == null) return;

        int current = inventory.items.Count;
        int max = stats.GetStorageCapacity();

        if (max <= 0)
        {
            storageText.text = "";
            return;
        }

        storageText.text = "Storage: " + current + " / " + max;

        float ratio = (float)current / max;

        if (ratio < 0.5f)
            storageText.color = Color.white;
        else if (ratio < 0.9f)
            storageText.color = Color.yellow;
        else
            storageText.color = Color.red;

        if (current >= max)
        {
            storageText.text = "Storage FULL!";
        }
    }

    void ShootTorpedo()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogError("Projectile prefab or fire point not assigned!");
            return;
        }

        // Fire towards mouse position
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 aimDirection = (mouseWorld - transform.position).normalized;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // Rotate projectile to face direction
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Get damage from submarine stats
        damageAmount = stats.GetDamage();

        // Get or add projectile component
        Torpedo projScript = projectile.GetComponent<Torpedo>();

        if (projScript != null)
        {
            projScript.Initialize(aimDirection, projectileSpeed, damageAmount, projectileLifetime);
        }
        else
        {
            projScript = projectile.AddComponent<Torpedo>();
            projScript.Initialize(aimDirection, projectileSpeed, damageAmount, projectileLifetime);
        }

        torpedoesRemaining--;
        Debug.Log("Player fired torpedo! Direction: " + aimDirection);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize collection range and angle
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, collectRange);
        Vector3 forward = transform.right * (transform.localScale.x > 0 ? 1 : -1);
        Vector3 leftBoundary = Quaternion.Euler(0, 0, collectAngle) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -collectAngle) * forward;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * collectRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * collectRange);
    }
}