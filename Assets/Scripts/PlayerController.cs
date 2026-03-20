using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 5f;
    public float drag = 4f;

    private Rigidbody2D rb;
    private Vector2 movement;

    [Header("Collection")]
    public float collectRange = 3f;
    public float collectAngle = 45f; // cone angle
    public LayerMask itemLayer;

    private Inventory inventory;
    public Item heldItem;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inventory = GetComponent<Inventory>();
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
            CollectItems();
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

    void CollectItems()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, collectRange, itemLayer);

        Vector2 forward = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;

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
}