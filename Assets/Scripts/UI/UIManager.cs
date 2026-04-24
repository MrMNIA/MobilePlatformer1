using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject almostGameOverpanel; // Can azaldığında gösterilecek panel
    public GameObject winPanel;
    public GameObject closeTutorialButton; // Tutorial kapatma butonu

    [Header("HUD Elements")]
    public GameObject pauseButton;
    public Text currentLevelCoinText;
    public Text finalCoinText;
    public Text addText; // Seviye sonu kazançlarını göstermek için (isteğe bağlı)

    [Header ("Almost Game Over")]
    public Text collectedCoinsText; // Toplanan coin miktarını göstermek için
    public Text respawnTimerText; // Yeniden doğma süresini göstermek için
    public GameObject respawnButton; // Joysticklerin bulunduğu canvas

    [SerializeField] private MovementJoystick movementJoystick;
    [SerializeField] private AttackJoystick attackJoystick;
    [SerializeField] private PlayerRespawn playerRespawn; // PlayerRespawn referansı

    [Header("Audio")]
    public AudioClip gameOverSound;
    public AudioClip winSound;

    public static UIManager instance;

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        Time.timeScale = 1f;
        pauseButton.SetActive(true);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        almostGameOverpanel.SetActive(false);
        winPanel.SetActive(false);
        closeTutorialButton.SetActive(false);
        UpdateLayout();
        UpdateCurrentLevelCoinUI(0);
    }

    private void UpdateLayout()
    {
        SettingsManager.Instance.LoadLayout();

        // 2. Sahnedeki tüm HUD elementlerini bul
        HUDElementController[] allElements = FindObjectsOfType<HUDElementController>();

        // 3. Hepsine "Pozisyonunu dosyaya göre güncelle" emrini ver
        foreach (var element in allElements)
        {
            element.ApplyMySettings();
        }
    }

    public void UpdateCurrentLevelCoinUI(int amount)
    {
        if (currentLevelCoinText != null)
            currentLevelCoinText.text = amount.ToString();
    }
    public void PauseGame()
    {
        SoundManager.Instance.PauseMusic();
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        pauseButton.SetActive(false); // Oyun duruyorken pause butonu gizlensin
    }

    // 2. Devam Etme (Pause menüsündeki Resume butonuna basınca çalışır)
    public void ResumeGame()
    {
        SoundManager.Instance.ResumeMusic();
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        pauseButton.SetActive(true); // Pause butonu geri gelsin
    }
    // RESTART
    public void RestartGame()
    {
        StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        Time.timeScale = 1f; // Sahne yüklenmeden zamanı açmalıyız
        yield return StartCoroutine(SceneFader.Instance.ManualFadeOut());
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // NEXT LEVEL
    public void LoadNextLevel()
    {
        DifficultyManager.Instance.UsePowerup();

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            StartCoroutine(LoadLevelRoutine(nextSceneIndex));
        }
        else
        {
            GotoMainMenu();
        }
    }

    private IEnumerator LoadLevelRoutine(int index)
    {
        Time.timeScale = 1f;
        yield return StartCoroutine(SceneFader.Instance.ManualFadeOut());
        SceneManager.LoadScene(index);
    }

    // MAIN MENU
    public void GotoMainMenu()
    {
        DifficultyManager.Instance.UsePowerup();
        StartCoroutine(MainMenuRoutine());
    }

    private IEnumerator MainMenuRoutine()
    {
        Time.timeScale = 1f;
        yield return StartCoroutine(SceneFader.Instance.ManualFadeOut());
        SceneManager.LoadScene(0);
    }

    public void LockJoysticks()
    {
        movementJoystick.ChangeAbleToMove(false);
        attackJoystick.ChangeAbleToAttack(false);
    }

    public void UnlockJoysticks()
    {
        movementJoystick.ChangeAbleToMove(true);
        attackJoystick.ChangeAbleToAttack(true);
    }

    // 3. Oyun Sonu (Ölünce çağrılır)
    public void ShowGameOver()
    {
        SoundManager.Instance.PlaySound(gameOverSound);
        SoundManager.Instance.PauseMusic();
        Time.timeScale = 0f;
        DifficultyManager.Instance.UsePowerup();
        almostGameOverpanel.SetActive(false); // Eğer can azaldığında gösterilen panel açıksa kapat
        gameOverPanel.SetActive(true);
        pauseButton.SetActive(false);
    }

    public void ShowAlmostGameOver()
    {
        almostGameOverpanel.SetActive(true);
        pauseButton.SetActive(false);
        Time.timeScale = 0f; // Zamanı durdur
        collectedCoinsText.text = MoneyManager.Instance.currentLevelCoins.ToString(); // Toplanan coin miktarını göster
        StartCoroutine(AlmostGameOverTimer());
    }

    private IEnumerator AlmostGameOverTimer()
    {
        float timer = 5f; // Örnek süre, istediğiniz gibi ayarlayabilirsiniz
        while (timer > 0)
        {
            if (playerRespawn.IsRespawned())
            {
                Respawn(); // Eğer oyuncu yeniden doğduysa, respawn işlemini gerçekleştir
                yield break; // Coroutine'i sonlandır
            }
            timer -= Time.unscaledDeltaTime; // Zaman durduğu için unscaledDeltaTime kullanıyoruz
            respawnTimerText.text = timer.ToString("F1"); // Kalan süreyi göster
            float progress = 1f - (timer / 5f);

            // Lerp ile 30/255'ten 100/255'e geçiş yapıyoruz
            float startAlpha = 0.2f;
            float endAlpha = 0.7f;

            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, progress);

            Color color = almostGameOverpanel.GetComponent<Image>().color;
            color.a = currentAlpha; // Alpha değerini güncelle
            almostGameOverpanel.GetComponent<Image>().color = color; // Yeni rengi uygula
            yield return null;
        }
        ShowGameOver(); // Süre dolunca Game Over panelini göster
    }

    public void Respawn()
    {
        playerRespawn.Respawn();
        almostGameOverpanel.SetActive(false);
        pauseButton.SetActive(true);
        StopCoroutine(AlmostGameOverTimer());
        Time.timeScale = 1f; // Zamanı tekrar aç
        UnlockJoysticks();
    }
    // 4. Kazanma (Win Zone'a girince çağrılır)
    public IEnumerator ShowWinScreen(int baseAmount, bool isFirstClear)
    {
        SoundManager.Instance.PlaySound(winSound);

        // Zamanı durdurduğunuz için 'Realtime' bekleme kullanmalıyız
        Time.timeScale = 0f;
        winPanel.SetActive(true);
        pauseButton.SetActive(false);

        // Bekleme süresini değişkene atayalım (Performans için)
        float waitTime = 1f / baseAmount; // Miktara göre hızlanır
        var wait = new WaitForSecondsRealtime(waitTime);

        // 1. AŞAMA: Temel Miktar Artışı
        for (int i = 0; i <= baseAmount; i++)
        {
            finalCoinText.text = i.ToString();
            yield return wait; // Bekleme burada gerçekleşir
        }

        int tempAmount = baseAmount;
        yield return new WaitForSecondsRealtime(0.5f); // Küçük bir duraklama ekleyelim

        // 2. AŞAMA: Zorluk Bonusu
        if (DifficultyManager.Instance != null && DifficultyManager.Instance.currentDifficulty != DifficultyManager.Difficulty.Easy)
        {
            baseAmount = Mathf.RoundToInt(baseAmount * DifficultyManager.Instance.GetCoinMultiplier());

            if (DifficultyManager.Instance.currentDifficulty == DifficultyManager.Difficulty.Medium)
            {
                addText.text = "Medium Bonus: x1.25";
                addText.color = Color.yellow;
            }
            else if (DifficultyManager.Instance.currentDifficulty == DifficultyManager.Difficulty.Hard)
            {
                addText.text = "Hard Bonus: x1.5";
                addText.color = Color.red;
            }

            waitTime = 1f / baseAmount; // Miktara göre hızlanır
            wait = new WaitForSecondsRealtime(waitTime);

            for (int i = tempAmount; i <= baseAmount; i++)
            {
                finalCoinText.text = i.ToString();
                yield return wait;
            }

            tempAmount = baseAmount;
        }

        yield return new WaitForSecondsRealtime(0.5f); // Küçük bir duraklama ekleyelim

        // 3. AŞAMA: İlk Bitirme Bonusu
        if (isFirstClear)
        {
            addText.text = "First Clear Bonus: x2"; // Alt satıra geçmesi için \n ekledim
            addText.color = Color.green;
            baseAmount = Mathf.RoundToInt(baseAmount * 2);

            waitTime = 1f / baseAmount; // Miktara göre hızlanır
            wait = new WaitForSecondsRealtime(waitTime);

            for (int i = tempAmount; i <= baseAmount; i++)
            {
                finalCoinText.text = i.ToString();
                yield return wait;
            }
        }
    }
}