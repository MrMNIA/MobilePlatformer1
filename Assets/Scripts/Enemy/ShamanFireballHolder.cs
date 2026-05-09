using System.Collections.Generic;
using UnityEngine;

public class ShamanFireballHolder : MonoBehaviour
{
    public static ShamanFireballHolder Instance;

    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private int poolSize = 10;
    private List<GameObject> fireballPool = new List<GameObject>();

    private void Awake()
    {
        Instance = this;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(fireballPrefab, this.transform);
            obj.SetActive(false);
            fireballPool.Add(obj);
        }
    }

    public GameObject GetFireball()
    {
        for (int i = 0; i < fireballPool.Count; i++)
        {
            if (!fireballPool[i].activeInHierarchy)
            {
                return fireballPool[i];
            }
        }

        GameObject newObj = Instantiate(fireballPrefab, this.transform);
        newObj.SetActive(false);
        fireballPool.Add(newObj);
        return newObj;
    }
}