using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject[] monsterPrefabs;

    public Vector2 minBounds;
    public Vector2 maxBounds;

    public float baseInterval = 4f;
    public int maxCount = 10;

    public Transform player;

    private int currentCount;

    void Start()
    {
        InvokeRepeating(nameof(Spawn), 2f, baseInterval);
    }

    void Spawn()
    {
        if (currentCount >= maxCount) return;

        float depth = Mathf.Abs(player.position.y);

        // Faster spawn when deeper
        float interval = Mathf.Clamp(baseInterval - (depth / 100f), 0.5f, baseInterval);

        CancelInvoke(nameof(Spawn));
        InvokeRepeating(nameof(Spawn), interval, interval);

        GameObject prefab = GetValidPrefab(depth);
        if (prefab == null) return;

        Vector2 pos = GetRandomPosition();

        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        Register(obj);
    }

    GameObject GetValidPrefab(float depth)
    {
        foreach (var prefab in monsterPrefabs)
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