using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingSceneController : MonoBehaviour
{
    public GameObject worstEnding;
    public GameObject badEnding;
    public GameObject goodEnding;
    public GameObject trueEnding;

    public AudioSource audioSource;
    public AudioClip worstClip;
    public AudioClip badClip;
    public AudioClip goodClip;
    public AudioClip trueClip;

    void Start()
    {
        var ending = GameManager.Instance.currentEnding;

        switch (ending)
        {
            case GameManager.EndingType.Worst:
                worstEnding.SetActive(true);
                audioSource.clip = worstClip;
                break;

            case GameManager.EndingType.Bad:
                badEnding.SetActive(true);
                audioSource.clip = badClip;
                break;

            case GameManager.EndingType.Good:
                goodEnding.SetActive(true);
                audioSource.clip = goodClip;
                break;

            case GameManager.EndingType.True:
                trueEnding.SetActive(true);
                audioSource.clip = trueClip;
                break;
        }

        GameManager.Instance.GameCompleted();
        audioSource.Play();
    }

    public void QuitToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}