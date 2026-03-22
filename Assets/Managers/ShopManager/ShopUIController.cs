using UnityEngine;

public class ShopUIController : MonoBehaviour
{
    public GameObject shopUI; // assign your Canvas panel here
    public GameObject PlayerHUD; // assign the player's HUD here

    private bool isOpen = false;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            CloseShop();
        }
    }
    public void OpenShop()
    {
        shopUI.SetActive(true);
        PlayerHUD.SetActive(false);
        isOpen = true;
        Time.timeScale = 0f; // pause game (optional but nice)
    }

    public void CloseShop()
    {
        shopUI.SetActive(false);
        PlayerHUD.SetActive(true);
        isOpen = false;
        Time.timeScale = 1f;
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}