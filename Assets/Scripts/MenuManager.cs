using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public GameObject difficultyPanel;
    public GameObject optionsPanel;
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
        optionsPanel.SetActive(false);

    }

    public void ShowOptions()
    {
        mainMenuPanel.SetActive(false);
        SoundManager.Instance.UpdateDisplay();
        optionsPanel.SetActive(true);
        SoundManager.Instance.PlaybuttonClickSound();
    }

    public void OpenLevelSelect()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
        SoundManager.Instance.PlaybuttonClickSound();
    }

    public void OpenDifficultyPopup(int levelIndex)
    {
        selectedLevelName = levelIndex;
        difficultyPanel.SetActive(true);
        SoundManager.Instance.PlaybuttonClickSound();

    }

    public void CloseDifficultyPopup()
    {
        difficultyPanel.SetActive(false);
        SoundManager.Instance.PlaybuttonClickSound();

    }

    public void SelectDifficultyAndStart(int difficultyIndex)
    {
        // Burada DifficultyManager'a dışarıdan ulaşıyoruz, içinde tanımlı değil!
        SoundManager.Instance.PlaybuttonClickSound();
        DifficultyManager.Instance.SetDifficulty(difficultyIndex);
        SceneManager.LoadScene(selectedLevelName);


    }

    public void QuitGame()
    {
        SoundManager.Instance.PlaybuttonClickSound();
        Application.Quit();
    }
}