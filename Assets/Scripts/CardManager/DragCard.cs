using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragCard : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public static GameObject objBeingDraged;
    [SerializeField] Card CardParent;
    public Transform startParent;
    private Transform itemDraggerParent;
    
    public static Vector2 startPos;
    public string direction;
    public bool directionChosen;
    [SerializeField] private GameObject DragPoint;
    [SerializeField] private LayerMask layerMask = new LayerMask();
    private bool m_Started = true;
    private int dragRange = 50;
    private float lastXPos = 0;
    private float deltaPos = 2f; // At least move 1 pixels
    private Dictionary<int, string> hittedDict = new Dictionary<int, string>();

    private void Start()
    {
        Input.simulateMouseWithTouches = false;
        objBeingDraged = gameObject;
        itemDraggerParent = GameObject.FindGameObjectWithTag("CardDraggerParent").transform;
        transform.SetParent(itemDraggerParent);
    }

    #region DragFunctions

    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("OnBeginDrag");
        startPos = this.transform.position;
        lastXPos = Input.mousePosition.x;
    }

    public void OnDrag(PointerEventData eventData)
    {
        TryDragCard(Input.mousePosition.x);
    }
    private void TryDragCard(float mouseOrTouchPosX)
    {
        if (Mathf.Abs(mouseOrTouchPosX - lastXPos) < deltaPos) { return; } // At least move deltaPos pixels

        direction = mouseOrTouchPosX > lastXPos ? "right" : "left";
        //Debug.Log($"On Drag Mouse Direction {mouseOrTouchPosX} lastPos {lastXPos}  , {direction} ");

        this.transform.position = new Vector3(mouseOrTouchPosX, startPos.y, 0f);

        ShiftCard();
        lastXPos = mouseOrTouchPosX;
    }
    private void ShiftCard()
    {
        int dragCardPlayerHandIndex = this.GetComponent<Card>().cardPlayerHandIndex;
        Collider[] hitColliders = Physics.OverlapBox(DragPoint.transform.position, transform.localScale * dragRange, Quaternion.identity, layerMask);
        int i = 0;
        Collider other;
        bool isMove = false;
        //Check when there is a new collider coming into contact with the box
        while (i < hitColliders.Length)
        {
            other = hitColliders[i++];
            if (other.TryGetComponent<Card>(out Card hittedCard))
            {
                if (dragCardPlayerHandIndex == hittedCard.cardPlayerHandIndex) { continue; }

                // Check whehter move card , new hitted card or old hitted card but different direction can move 
                if (hittedDict.TryGetValue(hittedCard.cardPlayerHandIndex, out string hittedCardDirection ) ) // hiited card 
                {
                    if(hittedCardDirection != direction) 
                    {
                        isMove = true;
                        hittedDict[hittedCard.cardPlayerHandIndex] = direction;
                    }
                }
                else // new hitted card
                {
                    hittedDict.Add(hittedCard.cardPlayerHandIndex, direction);
                    isMove = true;
                }
                if (isMove)
                {
                    CardParent.GetComponentInParent<Player>().moveCardAt(dragCardPlayerHandIndex, direction);
                    //Debug.Log($"Shift Card  {dragCardPlayerHandIndex } to  {hittedCard.cardPlayerHandIndex } / direction {direction} ");
                    break;
                }
            }
        }
    }
    /*
    private void OnTriggerEnter(Collider other) //sphere collider is used to differentiate between the unit itself, and the attack range (fireRange)
    {
        if (other.TryGetComponent<Card>(out Card card))
        {
            if (this.cardPlayerHandIndex == card.cardPlayerHandIndex) { return; }
            Debug.Log($" OnTriggerEnter move card from {this.cardPlayerHandIndex} to  {card.cardPlayerHandIndex}");
            CardParent.GetComponentInParent<Player>().moveCardAt(card.cardPlayerHandIndex, direction.magnitude > 0 ? "left" : "right");
        }
    }
    */
    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("OnEndDrag");
        objBeingDraged = null;
        CardParent.GetComponentInParent<Player>().dragCardMerge();
        if (transform.parent == itemDraggerParent)
        {
           //Debug.Log("drop failer");
           transform.position = startPos;
           transform.SetParent(startParent);
        }
    }
    //Draw the Box Overlap as a gizmo to show where it currently is testing. Click the Gizmos button to see this
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
        if (m_Started)
        {
            //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
            Gizmos.DrawWireCube(DragPoint.transform.position, transform.localScale * dragRange);
        }
    }

    #endregion

    private void Update()
    {
        // Track a single touch as a direction control.
        /*
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Handle finger movements based on touch phase.
            switch (touch.phase)
            {
                // Record initial touch position.
                case TouchPhase.Began:
                    startPos = this.transform.position;
                    lastXPos = touch.position.x;
                    directionChosen = false;
                    break;

                // Determine direction by comparing the current touch position with the initial one.
                case TouchPhase.Moved:
                    TryDragCard(touch.position.x);
                    break;

                // Report that a direction has been chosen when the finger is lifted.
                case TouchPhase.Ended:
                    directionChosen = true;
                    break;
            }
        }
        if (directionChosen)
        {
            // Something that uses the chosen direction...
        }
       */

    }
}
