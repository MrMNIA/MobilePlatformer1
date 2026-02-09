using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject pausePanel;    // Gri filtreli panel
    public GameObject gameOverPanel;
    public GameObject winPanel;

    [Header("HUD Elements")]
    public GameObject pauseButton;   // HUD üzerindeki duraklatma butonu

    // 1. Oyunu Duraklatma (Pause Butonuna basınca çalışır)
    public static UIManager instance; // Diğer scriptlerin ulaşacağı kapı

    void Awake()
    {
        // Singleton tanımlaması
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