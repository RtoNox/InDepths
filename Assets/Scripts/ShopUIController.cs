using UnityEngine;

public class ShopUIController : MonoBehaviour
{
    public GameObject shopUI; // assign your Canvas panel here

    private bool isOpen = false;

    public void OpenShop()
    {
        shopUI.SetActive(true);
        isOpen = true;
        Time.timeScale = 0f; // pause game (optional but nice)
    }

    public void CloseShop()
    {
        shopUI.SetActive(false);
        isOpen = false;
        Time.timeScale = 1f;
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}