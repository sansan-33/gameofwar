using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragCard : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public static GameObject objBeingDraged;
    [SerializeField] Card CardParent;
    private Vector3 startPosition;
    private Transform startParent;
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
         
        cardPlayerHandIndex = CardParent.GetComponent<Card>().cardPlayerHandIndex;
        transform.position = Input.mousePosition;
       
        if (transform.position.y!= startPosition.y)
        {
            float x = transform.position.x;
            float y = startPosition.y;
            float z = transform.position.z;
            transform.position = new Vector3(x, y, z);
        }
       
        if(transform.position.x > 990)
        {
            Vector3 maxlimit = new Vector3 (900, transform.position.y, transform.position.y);
            transform.position = maxlimit;
        }
        if (transform.position.x < 620)
        {
            Vector3 maxlimit = new Vector3(500, transform.position.y, transform.position.y);
            transform.position = maxlimit;
        }
        if (SecBeforePosition.x - transform.position.x > 0)
        {
            SecBeforePosition = transform.position;
            Debug.Log("moveing left");
            //move left

           
             SpaceBetweenTwoCard = cardBeforeTransform - transform.position.x;
            if (SpaceBetweenTwoCard >= 29&& SpaceBetweenTwoCard <= 31)
            {
                Debug.Log("Moving card left to right");
                cardBeforeTransform -= 60;
                CardParent.GetComponentInParent<Player>().moveCardAt(cardPlayerHandIndex,true);
                cardAfterTransform = transform.position.x + 30;
            }


        }

        else
        {
            SecBeforePosition = transform.position;
            Debug.Log("moveing right");
            //move right
            
                 SpaceBetweenTwoCard = transform.position.x - cardAfterTransform;
            Debug.Log(SpaceBetweenTwoCard);
                if (SpaceBetweenTwoCard >= -31&& SpaceBetweenTwoCard <= -29)
                {
                    Debug.Log("Moving card right to left");
                    cardAfterTransform = transform.position.x + 90;
                    CardParent.GetComponentInParent<Player>().moveCardAt(cardPlayerHandIndex, false);
                cardBeforeTransform = transform.position.x+30;
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
