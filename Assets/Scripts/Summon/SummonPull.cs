using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SummonPull : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public static Vector2 startPos;
    private float OutOfScreenPosY = 300f;
    public SummonManager summonManager;
    private void Start()
    {
        //Debug.Log("SummonPull initialized");

    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("OnBeginDrag summon");
        startPos = this.transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log($"On drag {Input.mousePosition}");
        TryPull(Input.mousePosition.x, Input.mousePosition.y);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        transform.position = startPos;
        summonManager.HandleSummon();
    }
    private void TryPull(float mouseOrTouchPosX, float mouseOrTouchPosY)
    {
        if (Mathf.Abs(mouseOrTouchPosY - startPos.y) > OutOfScreenPosY) { return; } // Prevent out of screen
        mouseOrTouchPosY = mouseOrTouchPosY > startPos.y ? startPos.y : mouseOrTouchPosY;

        //Debug.Log($"TryPull {transform.position}");
        transform.position = new Vector3(startPos.x, mouseOrTouchPosY, 0f);
    }
}

