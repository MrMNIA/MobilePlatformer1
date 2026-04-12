using UnityEngine;
using System.Collections.Generic; // List kullanmak için gerekli
using UnityEngine.UI; // UI elemanlarına erişmek için gerekli

public class EnemyDoor : MonoBehaviour
{
    public List<Health> enemiesToWatch = new List<Health>();
    
    private Text counter; // Counter Text bileşenine erişim

    private void Start() {
        counter = GetComponentInChildren<Text>();
        UpdateCounter(); // Başlangıçta counter'ı güncelle
    }
    private void OnEnable()
    {
        // Static event olduğu için bir kez abone olmak yeterli
        Health.imDead += HandleEnemyDeath;
        UpdateCounter();
    }

    private void OnDisable()
    {
        // Hafıza sızıntısını önlemek için abonelikten çık
        Health.imDead -= HandleEnemyDeath;
    }

    // Bu metod Health.cs içinden tetiklenecek
    private void HandleEnemyDeath(Health deadEnemy)
    {
        // Eğer ölen düşman benim listemdeyse onu çıkar
        if (enemiesToWatch.Contains(deadEnemy))
        {
            enemiesToWatch.Remove(deadEnemy);
            Debug.Log("Düşman öldü, kalan: " + enemiesToWatch.Count);
            UpdateCounter();

            // Liste boşaldıysa kapıyı aç
            if (enemiesToWatch.Count <= 0)
            {
                OpenDoor();
            }
        }
    }

    private void UpdateCounter()
    {
        if (counter != null)
        {
            counter.text = enemiesToWatch.Count.ToString();
        }
    }

    private void OpenDoor()
    {
        Debug.Log("Kapı açılıyor!");
        gameObject.SetActive(false); // Veya kapı animasyonunu oynat
    }
}