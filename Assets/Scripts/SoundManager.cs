using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Ses Ayarları")]
    public Text musicVolumeText;
    public Text sfxVolumeText;

    [Header("Müzik Kütüphanesi")]
    public AudioClip menuMusic;
    public AudioClip[] chapterMusic = new AudioClip[3]; // 0: 1-10, 1: 11-20, 2: 21-30
    public AudioClip bossMusic;

    [Header("Ses Efektleri")]
    public AudioClip buttonClickSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            LoadAllSettings();
        }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single); // Başlangıçta müziği ayarla
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int index = scene.buildIndex;

        if (index == 0)
            ChangeMusic(menuMusic);
        else if (index >= 1 && index <= 10)
            ChangeMusic(chapterMusic[0]);
        else if (index >= 11 && index <= 20)
            ChangeMusic(chapterMusic[1]);
        else if (index >= 21 && index <= 30)
            ChangeMusic(chapterMusic[2]);
    }

    public void ChangeMusic(AudioClip newClip)
    {
        if (newClip == null || musicSource.clip == newClip) return;
        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayBossMusic() => ChangeMusic(bossMusic);

    public void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlaybuttonClickSound() => PlaySound(buttonClickSound);

    // --- SES AYARLARI (%0 - %100) ---

    // Müzik butonları için (Unity Inspector'dan bunu çağır)
    public void AdjustMusic(int amount)
    {
        UpdateVolume(amount, musicSource, "MusicVol");
    }

    // SFX butonları için (Unity Inspector'dan bunu çağır)
    public void AdjustSFX(int amount)
    {
        UpdateVolume(amount, sfxSource, "SFXVol");
    }

    public void PauseMusic() => musicSource.Pause();
    public void ResumeMusic() => musicSource.UnPause();


    private void UpdateVolume(int amount, AudioSource source, string prefKey)
    {
        int current = PlayerPrefs.GetInt(prefKey, 100);
        current = Mathf.Clamp(current + amount, 0, 100);

        PlayerPrefs.SetInt(prefKey, current);
        source.volume = current / 100f; // Ses seviyesini 0.0 - 1.0 arasına çeker

        UpdateDisplay(); // Opsiyonel: Ayarları ekranda güncellemek için çağırabilirsiniz

        PlaybuttonClickSound();
    }

    public void UpdateDisplay()
    {
        if (musicVolumeText != null)
            musicVolumeText.text = PlayerPrefs.GetInt("MusicVol", 100).ToString();

        if (sfxVolumeText != null)
            sfxVolumeText.text = PlayerPrefs.GetInt("SFXVol", 100).ToString();
    }
    private void LoadAllSettings()
    {
        musicSource.volume = PlayerPrefs.GetInt("MusicSource", 100) / 100f;
        sfxSource.volume = PlayerPrefs.GetInt("SoundSource", 100) / 100f;
    }
}