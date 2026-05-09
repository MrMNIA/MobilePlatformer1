using UnityEngine;

public class RisingLava : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private float yukselmeHizi = 0.5f; // Lavın yükselme hızı
    [SerializeField] private bool isRising = false;

    // Eğer lavın belli bir noktada durmasını istersen bunu kullanabilirsin
    [SerializeField] private bool limitVarMi = false;
    [SerializeField] private float maksimumYukseklik = 20f;

    void Update()
    {
        // Eğer yükselme tetiklendiyse her karede objeyi yukarı taşı
        if (isRising)
        {
            YukselmeIslemi();
        }
    }

    private void YukselmeIslemi()
    {
        // Objenin pozisyonunu yukarı doğru güncelliyoruz
        transform.Translate(Vector3.up * yukselmeHizi * Time.deltaTime);

        // Limit kontrolü
        if (limitVarMi && transform.position.y >= maksimumYukseklik)
        {
            isRising = false;
            Debug.Log("Lav maksimum yüksekliğe ulaştı.");
        }
    }

    /// <summary>
    /// Bu metot dışarıdan çağrıldığında lav yükselmeye başlar.
    /// </summary>
    public void LavYukselt()
    {
        isRising = true;
        Debug.Log("Lav yükselme işlemi başlatıldı!");
    }
}