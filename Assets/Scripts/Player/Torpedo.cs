using UnityEngine;

public class Torpedo : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private int damage;
    private float lifetime;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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

        // Rotate projectile to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Projectile hit: {other.gameObject.name} with tag: {other.tag}"); // Add this for debugging

        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($"Projectile hit {other.gameObject.name} for {damage} damage!");
            }
            else
            {
                // Try to find Health in parent or children
                enemyHealth = other.GetComponentInParent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                    Debug.Log($"Projectile hit {other.gameObject.name} (found Health in parent) for {damage} damage!");
                }
                else
                {
                    enemyHealth = other.GetComponentInChildren<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(damage);
                        Debug.Log($"Projectile hit {other.gameObject.name} (found Health in child) for {damage} damage!");
                    }
                    else
                    {
                        Debug.LogError("Enemy has no Health component!");
                    }
                }
            }
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacles") || !other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}