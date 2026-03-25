using UnityEngine;

public class ShopUIController : MonoBehaviour
{
    public GameObject shopUI; // assign your Canvas panel here
    public GameObject PlayerHUD; // assign the player's HUD here

    private bool isOpen = false;

    public RectTransform shopPanel;

    [Header("Scroll Settings")]
    public float scrollSpeed = 500f;

    [Header("Bounds")]
    public float minX; // left limit
    public float maxX; // right limit

    void Update()
    {
        if (!isOpen)
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            Vector2 pos = shopPanel.anchoredPosition;

            pos.x -= scroll * scrollSpeed;

            // Clamp so it doesn't go too far
            pos.x = Mathf.Clamp(pos.x, minX, maxX);

            shopPanel.anchoredPosition = pos;
        }
    }
    public void OpenShop()
    {
        shopUI.SetActive(true);
        PlayerHUD.SetActive(false);
        isOpen = true;
    }

    public void CloseShop()
    {
        shopUI.SetActive(false);
        PlayerHUD.SetActive(true);
        isOpen = false;
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}