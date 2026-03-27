using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadGameManager : MonoBehaviour
{
    [Header("Canvas References")]
    [SerializeField] private GameObject SaveLoadCanvas;
    [SerializeField] private GameObject MainMenuCanvas;

    [Header("Buttons")]
    [SerializeField] private Button backButton;

    [Header("Save Slots")]
    [SerializeField] private Button[] slotButtons; // size = 3
    [SerializeField] private TextMeshProUGUI[] slotTexts; // size = 3

    [Header("Audio Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonPressed);

        SetupSlots();
    }

    void SetupSlots()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slotIndex = i;

            if (SaveSystem.SaveExists(slotIndex))
            {
                SaveData data = SaveSystem.LoadGame(slotIndex);

                slotTexts[i].text =
                    "Slot " + (i + 1) +
                    "\nDay: " + data.currentDay +
                    "\nMoney: $" + data.money;

                slotButtons[i].onClick.RemoveAllListeners();
                slotButtons[i].onClick.AddListener(() => OnLoadGame(slotIndex));
            }
            else
            {
                slotTexts[i].text =
                    "Slot " + (i + 1) +
                    "\n<Empty>";

                slotButtons[i].onClick.RemoveAllListeners();
                slotButtons[i].onClick.AddListener(() => OnNewGame(slotIndex));
            }
        }
    }

    // === SLOT ACTIONS ===

    void OnNewGame(int slot)
    {
        PlayClickSound();

        Debug.Log("Starting new game in slot: " + slot);

        GameManager.Instance.currentSaveSlot = slot;

        GameManager.Instance.currentDay = 1;
        GameManager.Instance.debt = 10000000;

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    void OnLoadGame(int slot)
    {
        PlayClickSound();

        Debug.Log("Loading game from slot: " + slot);

        GameManager.Instance.currentSaveSlot = slot;

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    public void DeleteSave(int slot)
    {
        PlayClickSound();

        SaveSystem.DeleteSave(slot);

        Debug.Log("Deleted save in slot: " + slot);

        SetupSlots(); // refresh UI
    }

    // === BACK BUTTON ===

    public void OnBackButtonPressed()
    {
        PlayClickSound();
        Debug.Log("Closing...");

        PlayerPrefs.Save();

        if (SaveLoadCanvas != null)
            SaveLoadCanvas.SetActive(false);

        if (MainMenuCanvas != null)
            MainMenuCanvas.SetActive(true);
    }

    void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}