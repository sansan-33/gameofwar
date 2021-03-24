using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardSlot : MonoBehaviour, IDropHandler
{
    public GameObject item;

    public void OnDrop(PointerEventData eventData)
    {
        /*
        GameObject cardNow;
        if (!item)
        {
            Debug.Log("Drop succes");
            item = DragCard.objBeingDraged;
            item.transform.SetParent(transform);

            //item.GetComponent<RectTransform>().anchoredPosition = new Vector2(-80,100);
            cardNow = item;
        }
        else
        {
            item.GetComponent<RectTransform>().anchoredPosition = new Vector2(-80, 100);
        }
        */

    }

    private void Update()
    {
        /*
        if (item != null && item.transform.parent != transform)
        {
            item = null;
        }
        */
    }

    internal object GetComponenInchild<T>()
    {
        throw new NotImplementedException();
    }
}