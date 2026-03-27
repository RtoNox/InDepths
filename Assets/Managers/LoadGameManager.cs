using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class LoadGameManager : MonoBehaviour
{
    [Header("Canvas References")]
    [SerializeField] private GameObject SaveLoadCanvas;
    [SerializeField] private GameObject MainMenuCanvas;
    
    [Header("Buttons")]
    [SerializeField] private Button backButton;
    
    
    [Header("Audio Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    
    void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonPressed);
        
    }
    
    // Master Volume Control
    
    public void OnBackButtonPressed()
    {
        PlayClickSound();
        Debug.Log("Closing...");
        
        // Save settings before closing
        PlayerPrefs.Save();
        
        // Disable settings canvas, enable main menu canvas
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