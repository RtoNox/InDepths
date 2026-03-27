using Unity.VisualScripting;
using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    public Transform player;
    public float bossArenaY = 8000f;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
    }

    void FixedUpdate()
    {
        if (player == null)
            return;

        if (Mathf.Abs(player.transform.position.y) >= bossArenaY)
        {
            GameManager.Instance.isBossFightActive = true;
            gameObject.SetActive(false);
        }
    }
}