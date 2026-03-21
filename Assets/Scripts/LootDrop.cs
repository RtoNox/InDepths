using UnityEngine;

public class LootDrop : MonoBehaviour
{
    [Header("Drop Settings")]
    public GameObject dropPrefab; // assign carcass prefab
    public int dropAmount = 1;

    public void DropLoot()
    {
        if (dropPrefab == null) return;

        for (int i = 0; i < dropAmount; i++)
        {
            Instantiate(
                dropPrefab,
                (Vector2)transform.position,
                Quaternion.identity
            );
        }
    }
}