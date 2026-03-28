using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource ambienceSource;
    public AudioSource sfxSource;

    void Awake()
    {
        Instance = this;
    }

    public void PlayAmbience(AudioClip clip)
    {
        ambienceSource.clip = clip;
        ambienceSource.loop = true;
        ambienceSource.Play();
    }

    public void StopAmbience()
    {
        ambienceSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource.isPlaying)
            sfxSource.Stop();

        sfxSource.PlayOneShot(clip);
    }
}