using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Card : MonoBehaviour
{


    bool flipped = false;
    Animator animator;
    Player owner;

    public CardFace cardFace;
    public CardFaceCoords coord;
    public int cardPlayerHandIndex = 0;

    [SerializeField] Renderer cardRenderer;
    [SerializeField] RTSPlayer RTSPlayer;
    Material cardFrontMat;

    public int a;
    public float cardTimer = 0;
    [SerializeField] public List<Sprite> sprite = new List<Sprite>();
    private GameObject unitPreviewInstance;
    private Renderer unitRendererInstance;
    private Camera mainCamera;
    private UnitFactory localFactory;
    int playerID = 0;
    int enemyID = 0;
    
    [SerializeField] public TMP_Text cardStar;
    [SerializeField] public Button cardSpawnButton;
    [SerializeField] public Image charIcon;

    void Awake()
    {
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();
        cardFrontMat = cardRenderer.materials[0];
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        playerID = player.GetPlayerID();
        enemyID = player.GetEnemyID();
    }
    

    
        public void Update()
    {
        cardTiming();
   
        
        if (unitPreviewInstance == null) { return; }

    }
   
    public void onBeginDrag(PointerEventData eventData)
    {

        Debug.Log($"Begin Drag");
        unitRendererInstance = unitPreviewInstance.GetComponentInChildren<Renderer>();
        //testing

    }
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"End Drag {eventData.position}");

        if (unitPreviewInstance == null) { return; }

        Ray ray = mainCamera.ScreenPointToRay(eventData.position);

    }
    public void SetCard(CardFace _cardFace, CardFaceCoords coord)
    {
        cardFace = _cardFace;
        a = (int)cardFace.numbers;
        if (a == 13)
        {
            a = 12;
        }
        // gameobject = GetComponent<Button>();
        //gameobject.sprite = sprite[a];
        if (coord != null)
        {
            cardFrontMat.SetTextureOffset("_BaseMap", coord.coord);
            cardRenderer.materials[0] = cardFrontMat;
        }
    }


    public void OnPointerDown()
    {
        GameObject DealManagers = GameObject.FindGameObjectWithTag("DealManager");
        if (DealManagers.GetComponent<CardDealer>().eleixer < 1)
        {
            return;
        }
        DealManagers.GetComponent<CardDealer>().eleixer -= ((int)this.cardFace.star+1);
        //Debug.Log($"Card ==> OnPointerDown {cardFace.numbers} / star {cardFace.star} / index {this.cardPlayerHandIndex} ");

        Destroy(gameObject);
        int type = (int)cardFace.numbers % System.Enum.GetNames(typeof(Unit.UnitType)).Length;
        Debug.Log($"Card ==> OnPointerDown {cardFace.numbers} / star {cardFace.star} / Unit Type {type} / PlayerHand index {this.cardPlayerHandIndex} playerID {playerID} localFactory is null ? {localFactory == null} ");
        if (localFactory == null) {
            foreach (GameObject factroy in GameObject.FindGameObjectsWithTag("UnitFactory"))
            {
                if (factroy.GetComponent<UnitFactory>().hasAuthority)
                {
                    localFactory = factroy.GetComponent<UnitFactory>();
                }
            }
        }

        localFactory.CmdSpawnUnit((Unit.UnitType) type , (int)this.cardFace.star + 1, playerID, true );
        FindObjectOfType<TacticalBehavior>().TryReinforce(playerID, enemyID);
        

        this.GetComponentInParent<Player>().moveCard(this.cardPlayerHandIndex);
        DealManagers.GetComponent<CardDealer>().Hit();
       
    }

    public void FadeOut()
    { if (this == null) { return; }
        StartCoroutine(FadeOutCard());
    }

    IEnumerator FadeOutCard()
    {
        Color col = cardFrontMat.GetColor("_BaseColor");
        Color colTarget = Color.grey;
        float change = 0;
        while (change < 1)
        {
            change += Time.deltaTime * 10;
            cardFrontMat.SetColor("_BaseColor", Color.Lerp(col, colTarget, change));
            yield return null;
        }
    }
    public void destroy()
    {if (gameObject != null)
        {

            Destroy(gameObject);
        }
            
        
    }

        void OnDisable()
        {
            if (owner != null)
                owner.OnPlayerHandReveal -= Reveal;
        }

        void Reveal()
        {
            if (!flipped) Flip(0);
        }

        public void SetOwner(Player player)
        {
            owner = player;
            owner.OnPlayerHandReveal += Reveal;
        }

        public void Flip(int cardIndex)
        {
            if (cardIndex == 1 && owner.dealer)
            {
                //First dealer card - don't flip yet
            }
            else
            {
                animator.SetTrigger("Flip");
                flipped = true;
            }

    }
    
       
        
    
    public void cardTiming()
    {
        
        if (cardTimer == 0) { return; }
        
        //float cardTimerPrivate = cardTimer;
        cardTimer -= Time.deltaTime;
        //Debug.Log($"cardTimer{cardTimerPrivate}");
       
        if (cardTimer <= 1)
        {
            int i = 0;
            if (i >= 1) { return; }   
         
            OnPointerDown();
            i++;
        }
    }
   
}

    public enum Card_Suits
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades
    }
    public enum Card_Stars
    {
        Bronze,
        Silver,
        Gold
    }
    public enum Card_Numbers
    {
    ARCHER, KNIGHT, MAGE, CAVALRY, SPEARMAN, HERO, MINISKELETON, GIANT
}
