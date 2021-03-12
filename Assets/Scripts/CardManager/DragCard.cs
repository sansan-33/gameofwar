using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DragCard : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public GameObject MiddleLine;
    public static GameObject objBeingDraged;
    [SerializeField] Card CardParent;
    public Transform startParent;
    private Transform itemDraggerParent;
    [SerializeField] private LayerMask floorMask = new LayerMask();
    public static Vector2 startPos;
    public string direction;
    public bool directionChosen;
    [SerializeField] private GameObject DragPoint;
    [SerializeField] private LayerMask layerMask = new LayerMask();
    private GameObject whereCanNotPlaceUnitImage;
    private bool m_Started = true;
    private int dragRange = 50;
    private float lastXPos = 0;
    private float deltaPos = 2f; // At least move 1 pixels
    private float spawnLinePos = 25f; // At least move 1 pixels
    private Dictionary<int, string> hittedDict = new Dictionary<int, string>();
    public GameObject unitPreviewInstance;
    private UnitFactory localFactory;
    private CardDealer dealManagers;
    Camera mainCamera;
    [SerializeField] GameObject unitPrefab;
    public GameObject EmptyCard;
    int i = 0;
    private void Start()
    {
        whereCanNotPlaceUnitImage = GameObject.FindGameObjectWithTag("WhereCanNotPlaceUnitImage");
        MiddleLine = GameObject.FindGameObjectWithTag("MiddleLine");
        mainCamera = Camera.main;
        dealManagers = GameObject.FindGameObjectWithTag("DealManager").GetComponent<CardDealer>();
        //SpawnLine = GameObject.FindGameObjectWithTag("SpawnLine").transform;
        Input.simulateMouseWithTouches = false;
        objBeingDraged = gameObject;
        itemDraggerParent = GameObject.FindGameObjectWithTag("CardDraggerParent").transform;
        transform.SetParent(itemDraggerParent);
    }

    #region DragFunctions

    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("OnBeginDrag");
        if (unitPreviewInstance != null) Destroy(unitPreviewInstance);
        startPos = this.transform.position;
        lastXPos = Input.mousePosition.x;
    }

    public void OnDrag(PointerEventData eventData)
    {
        
            // Debug.Log(Input.mousePosition);
            TryDragCard(Input.mousePosition.x, Input.mousePosition.y);
    }
    private void TryDragCard(float mouseOrTouchPosX, float mouseOrTouchPosY)
    {
        if (Mathf.Abs(mouseOrTouchPosX - lastXPos) < deltaPos) { return; } // At least move deltaPos pixels

        direction = mouseOrTouchPosX > lastXPos ? "right" : "left";

        //Debug.Log($"On Drag Mouse Direction {mouseOrTouchPosX} lastPos {lastXPos}  , {direction} ");

        // Prevent drag card to the bottom.
        mouseOrTouchPosY = mouseOrTouchPosY < startPos.y ? startPos.y : mouseOrTouchPosY;
        mouseOrTouchPosX = mouseOrTouchPosY > startPos.y + deltaPos ? startPos.x : mouseOrTouchPosX;

        // Freeze the card position if unit preview instance spawned
        if (unitPreviewInstance != null)
            transform.position = startPos;
        else
            transform.position = new Vector3(mouseOrTouchPosX, mouseOrTouchPosY, 0f);

        ShiftCard(mouseOrTouchPosX, mouseOrTouchPosY);
        lastXPos = mouseOrTouchPosX;
    }
    private void ShiftCard(float mouseOrTouchPosX, float mouseOrTouchPosY)
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
            if (spawnLinePos <= mouseOrTouchPosY - startPos.y)
            {
                //Debug.Log($"call MoveUnitInstance mouseOrTouchPosX {mouseOrTouchPosX} mouseOrTouchPosY {mouseOrTouchPosY} ");
                MoveUnitInstance();
            }
            else
            {
                //Debug.Log($"unitPreviewInstance{unitPreviewInstance}");
                EmptyCard.GetComponentInChildren<Image>().color = Color.white;
                if (whereCanNotPlaceUnitImage == null) { whereCanNotPlaceUnitImage = GameObject.FindGameObjectWithTag("WhereCanNotPlaceUnitImage"); }
                whereCanNotPlaceUnitImage.SetActive(false);
                if (unitPreviewInstance != null) { Destroy(unitPreviewInstance); }
            }

            if (other.TryGetComponent<Card>(out Card hittedCard))
            {
                if (dragCardPlayerHandIndex == hittedCard.cardPlayerHandIndex) { continue; }

                // Check whehter move card , new hitted card or old hitted card but different direction can move 
                if (hittedDict.TryGetValue(hittedCard.cardPlayerHandIndex, out string hittedCardDirection)) // hiited card 
                {
                    if (hittedCardDirection != direction)
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
    private void MoveUnitInstance()
    {
        if (whereCanNotPlaceUnitImage == null) { whereCanNotPlaceUnitImage = GameObject.FindGameObjectWithTag("WhereCanNotPlaceUnitImage"); }
        whereCanNotPlaceUnitImage.SetActive(true);
        whereCanNotPlaceUnitImage.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, (Screen.height / 8) * 5);
        whereCanNotPlaceUnitImage.GetComponent<RectTransform>().localPosition = new Vector3(0, (Screen.height / 8)*2, 0);
        if (localFactory == null)
        {
            foreach (GameObject factroy in GameObject.FindGameObjectsWithTag("UnitFactory"))
            {
                if (factroy.GetComponent<UnitFactory>().hasAuthority)
                {
                    localFactory = factroy.GetComponent<UnitFactory>();
                }
            }
        }
        //Debug.Log($"MoveUnitInstance {mousePos}");
        EmptyCard.GetComponentInChildren<Image>().color = Color.black;

        // Create unit preview
        int type = (int)GetComponent<Card>().cardFace.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length;
        if (!UnitMeta.UnitSize.TryGetValue((UnitMeta.UnitType)type, out int unitsize)) { unitsize = 1; }
        GameObject UnitPrefab = localFactory.GetUnitPrefab((UnitMeta.Race)GetComponent<Card>().playerID, (UnitMeta.UnitType)type);
        if (unitPreviewInstance == null)
        {
            unitPreviewInstance = Instantiate(unitPrefab);
            int i = 0;
            while (UnitPrefab.transform.GetChild(i).name != "Body")
            {
                i++;
            }
            Transform unitBody = Instantiate(UnitPrefab.transform.GetChild(i));
            unitBody.SetParent(unitPreviewInstance.transform);
            //unitPreviewInstance.transform.SetParent(transform);
            unitPreviewInstance.transform.position = new Vector3();
            unitBody.position = new Vector3();
        }
        // unitPreviewInstance.transform.position = mousePos;
       
        //Debug.Log(unitPreviewInstance.transform.position);
    }

    /*
     * 
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

        if (unitPreviewInstance != null)
        {
            whereCanNotPlaceUnitImage.SetActive(false);
            Ray ray = mainCamera.ScreenPointToRay(eventData.position);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
            {
                int type = (int)GetComponent<Card>().cardFace.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length;
                int uniteleixer = 1; ;
                if (UnitMeta.UnitEleixer.TryGetValue((UnitMeta.UnitType)type, out int value)) { uniteleixer = value; }
                if (GetComponent<Card>().eleixers.eleixer < uniteleixer)
                {
                    Destroy(unitPreviewInstance);
                    EmptyCard.GetComponentInChildren<Image>().color = Color.white;
                    //transform.position = startPos;
                    //transform.SetParent(startParent);
                    return;
                }
                GetComponent<Card>().eleixers.eleixer -= uniteleixer;
                //Debug.Log("hit");
                GetComponent<Card>().DropUnit(unitPreviewInstance.transform.position);

                Destroy(unitPreviewInstance);
                this.GetComponentInParent<Player>().moveCard(GetComponent<Card>().cardPlayerHandIndex);
                dealManagers.GetComponent<CardDealer>().Hit();
            }
        } else { 
            //Debug.Log("destroy card");
            CardParent.GetComponentInParent<Player>().dragCardMerge();
            if (transform.parent == itemDraggerParent)
            {
                //Debug.Log("drop failer");
                transform.position = startPos;
                transform.SetParent(startParent);
            }
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
      
            try
            {
                Vector3 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();

                Ray ray = mainCamera.ScreenPointToRay(pos);

                if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) { return; }
           
            if (hit.point.z > -70&&i>0)
            {
                unitPreviewInstance.transform.position = new Vector3(hit.point.x, hit.point.y,-71);
                //Debug.Log(unitPreviewInstance.transform.position);
                return;
            }
            else { unitPreviewInstance.transform.position = hit.point; }
            //Debug.Log("suck");
            
            i++;
                //Debug.Log(hit.point);
           
            }
            catch (Exception) { }
        
        //whereCanNotPlaceUnitImage.SetActive(false);
    }
} 
