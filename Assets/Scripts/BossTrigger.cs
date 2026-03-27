using Unity.VisualScripting;
using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    public Transform player;
    public float bossArenaY = 8000f;

    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            Debug.LogError("Player reference is missing in BossTrigger.");
            return;
        }

        if (Mathf.Abs(player.transform.position.y) >= bossArenaY)
        {
            GameManager.Instance.isBossFightActive = true;
            gameObject.SetActive(false);
        }
    }
}