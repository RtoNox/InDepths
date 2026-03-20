using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MainMenuSettingManager : MonoBehaviour
{
    [Header("Canvas References")]
    [SerializeField] private GameObject settingsCanvas;
    [SerializeField] private GameObject mainMenuCanvas;
    
    [Header("Buttons")]
    [SerializeField] private Button backButton;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Text masterVolumeText; // Optional: displays percentage
    
    [Header("Display Settings")]
    [SerializeField] private Toggle fullscreenToggle;
    
    [Header("Audio Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    
    void Start()
    {
        // Setup back button
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonPressed);
        
        // Load saved settings
        LoadSettings();
        
        // Setup master volume slider
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            OnMasterVolumeChanged(masterVolumeSlider.value);
        }
        
        // Setup fullscreen toggle
        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggle);
            OnFullscreenToggle(fullscreenToggle.isOn);
        }
    }
    
    // Master Volume Control
    void OnMasterVolumeChanged(float volume)
    {
        // Convert linear 0-1 value to decibels (-80dB to 0dB)
        float dB = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20;
        if (audioMixer != null)
            audioMixer.SetFloat("MasterVolume", dB);
        
        // Update volume percentage text (optional)
        if (masterVolumeText != null)
            masterVolumeText.text = Mathf.RoundToInt(volume * 100).ToString() + "%";
        
        // Save volume setting
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }
    
    // Fullscreen Control
    void OnFullscreenToggle(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }
    
    // Back Button Function
    public void OnBackButtonPressed()
    {
        PlayClickSound();
        Debug.Log("Closing settings...");
        
        // Save settings before closing
        PlayerPrefs.Save();
        
        // Disable settings canvas, enable main menu canvas
        if (settingsCanvas != null)
            settingsCanvas.SetActive(false);
        
        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(true);
    }
    
    // Load saved settings
    void LoadSettings()
    {
        // Load master volume
        if (masterVolumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
            masterVolumeSlider.value = savedVolume;
            OnMasterVolumeChanged(savedVolume);
        }
        
        // Load fullscreen setting
        if (fullscreenToggle != null)
        {
            bool savedFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            fullscreenToggle.isOn = savedFullscreen;
            OnFullscreenToggle(savedFullscreen);
        }
    }
    
    void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
    
    // Optional: Call this when the game exits to ensure settings are saved
    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}