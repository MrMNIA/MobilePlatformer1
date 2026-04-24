using UnityEngine;
using UnityEngine.EventSystems;

public class HUDDraggable : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        // Butonu mouse/parmak ile hareket ettir
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Hareket bitince Manager'daki listeyi güncelle
        var myData = SettingsManager.Instance.currentSettings.elements.Find(x => x.elementID == GetComponent<HUDElementController>().elementID);

        if (myData != null)
        {
            RectTransform rect = GetComponent<RectTransform>();
            myData.posX = rect.anchoredPosition.x;
            myData.posY = rect.anchoredPosition.y;
        }
    }
}