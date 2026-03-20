using UnityEngine;

public class Spawnable : MonoBehaviour
{
    [Header("Spawn Depth Range")]
    public float minDepth;
    public float maxDepth;

    [Header("Spawn Weight")]
    public float baseWeight = 1f;

    [Tooltip("Depth where this is MOST common")]
    public float idealDepth = 0f;

    [Tooltip("How quickly spawn chance falls off from ideal depth")]
    public float falloff = 50f;

    public float GetWeight(float currentDepth)
    {
        if (currentDepth < minDepth || currentDepth > maxDepth)
            return 0f;

        // Distance from ideal depth
        float distance = Mathf.Abs(currentDepth - idealDepth);

        // Weight decreases as you move away from ideal depth
        float weight = baseWeight * Mathf.Clamp01(1f - (distance / falloff));

        return weight;
    }

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

    public System.Action onDestroyed;
    void OnDestroy()
    {
        onDestroyed?.Invoke();
    }
}