using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public GameObject tutorialPanel; // Tutorial panelleri için dizi
    public GameObject closeTutorialButton; // Tutorial kapatma butonu

    [Header("HUD Elements")]
    public GameObject pauseButton;
    public Text currentLevelCoinText;
    public Text finalCoinText;
    public Text addText; // Seviye sonu kazançlarını göstermek için (isteğe bağlı)

    [SerializeField] private MovementJoystick movementJoystick;
    [SerializeField] private AttackJoystick attackJoystick;

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
        winPanel.SetActive(false);

        tutorialPanel.SetActive(false);
        for (int i = 0; i < tutorialPanel.transform.childCount; i++)
        {
            tutorialPanel.transform.GetChild(i).gameObject.SetActive(false); // Tüm tutorial panellerini gizle
        }
        closeTutorialButton.SetActive(false);

        UpdateCurrentLevelCoinUI(0);
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

    public void ShowTutorial(int tutorialNumber)
    {
        tutorialPanel.SetActive(true);
        foreach (Transform child in tutorialPanel.transform)
        {
            child.gameObject.SetActive(false); // Tüm tutorial panellerini gizle
        }
        tutorialPanel.transform.GetChild(tutorialNumber).gameObject.SetActive(true);
        closeTutorialButton.SetActive(true);

        attackJoystick.ChangeAbleToAttack(); // Saldırı joystickini devre dışı bırak
        movementJoystick.ChangeAbleToMove(); // Hareket joystickini devre dışı bırak
    }
    public void CloseTutorial()
    {
        SoundManager.Instance.PlaybuttonClickSound();
        tutorialPanel.SetActive(false);
        closeTutorialButton.SetActive(false); // Kapatma butonunu gizle
        Time.timeScale = 1f; // Tutorial kapatıldığında zamanı tekrar akıt
        attackJoystick.ChangeAbleToAttack(); // Saldırı joystickini tekrar aktif yap
        movementJoystick.ChangeAbleToMove(); // Hareket joystickini tekrar aktif yap
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
        StartCoroutine(MainMenuRoutine());
    }

    private IEnumerator MainMenuRoutine()
    {
        Time.timeScale = 1f;
        yield return StartCoroutine(SceneFader.Instance.ManualFadeOut());
        SceneManager.LoadScene(0);
    }


    // 3. Oyun Sonu (Ölünce çağrılır)
    public void ShowGameOver()
    {
        SoundManager.Instance.PlaySound(gameOverSound);
        SoundManager.Instance.PauseMusic();
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);
        pauseButton.SetActive(false);
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