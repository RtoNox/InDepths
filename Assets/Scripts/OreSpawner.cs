using UnityEngine;

public class OreSpawner : MonoBehaviour
{
    public GameObject[] orePrefabs;

    public Vector2 minBounds;
    public Vector2 maxBounds;

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

        GameObject prefab = GetValidPrefab(depth);
        if (prefab == null) return;

        Vector2 pos = GetRandomPosition();

        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        Register(obj);
    }

    GameObject GetValidPrefab(float depth)
    {
        // Prefer deeper ores
        GameObject selected = null;

        foreach (var prefab in orePrefabs)
        {
            Spawnable s = prefab.GetComponent<Spawnable>();

            if (s != null && depth >= s.minDepth && depth <= s.maxDepth)
            {
                selected = prefab;
            }
        }

        return selected;
    }

    Vector2 GetRandomPosition()
    {
        return new Vector2(
            Random.Range(minBounds.x, maxBounds.x),
            Random.Range(minBounds.y, maxBounds.y)
        );
    }

    void Register(GameObject obj)
    {
        currentCount++;

        Spawnable so = obj.GetComponent<Spawnable>();
        if (so != null)
            so.onDestroyed += () => currentCount--;
    }
}