using UnityEngine;

public class OreSpawner : MonoBehaviour
{
    public GameObject[] orePrefabs;

    public float minBounds;
    public float maxBounds;

    public float spawnInterval = 5f;
    public int maxCount = 15;

    public Transform player;

    private int currentCount;

    void Start()
    {
        InvokeRepeating(nameof(Spawn), 2f, spawnInterval);
    }

    void Spawn()
    {
        if (currentCount >= maxCount) return;

        float depth = Mathf.Abs(player.position.y);

        GameObject prefab = GetWeightedPrefab(orePrefabs, depth);
        if (prefab == null) return;

        Vector2 pos = GetSafeSpawnPosition(player, minBounds, maxBounds);

        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        Register(obj);
    }

    GameObject GetWeightedPrefab(GameObject[] prefabs, float depth)
    {
        float totalWeight = 0f;

        float[] weights = new float[prefabs.Length];

        for (int i = 0; i < prefabs.Length; i++)
        {
            Spawnable s = prefabs[i].GetComponent<Spawnable>();

            if (s == null) continue;

            float w = s.GetWeight(depth);
            weights[i] = w;
            totalWeight += w;
        }

        if (totalWeight <= 0f) return null;

        float random = Random.Range(0, totalWeight);

        float cumulative = 0f;

        for (int i = 0; i < prefabs.Length; i++)
        {
            cumulative += weights[i];

            if (random <= cumulative)
            {
                return prefabs[i];
            }
        }

        return null;
    }

    Vector2 GetSafeSpawnPosition(Transform player, float minDistance, float maxDistance)
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        float distance = Random.Range(minDistance, maxDistance);

        return (Vector2)player.position + dir * distance;
    }

    void Register(GameObject obj)
    {
        currentCount++;

        Spawnable so = obj.GetComponent<Spawnable>();
        if (so != null)
            so.onDestroyed += () => currentCount--;
    }
}