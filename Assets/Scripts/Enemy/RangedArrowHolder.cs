using System.Collections.Generic;
using UnityEngine;

public class RangedArrowHolder : MonoBehaviour
{
    public static RangedArrowHolder Instance; // Singleton: Her yerden erişim için

    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private int poolSize = 10;
    private List<GameObject> arrowPool = new List<GameObject>();

    private void Awake()
    {
        Instance = this;

        // Başlangıçta havuzu doldur
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(arrowPrefab, this.transform);
            obj.SetActive(false);
            arrowPool.Add(obj);
        }
    }

    public GameObject GetArrow()
    {
        // Pasif olan ilk oku bul ve gönder
        for (int i = 0; i < arrowPool.Count; i++)
        {
            if (!arrowPool[i].activeInHierarchy)
            {
                return arrowPool[i];
            }
        }

        // Eğer havuz yetmezse yeni bir tane oluştur (Opsiyonel: Havuzu genişletme)
        GameObject newObj = Instantiate(arrowPrefab, this.transform);
        newObj.SetActive(false);
        arrowPool.Add(newObj);
        return newObj;
    }
}