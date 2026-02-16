using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    [Header("HUD Elements")]
    public GameObject pauseButton;
    public Text currentLevelCoinText;
    public Text totalCoinText;

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

        UpdateTotalCoinUI();
        UpdateCurrentLevelCoinUI(0);
    }

    public void UpdateCurrentLevelCoinUI(int amount)
    {
        if (currentLevelCoinText != null)
            currentLevelCoinText.text = "Coins: " + amount.ToString();
    }

    public void UpdateTotalCoinUI()
    {
        if (totalCoinText != null)
        {
            int total = PlayerPrefs.GetInt("TotalCoins", 0);
            totalCoinText.text = "Bank: " + total.ToString();
        }
    }
    public void PauseGame()
    {
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        pauseButton.SetActive(false); // Oyun duruyorken pause butonu gizlensin
    }

    // 2. Devam Etme (Pause menüsündeki Resume butonuna basınca çalışır)
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        pauseButton.SetActive(true); // Pause butonu geri gelsin
    }

    public void RestartGame()
    {
        // ÇOK ÖNEMLİ: Zamanı tekrar akıtmalısın! 
        // Yoksa sahne yüklendiğinde oyun 0 hızında (donuk) başlar.
        Time.timeScale = 1f;

        // Mevcut aktif sahnenin adını veya index'ini alıp tekrar yüklüyoruz
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Eğer bir sonraki sahne build settings'de varsa yükle
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Time.timeScale = 1f; // Zamanı normalleştir
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // Eğer sonraki seviye yoksa ana menüye dön
            GotoMainMenu();
        }
    }

    public void GotoMainMenu()
    {
        Time.timeScale = 1f; // Ana menüye giderken zamanı normalleştir
        SceneManager.LoadScene(0); // Ana menü sahnesinin adını kullanarak yükle
    }


    // 3. Oyun Sonu (Ölünce çağrılır)
    public void ShowGameOver()
    {
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);
        pauseButton.SetActive(false);
    }

    // 4. Kazanma (Win Zone'a girince çağrılır)
    public void ShowWinScreen()
    {
        Time.timeScale = 0f;
        winPanel.SetActive(true);
        pauseButton.SetActive(false);
    }
}