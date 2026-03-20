using UnityEngine;

public class DespawnObject : MonoBehaviour
{
    public Transform player;

    [Header("Despawn Settings")]
    public float despawnDistance = 40f;
    public float checkInterval = 1f;

    private float timer;

    void Update()
    {
        if (player == null) return;

        timer += Time.deltaTime;

        if (timer >= checkInterval)
        {
            timer = 0f;

            float distance = Vector2.Distance(transform.position, player.position);

            if (distance > despawnDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, despawnDistance);
        }
    }
}