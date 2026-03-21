using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<Item> items = new List<Item>();

    private SubmarineStats stats;

    void Awake()
    {
        stats = GetComponent<SubmarineStats>();
    }

    public bool HasSpace()
    {
        return items.Count < stats.GetStorageCapacity();
    }
    public int GetRemainingSlots()
    {
        return stats.GetStorageCapacity() - items.Count;
    }

    public void AddItem(Item item)
    {
        items.Add(item);
    }

    public void ResetInventory()
    {
        items.Clear();
    }
}