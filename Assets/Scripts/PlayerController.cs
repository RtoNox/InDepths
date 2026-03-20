using UnityEngine;
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

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 5f;
    public float drag = 4f;

    [Header("Robot Arm")]
    public Transform armTransform; // optional (can be null)
    public float armExtendDistance = 2f;
    public float armExtendSpeed = 10f;

    private bool isExtending = false;
    private Vector3 armStartPos;
    private Vector3 armTargetPos;
    private Rigidbody2D rb;
    private Vector2 movement;

    [Header("Item Collection")]
    public float collectRange = 3f;
    public float collectAngle = 30f; // cone angle
    public LayerMask itemLayer; // Layer for items to collect

    private Inventory inventory;
    public Item heldItem;

    [Header("Game UI")]
    public TextMeshProUGUI storageText;

    [Header("Shop UI")]
    private ShopUIController shopUI;
    private ShopSystem shopSystem;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inventory = GetComponent<Inventory>();

        if (armTransform != null)
        {
            armStartPos = armTransform.localPosition;
        }
    }

    void Update()
    {
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

        UpdateStorageUI();

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

        if (Input.GetMouseButtonDown(0) && currentState == PlayerState.ArmCollect)
        {
            StartArmAction();
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

        if (transform.position.y >= 0f)
        {
            if (Input.GetKeyDown(KeyCode.F) && !shopUI.IsOpen())
            {
                shopSystem.SellItems(); // SELL FIRST
                shopUI.OpenShop();
            }
        }
    }

    void FixedUpdate()
    {
        // Smooth underwater-like movement
        Vector2 targetVelocity = movement * moveSpeed;
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        // Apply drag when no input
        if (movement.magnitude == 0)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, drag * Time.fixedDeltaTime);
        }

        // Surface limit: Prevent moving above water
        if (transform.position.y > 0f)
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
        int max = GetComponent<SubmarineStats>().GetStorageCapacity();

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