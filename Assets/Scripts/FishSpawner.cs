using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public GameObject[] fishPrefabs;

    public Vector2 minBounds;
    public Vector2 maxBounds;

    public float baseSpawnInterval = 2f;
    public int maxCount = 20;

    public Transform player;

    private int currentCount;

    void Start()
    {
        InvokeRepeating(nameof(Spawn), 1f, baseSpawnInterval);
    }

    void Spawn()
    {
        if (currentCount >= maxCount) return;

        float depth = Mathf.Abs(player.position.y);

        // More delay when deeper (fish become rarer)
        float depthMultiplier = Mathf.Clamp(depth / 100f, 1f, 5f);
        CancelInvoke(nameof(Spawn));
        InvokeRepeating(nameof(Spawn), depthMultiplier, depthMultiplier);

        GameObject prefab = GetValidPrefab(depth);
        if (prefab == null) return;

        Vector2 pos = GetRandomPosition();

        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        Register(obj);
    }

    GameObject GetValidPrefab(float depth)
    {
        foreach (var prefab in fishPrefabs)
        {
            Spawnable s = prefab.GetComponent<Spawnable>();

            if (s != null && depth >= s.minDepth && depth <= s.maxDepth)
                return prefab;
        }

        return null;
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