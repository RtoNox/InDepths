using UnityEngine;

public class Item : MonoBehaviour
{
    public string itemName;
    public int value; // for selling later

    public void OnCollected()
    {
        Destroy(gameObject);
    }
}