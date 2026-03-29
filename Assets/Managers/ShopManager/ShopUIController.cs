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

    private SubmarineStats stats;
    public GameObject hintPanel;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        stats = player.GetComponent<SubmarineStats>();
    }

    void Update()
    {
        if (!isOpen)
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float horizontalInput = Input.GetAxis("Horizontal");

        if (scroll != 0)
        {
            Vector2 pos = shopPanel.anchoredPosition;

            pos.x -= scroll * scrollSpeed;

            // Clamp so it doesn't go too far
            pos.x = Mathf.Clamp(pos.x, minX, maxX);

            shopPanel.anchoredPosition = pos;
        }

        if (horizontalInput != 0)
        {
            Vector2 pos = shopPanel.anchoredPosition;

            pos.x -= horizontalInput * scrollSpeed * Time.deltaTime;

            // Clamp so it doesn't go too far
            pos.x = Mathf.Clamp(pos.x, minX, maxX);

            shopPanel.anchoredPosition = pos;
        }

        if (stats.vitalityLevel >= 75)
        {
            maxX = 900;
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
        GameManager.Instance.StartNewDay();
    }

    public void ShowHint()
    {
        hintPanel.SetActive(true);
    }

    public void CloseHint()
    {
        hintPanel.SetActive(false);
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}