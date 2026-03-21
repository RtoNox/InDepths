using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button SettingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private GameObject settingsCanvas;
    [SerializeField] private GameObject mainMenuCanvas; // Add reference to main menu canvas
    
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1f;
        
        // Ensure main menu is active and settings is inactive at start
        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(true);
        
        if (settingsCanvas != null)
            settingsCanvas.SetActive(false);
        
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
        
        if (SettingsButton != null)
            SettingsButton.onClick.AddListener(OpenSettings);
        
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
    }
    
    void StartGame()
    {
        PlayClickSound();
        Debug.Log("Starting game...");
        
        SceneManager.LoadScene("MainScene");
    }
    
    void OpenSettings()
    {
        PlayClickSound();
        Debug.Log("Opening settings...");
        
        // Disable main menu, enable settings
        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(false);
        
        if (settingsCanvas != null)
            settingsCanvas.SetActive(true);
    }
    
    void CloseSettings()
    {
        PlayClickSound();
        Debug.Log("Closing settings...");
        
        // Disable settings, enable main menu
        if (settingsCanvas != null)
            settingsCanvas.SetActive(false);
        
        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(true);
    }
    
    void ExitGame()
    {
        PlayClickSound();
        Debug.Log("Exiting game...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
    
    void Update()
    {
        // Only start game if main menu is active
        if (mainMenuCanvas != null && mainMenuCanvas.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                StartGame();
        }
        
        // Handle ESC key to close settings or exit game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If settings is active, close it
            if (settingsCanvas != null && settingsCanvas.activeInHierarchy)
            {
                CloseSettings();
            }
            // Otherwise, exit the game (only if main menu is active)
            else if (mainMenuCanvas != null && mainMenuCanvas.activeInHierarchy)
            {
                ExitGame();
            }
        }
    }
}