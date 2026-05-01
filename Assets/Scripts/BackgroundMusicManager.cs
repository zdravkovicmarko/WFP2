using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField, Range(0f, 1f)] private float volume = 0.2f;

    private void Awake()
    {
        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.volume = volume;
    }

    private void OnEnable()
    {
        PlayMusic();
    }

    private void OnDisable()
    {
        StopMusic();
    }

    private void PlayMusic()
    {
        if (musicSource == null || backgroundMusic == null)
            return;

        musicSource.clip = backgroundMusic;
        musicSource.volume = volume;
        musicSource.loop = true;

        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    private void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }
}