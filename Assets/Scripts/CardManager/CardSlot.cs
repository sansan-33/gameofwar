using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardSlot : MonoBehaviour, IDropHandler
{
    public GameObject item;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject cardNow;
        Debug.Log("Drop");

        if (!item)
        {
            item = DragCard.objBeingDraged;
            item.transform.SetParent(transform);
            item.transform.position = transform.position;
            cardNow = item;
        }
        /*else
        {
            Debug.Log(item);
            item = DragCard.objBeingDraged;
            item.transform.SetParent(transform);
            item.transform.position = transform.position;
        }*/
    }

    private void Update()
    {
        if (item != null && item.transform.parent != transform)
        {
            Debug.Log("Remover");
            item = null;
        }

    }
}
