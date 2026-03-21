using UnityEngine;
using TMPro;

public class PlayerCurrency : MonoBehaviour
{
    public int money = 0;
    public TextMeshProUGUI moneyText;

    void Update()
    {
        moneyText.text = "Money: $" + money;
    }

    public void AddMoney(int amount)
    {
        money += amount;
        Debug.Log("Money: " + money);
    }

    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            return true;
        }

        return false;
    }
}