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
    
    public Vector2 startPos;
    public string direction;
    public bool directionChosen;
    [SerializeField] private GameObject DragPoint;
    [SerializeField] private LayerMask layerMask = new LayerMask();
    private bool m_Started = true;
    private int dragRange = 80;
    private float lastXPos = 0;

    private void Start()
    {
        itemDraggerParent = GameObject.FindGameObjectWithTag("CardDraggerParent").transform;
    }

    #region DragFunctions

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");
        startPos = this.transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        direction = Input.mousePosition.x > lastXPos ? "right" : "left";
        MoveCard();
        ShiftCard();
        lastXPos = Input.mousePosition.x;
    }
    private void MoveCard()
    {
        this.transform.position = new Vector3(Input.mousePosition.x, startPos.y, 0f);
    }
    private void ShiftCard()
    {
        int dragCardPlayerHandIndex = this.GetComponent<Card>().cardPlayerHandIndex;
        Collider[] hitColliders = Physics.OverlapBox(DragPoint.transform.position, transform.localScale * dragRange, Quaternion.identity, layerMask);
        int i = 0;
        Collider other;
        //Check when there is a new collider coming into contact with the box
        while (i < hitColliders.Length)
        {
            other = hitColliders[i++];
            if (other.TryGetComponent<Card>(out Card hittedCard))
            {
                if (dragCardPlayerHandIndex == hittedCard.cardPlayerHandIndex) { continue; }
                Debug.Log($"Shift Card  {dragCardPlayerHandIndex } to  {hittedCard.cardPlayerHandIndex } / direction {direction} ");
                CardParent.GetComponentInParent<Player>().moveCardAt(dragCardPlayerHandIndex , direction );
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
        Debug.Log("OnEndDrag");
        CardParent.GetComponentInParent<Player>().dragCardMerge();
        if (transform.parent == itemDraggerParent)
        {
           Debug.Log("drop failer");
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
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Handle finger movements based on touch phase.
            switch (touch.phase)
            {
                // Record initial touch position.
                case TouchPhase.Began:
                    startPos = touch.position;
                    directionChosen = false;
                    break;

                // Determine direction by comparing the current touch position with the initial one.
                case TouchPhase.Moved:
                    direction = touch.position.x > lastXPos ? "right" : "left";  //touch.position - startPos;
                    lastXPos = touch.position.x;
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

    }
}
