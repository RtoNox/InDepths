using UnityEngine;
using TMPro;

public class EndingSceneController : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public AudioSource audioSource;

    public AudioClip badClip;
    public AudioClip neutralClip;
    public AudioClip goodClip;
    public AudioClip trueClip;

    void Start()
    {
        var ending = GameManager.Instance.currentEnding;

        switch (ending)
        {
            case GameManager.EndingType.Worst:
                titleText.text = "LOST TO THE DEPTHS";
                descriptionText.text = "You went too far...";
                audioSource.clip = badClip;
                break;

            case GameManager.EndingType.Bad:
                titleText.text = "SURVIVED... BARELY";
                descriptionText.text = "But the debt remains.";
                audioSource.clip = neutralClip;
                break;

            case GameManager.EndingType.Good:
                titleText.text = "DEBT CLEARED";
                descriptionText.text = "You made it out alive.";
                audioSource.clip = goodClip;
                break;

            case GameManager.EndingType.True:
                titleText.text = "TRUE FREEDOM";
                descriptionText.text = "You conquered the depths.";
                audioSource.clip = trueClip;
                break;
        }

        audioSource.Play();
    }
}