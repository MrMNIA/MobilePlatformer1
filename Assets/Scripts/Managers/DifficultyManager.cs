using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;

    public enum Difficulty { Easy, Medium, Hard }
    public enum CurrentPowerup { Speed, Attack, Shield, None}
    public Difficulty currentDifficulty;
    public CurrentPowerup selectedPowerup;
    private void Awake()
    {
        // Singleton Yapısı: Sahneler arası geçişte bu objeyi korur
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDifficulty(); // Kayıtlı zorluğu açılışta yükle
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- MENÜDEN ÇAĞRILACAK OLAN METOD ---
    public void SetDifficulty(int difficultyIndex)
    {
        // Gelen 0, 1, 2 sayılarını Enum tipine çeviriyoruz
        currentDifficulty = (Difficulty)difficultyIndex;

        // Seçimi kalıcı hafızaya kaydet
        PlayerPrefs.SetInt("SelectedDifficulty", difficultyIndex);
        PlayerPrefs.Save();

        Debug.Log("Zorluk Ayarlandı: " + currentDifficulty);
    }

    public void SetPowerupDifficulty(int powerupIndex)
    {
        selectedPowerup = (CurrentPowerup)powerupIndex;

        // Seçimi kalıcı hafızaya kaydet
        PlayerPrefs.SetInt("SelectedPowerup", powerupIndex);
        PlayerPrefs.Save();

        Debug.Log("Powerup Zorluğu Ayarlandı: " + selectedPowerup);
    }

    public void UsePowerup()
    {
        if (selectedPowerup == CurrentPowerup.None) return;

        selectedPowerup = CurrentPowerup.None;
        PlayerPrefs.SetInt("SelectedPowerup", 3);
        PlayerPrefs.Save();
        MoneyManager.Instance.SpendCoins(50);
    }

    private void LoadDifficulty()
    {
        // Kayıt yoksa varsayılan olarak Easy (0) yükle
        int saved = PlayerPrefs.GetInt("SelectedDifficulty", 0);
        currentDifficulty = (Difficulty)saved;
    }

    // Düşmanların ve Tuzakların kullanacağı çarpanlar
    public float GetStatsMultiplier() => currentDifficulty switch
    {
        Difficulty.Easy => 1.0f,
        Difficulty.Medium => 1.25f,
        Difficulty.Hard => 1.5f,
        _ => 1.0f
    };

    public float GetCoinMultiplier() => currentDifficulty switch
    {
        Difficulty.Easy => 1.0f,
        Difficulty.Medium => 1.25f,
        Difficulty.Hard => 1.5f,
        _ => 1.0f
    };
}