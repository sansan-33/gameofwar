using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragCard : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public static GameObject objBeingDraged;
    [SerializeField] Card CardParent;
    public Vector3 startPosition;
    public Transform startParent;
    private CanvasGroup canvasGroup;
    private Transform itemDraggerParent;
    public int cardPlayerHandIndex = 0;
    float SpaceBetweenTwoCard;
    Vector3 nowPosition;
    Vector3 SecBeforePosition;
    float cardAfterTransform;
    float cardBeforeTransform;
    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        itemDraggerParent = GameObject.FindGameObjectWithTag("CardDraggerParent").transform;
    }

    #region DragFunctions

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");
        objBeingDraged = gameObject;
        cardBeforeTransform = transform.position.x;
        cardAfterTransform = transform.position.x + 60;
        startPosition = transform.position;
        startParent = transform.parent;
        transform.SetParent(itemDraggerParent);
        SecBeforePosition = startPosition;
        canvasGroup.blocksRaycasts = false;

    }

    public void OnDrag(PointerEventData eventData)
    {


        // Get the dragged  card index 
        cardPlayerHandIndex = CardParent.GetComponent<Card>().cardPlayerHandIndex;
        // Dragged Card position map to mouse position
        transform.position = Input.mousePosition;
        //Block Moving Up and Down, Allow Horizontal Move only.
        if (transform.position.y!= startPosition.y)
        {
            float x = transform.position.x;
            float y = startPosition.y;
            float z = transform.position.z;
            transform.position = new Vector3(x, y, z);
        }

        ShiftNeighbourCard();

    }
    public void ShiftNeighbourCard()
    {
        //Detect Card Direction
        if (SecBeforePosition.x - transform.position.x > 0)   //move left
        {
            SecBeforePosition = transform.position;
            SpaceBetweenTwoCard = cardBeforeTransform - transform.position.x;
            if (SpaceBetweenTwoCard >= 29)
            {
                //Moving card left to right
                cardBeforeTransform -= 90; //update card before position 
                CardParent.GetComponentInParent<Player>().moveCardAt(cardPlayerHandIndex, "left");
                cardAfterTransform = transform.position.x + 30;
            }
        }
        else //move right
        {
            SecBeforePosition = transform.position;
            SpaceBetweenTwoCard = cardAfterTransform - transform.position.x;
            if (SpaceBetweenTwoCard <= 31)
            {
                //Moving card right to left
                cardAfterTransform = transform.position.x + 120;
                CardParent.GetComponentInParent<Player>().moveCardAt(cardPlayerHandIndex, "right");
                cardBeforeTransform = transform.position.x + 60;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        objBeingDraged = null;
        CardParent.GetComponentInParent<Player>().dragCardMerge();
        canvasGroup.blocksRaycasts = true;
        if (transform.parent == itemDraggerParent)
        {
            Debug.Log("drop failer");
           transform.position = startPosition;
            transform.SetParent(startParent);
        }
    }

    #endregion

    private void Update()
    {


    }
}
