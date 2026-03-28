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
    [SerializeField] private Button[] slotButtons;
    [SerializeField] private TextMeshProUGUI[] slotTexts;
    [SerializeField] private GameObject[] deleteButtons; // assign per slot

    [Header("Delete Confirmation UI")]
    [SerializeField] private GameObject deleteConfirmPanel;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    private int pendingDeleteSlot = -1;

    [Header("Audio Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonPressed);

        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(ConfirmDelete);

        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(CancelDelete);

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

                // SHOW delete button
                deleteButtons[i].SetActive(true);

                slotButtons[i].onClick.RemoveAllListeners();
                slotButtons[i].onClick.AddListener(() => OnLoadGame(slotIndex));

                // Hook delete button
                int capturedIndex = i;
                deleteButtons[i].GetComponent<Button>().onClick.RemoveAllListeners();
                deleteButtons[i].GetComponent<Button>().onClick.AddListener(() => OnDeletePressed(capturedIndex));
            }
            else
            {
                slotTexts[i].text =
                    "Slot " + (i + 1) +
                    "\n<Empty>";

                // HIDE delete button
                deleteButtons[i].SetActive(false);

                slotButtons[i].onClick.RemoveAllListeners();
                slotButtons[i].onClick.AddListener(() => OnNewGame(slotIndex));
            }
        }
    }

    // === SLOT ACTIONS ===

    void OnNewGame(int slot)
    {
        PlayClickSound();

        GameManager.Instance.currentSaveSlot = slot;
        GameManager.Instance.currentDay = 1;
        GameManager.Instance.debt = 10000000;

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    void OnLoadGame(int slot)
    {
        PlayClickSound();

        GameManager.Instance.currentSaveSlot = slot;

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    // === DELETE FLOW ===

    void OnDeletePressed(int slot)
    {
        PlayClickSound();

        pendingDeleteSlot = slot;
        deleteConfirmPanel.SetActive(true);
    }

    void ConfirmDelete()
    {
        PlayClickSound();

        if (pendingDeleteSlot != -1)
        {
            SaveSystem.DeleteSave(pendingDeleteSlot);
            Debug.Log("Deleted slot: " + pendingDeleteSlot);
        }

        pendingDeleteSlot = -1;
        deleteConfirmPanel.SetActive(false);

        SetupSlots(); // refresh UI
    }

    void CancelDelete()
    {
        PlayClickSound();

        pendingDeleteSlot = -1;
        deleteConfirmPanel.SetActive(false);
    }

    // === BACK BUTTON ===

    public void OnBackButtonPressed()
    {
        PlayClickSound();

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