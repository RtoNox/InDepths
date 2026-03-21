using UnityEngine;

public class ShopSystem : MonoBehaviour
{
    private Inventory inventory;
    private PlayerCurrency currency;

    public void Initialize(Inventory inv, PlayerCurrency curr)
    {
        inventory = inv;
        currency = curr;
    }

    public void SellItems()
    {
        int totalValue = 0;

        foreach (Item item in inventory.items)
        {
            totalValue += item.value;
        }

        currency.AddMoney(totalValue);
        inventory.items.Clear();

        Debug.Log("Sold everything for: " + totalValue);
    }
}