using UnityEngine;

public class ShopSystem : MonoBehaviour
{
    private Inventory inventory;
    private PlayerCurrency currency;

    void Awake()
    {
        inventory = GetComponent<Inventory>();
        currency = GetComponent<PlayerCurrency>();
    }

    public void SellItems()
    {
        int totalValue = 0;

        foreach (Item item in inventory.items)
        {
            totalValue += item.value;
        }

        // Give money
        currency.AddMoney(totalValue);

        // Clear inventory
        inventory.items.Clear();

        Debug.Log("Sold everything for: " + totalValue);
    }
}