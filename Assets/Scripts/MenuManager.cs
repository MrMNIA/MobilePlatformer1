using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public GameObject difficultyPanel;

    private int selectedLevelName;

    void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
        difficultyPanel.SetActive(false);
    }

    public void OpenLevelSelect()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    public void OpenDifficultyPopup(int levelIndex)
    {
        selectedLevelName = levelIndex;
        difficultyPanel.SetActive(true);
    }

    public void CloseDifficultyPopup()
    {
        difficultyPanel.SetActive(false);
    }

    public void SelectDifficultyAndStart(int difficultyIndex)
    {
        // Burada DifficultyManager'a dışarıdan ulaşıyoruz, içinde tanımlı değil!
        DifficultyManager.Instance.SetDifficulty(difficultyIndex);
        SceneManager.LoadScene(selectedLevelName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}