using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] public string itemcode = null;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Shop Button OnPointerClick itemcode {itemcode}");
        StaticClass.ItemCode = itemcode;
        
    }
}