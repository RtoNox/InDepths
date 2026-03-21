using UnityEngine;

public class Item : MonoBehaviour
{
    public string itemName;
    public int value; // for selling later
    public int weight; // minimum arm strength required to collect

    public void OnCollected()
    {
        Destroy(gameObject);
    }
}