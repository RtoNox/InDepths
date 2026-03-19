using UnityEngine;

public class Spawnable : MonoBehaviour
{
    [Header("Spawn Depth Range")]
    public float minDepth;
    public float maxDepth;

    public System.Action onDestroyed;

    void OnDestroy()
    {
        onDestroyed?.Invoke();
    }
}