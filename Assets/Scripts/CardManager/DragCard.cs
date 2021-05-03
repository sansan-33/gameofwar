using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DragCard : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] Card CardParent;
    [SerializeField] private LayerMask floorMask = new LayerMask();
    public static Vector2 startPos;
    public string direction;
    public bool directionChosen;
    [SerializeField] private GameObject DragPoint;
    [SerializeField] private LayerMask layerMask = new LayerMask();
    //private MeshRenderer forbiddenArea;
    private bool m_Started = true;
    private int dragRange = 60;
    private float lastXPos = 0;
    private float deltaPos = 2f; // At least move 1 pixels
    private float spawnLinePos = 120f; // At least move 1 pixels, difference between card position.y and mouse position.y
    private Dictionary<int, string> hittedDict = new Dictionary<int, string>();
    public GameObject unitPreviewInstance;
    private UnitFactory localFactory;
    private CardDealer dealManagers;
    private RTSPlayer RTSplayer;
    Camera mainCamera;
    private PlayerGround playerGround;
    [SerializeField] GameObject unitPrefab;
    public GameObject EmptyCard;
 
    private bool IS_HITTED_TIMER = false;
    UnitMeta.Race playerRace;
    private void Start()
    {
        RTSplayer = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        playerGround = GameObject.FindGameObjectWithTag("FightGround").GetComponent<PlayerGround>();
       
        mainCamera = Camera.main;
        dealManagers = GameObject.FindGameObjectWithTag("DealManager").GetComponent<CardDealer>();
        Input.simulateMouseWithTouches = false;
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
        //Debug.Log(Input.mousePosition);
        TryDragCard(Input.mousePosition.x, Input.mousePosition.y);
    }
    private void TryDragCard(float mouseOrTouchPosX, float mouseOrTouchPosY)
    {
        if (Mathf.Abs(mouseOrTouchPosX - lastXPos) < deltaPos) { return; } // At least move deltaPos pixels

        direction = mouseOrTouchPosX > lastXPos ? "right" : "left";
        //Debug.Log($"Drag Card mouseOrTouchPosY {mouseOrTouchPosY} startPos.y {startPos.y}");
        // Prevent drag card to the bottom.
        mouseOrTouchPosY = mouseOrTouchPosY < startPos.y ? startPos.y : mouseOrTouchPosY;
        mouseOrTouchPosX = mouseOrTouchPosY > startPos.y + deltaPos ? startPos.x : mouseOrTouchPosX;

        // Freeze the card position if unit preview instance spawned
        if (unitPreviewInstance != null)
            transform.position = startPos;
        else
            transform.position = new Vector3(MouseSpeed(mouseOrTouchPosX , lastXPos) , mouseOrTouchPosY, 0f );

        if (spawnLinePos <= mouseOrTouchPosY - startPos.y){
            MoveUnitInstance();
        }
        else{
            StartCoroutine(ShiftCard());
        }
        lastXPos = mouseOrTouchPosX;
    }
    private IEnumerator ShiftCard()
    {
        if (IS_HITTED_TIMER) { yield break; }
        int dragCardPlayerHandIndex = this.GetComponent<Card>().cardPlayerHandIndex;
        Collider[] hitColliders = Physics.OverlapBox(DragPoint.transform.position, transform.localScale * dragRange, Quaternion.identity, layerMask);
        int i = 0;
        Collider other;
        bool isMove = false;

        //Check when there is a new collider coming into contact with the box
        while (i < hitColliders.Length)
        {
            other = hitColliders[i++];
            
            if (unitPreviewInstance != null) { Destroy(unitPreviewInstance); }

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
                    hittedDict.Clear();
                    //Debug.Log($"Shift Card  {dragCardPlayerHandIndex } to  {hittedCard.cardPlayerHandIndex } / direction {direction} ");
                    CardParent.GetComponentInParent<Player>().moveCardAt(dragCardPlayerHandIndex, direction);
                    //Prevent moving 2 cards in  one hitted, need to wait 0.5 sec for next move
                    IS_HITTED_TIMER = true;
                    yield return new WaitForSeconds(0.5f);
                    IS_HITTED_TIMER = false;
                    break;
                }
            }

        }
    }
    private void MoveUnitInstance()
    {
        //forbiddenArea = GetComponentInParent<Player>().forbiddenArea;
        playerGround.sortLayer(RTSplayer.GetPlayerID());
        //forbiddenArea.transform.localScale = GetComponentInParent<Player>().forbiddenAreaScale;
        //forbiddenArea.GetComponent<MeshRenderer>().enabled = true;
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
        EmptyCard.GetComponentInChildren<Image>().color = Color.black;

        // Create unit preview
        int type = (int)GetComponent<Card>().cardFace.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length;
        if (!UnitMeta.UnitSize.TryGetValue((UnitMeta.UnitType)type, out int unitsize)) { unitsize = 1; }
        playerRace = StaticClass.playerRace;
        //Debug.Log($"MoveUnitInstance race {playerRace} type {type}");
        GameObject UnitPrefab = localFactory.GetUnitPrefab(playerRace, (UnitMeta.UnitType)type);
        if (unitPreviewInstance == null)
        {
            unitPreviewInstance = Instantiate(unitPrefab);
            Transform unitBody = Instantiate(UnitPrefab.transform.Find("Body"));
            unitBody.SetParent(unitPreviewInstance.transform);
            //unitPreviewInstance.transform.SetParent(transform);
            unitPreviewInstance.transform.position = new Vector3();
            unitBody.position = new Vector3();
        }
        // unitPreviewInstance.transform.position = mousePos;
        //Debug.Log(unitPreviewInstance.transform.position);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        //forbiddenArea.GetComponent<MeshRenderer>().enabled = false;
        if (unitPreviewInstance != null){
           
            Destroy(unitPreviewInstance);
           
            Ray ray = mainCamera.ScreenPointToRay(eventData.position);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
            {
                playerGround.resetLayer();
                int type = (int)GetComponent<Card>().cardFace.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length;
                int uniteleixer = 1; ;
                if (UnitMeta.UnitEleixer.TryGetValue((UnitMeta.UnitType)type, out int value)) { uniteleixer = value; }
                if (GetComponent<Card>().eleixer < uniteleixer)
                {
                    //Debug.Log("hit");
                    //Destroy(unitPreviewInstance);
                    EmptyCard.GetComponentInChildren<Image>().color = Color.white;
                    transform.position = startPos;
                    return;
                }
                
                GetComponent<Card>().eleixer -= uniteleixer;
                GetComponent<Card>().DropUnit(unitPreviewInstance.transform.position);
                // Special Checking for Wall Button Card not under Card Slot (player)
                Player playerDeck = this.GetComponentInParent<Player>();
                if (playerDeck !=null)
                    playerDeck.moveCard(GetComponent<Card>().cardPlayerHandIndex);
                dealManagers.GetComponent<CardDealer>().Hit();
                
            }
            if(EmptyCard != null)
            {
                EmptyCard.GetComponentInChildren<Image>().color = Color.white;
            }
        } else {
            Vector3 pos = CardParent.GetComponentInParent<CardSlot>().transform.position;
            CardParent.GetComponentInParent<Player>().dragCardMerge();
            // Set the dragged card position right under the last hitted card slot again, did it in moveOneCard, need to set it again otheriwse it will stop in the middle.
            transform.position = pos;
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
            if (unitPreviewInstance == null) { return; }
            Vector3 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(pos);
            //if the floor layer is not floor it will not work!!!
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) { return; }
            unitPreviewInstance.transform.position = hit.point; 
        }
        catch (Exception) { }
    }
    public void SetPlayerRace(UnitMeta.Race race)
    {
        playerRace = race;
    }
    private float MouseSpeed(float mouseXPosition, float lastXPos)
    {
        //float adjustedXPosition = mouseXPosition + (Mathf.Abs(mouseXPosition - lastXPos) / speed); 
        return mouseXPosition;
    }
} 
