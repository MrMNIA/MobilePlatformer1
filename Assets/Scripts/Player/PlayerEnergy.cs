using UnityEngine;

public class PlayerEnergy : MonoBehaviour
{
    public float maxEnergy = 100f; // Maksimum enerji miktarı
    public float currentEnergy { get; private set; } // Şu anki enerji miktarı

    [SerializeField] private Rigidbody2D playerSpeed;
    [SerializeField] private float energyRegenRate = 5f; // Enerji
    
    private void Awake() {
        maxEnergy += PlayerPrefs.GetInt("EnergyLevel", 0) * 10; // Mağazadan alınan enerji geliştirmesi etkisi
        currentEnergy = maxEnergy; // Başlangıçta enerji tam dolu
    }
    
    public bool tryUseEnergy(float amount) {
        if (currentEnergy >= amount)
        {
            return true; // Enerji yeterli, işlemi gerçekleştirebilirsin
        }
        return false; // Enerji yeterli değil, işlemi gerçekleştiremezsin
    }
    public void UseEnergy(float amount)
    {
        currentEnergy = Mathf.Max(currentEnergy - amount, 0); // Enerjiyi kullan ve sıfırın altına düşmesini engelle
    }

    private void Update()
    {
        RegenerateEnergy(); // Her frame enerji yenile
    }
    void RegenerateEnergy()
    {
        if (currentEnergy < maxEnergy)
        {
            float regeneration = energyRegenRate;
            if (playerSpeed.linearVelocity.magnitude == 0) // Eğer karakter hareket etmiyorsa, yenilenme hızını artır
            {
                regeneration *= 2f; // Örneğin, dururken yenilenme hızını %100 artırabilirsiniz
            }
            currentEnergy += regeneration * Time.deltaTime; // Enerjiyi zamanla yenile
            currentEnergy = Mathf.Min(currentEnergy, maxEnergy); // Enerjinin maksimumu aşmamasını sağla
        }
    }
}
