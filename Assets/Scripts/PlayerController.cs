using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 5f;
    public float drag = 4f;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Get input (WASD)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        movement = movement.normalized;
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // Smooth underwater-like movement
        Vector2 targetVelocity = movement * moveSpeed;
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        // Apply drag when no input
        if (movement.magnitude == 0)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, drag * Time.fixedDeltaTime);
        }
    }
}