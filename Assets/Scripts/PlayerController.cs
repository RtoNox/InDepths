using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 5f;
    public float drag = 4f;

    private Rigidbody2D rb;
    private Vector2 movement;

    [Header("Item Collection")]
    public float collectRange = 3f;
    public float collectAngle = 45f; // cone angle
    public LayerMask itemLayer; // Layer for items to collect

    private Inventory inventory;
    public Item heldItem;

    [Header("Robot Arm")]
    public Transform armTransform; // optional (can be null)
    public float armExtendDistance = 2f;
    public float armExtendSpeed = 10f;

    private bool isExtending = false;
    private Vector3 armStartPos;
    private Vector3 armTargetPos;

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

        if (Input.GetMouseButtonDown(0))
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