
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip victorySound;
    public AudioClip defeatSound;

    [Header("Volume")]
    [Range(0f, 1f)]
    public float bgmVolume = 0.35f;

    [Range(0f, 1f)]
    public float effectVolume = 0.8f;

    private AudioSource bgmSource;
    private AudioSource effectSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 背景音樂
        bgmSource = gameObject.AddComponent<AudioSource>();

        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;
        bgmSource.volume = bgmVolume;

        // 勝利 / 失敗音效
        effectSource = gameObject.AddComponent<AudioSource>();

        effectSource.playOnAwake = false;
        effectSource.loop = false;
        effectSource.spatialBlend = 0f;
        effectSource.volume = effectVolume;
    }

    public void PlayBGM()
    {
        if (backgroundMusic == null)
            return;

        bgmSource.clip = backgroundMusic;
        bgmSource.volume = bgmVolume;

        if (!bgmSource.isPlaying)
            bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }

    public void PlayVictory()
    {
        StopBGM();

        if (victorySound != null)
            effectSource.PlayOneShot(victorySound);
    }

    public void PlayDefeat()
    {
        StopBGM();

        if (defeatSound != null)
            effectSource.PlayOneShot(defeatSound);
    }

    public void StopEffects()
    {
        if (effectSource != null)
            effectSource.Stop();
    }
}
