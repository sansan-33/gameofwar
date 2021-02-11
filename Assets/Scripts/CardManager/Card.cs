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
    Animator animator;
    Player owner;

    public CardFace cardFace;
    public int cardPlayerHandIndex = 0;

    [SerializeField] Renderer cardRenderer;
    [SerializeField] TMP_Text eleixerText;
    public float cardTimer = 0;
    [SerializeField] public List<Sprite> sprite = new List<Sprite>();
    private Camera mainCamera;
    private UnitFactory localFactory;
    private GameObject dealManagers;
    int playerID = 0;
    int enemyID = 0;
    Color teamColor;
    public eleixier eleixers;
    [SerializeField] public TMP_Text cardStar;
    [SerializeField] public Button cardSpawnButton;
    [SerializeField] public Image charIcon;

    void Awake()
    {
        eleixers = FindObjectOfType<eleixier>();
        if (NetworkClient.connection.identity == null) { return; }
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        playerID = player.GetPlayerID();
        enemyID = player.GetEnemyID();
        teamColor = player.GetTeamColor();
        dealManagers = GameObject.FindGameObjectWithTag("DealManager");
      
    }
    public void Update()
    {
    }
    public void SetCard(CardFace _cardFace)
    {
        cardFace = _cardFace;
    }
    public void OnPointerDown()
    {
        int type = (int)cardFace.numbers % System.Enum.GetNames(typeof(Unit.UnitType)).Length;
        int uniteleixer = 1;
        Debug.Log($"{eleixers.eleixer},{ uniteleixer}");
        if (Unit.UnitEleixer.TryGetValue((Unit.UnitType)type, out int value)) { uniteleixer = value; }
        
        if (eleixers.eleixer < uniteleixer)
        {
            return;
        }
        eleixers.eleixer -= uniteleixer;
        //Debug.Log($"Card ==> OnPointerDown {cardFace.numbers} / star {cardFace.star} / index {this.cardPlayerHandIndex} ");

        Destroy(gameObject);
        this.GetComponentInParent<Player>().moveCard(this.cardPlayerHandIndex);
        dealManagers.GetComponent<CardDealer>().Hit();
        
        //Debug.Log($"Card ==> OnPointerDown {cardFace.numbers} / star {cardFace.star} / Unit Type {type} / PlayerHand index {this.cardPlayerHandIndex} playerID {playerID} localFactory is null ? {localFactory == null} ");
        if (localFactory == null) {
            foreach (GameObject factroy in GameObject.FindGameObjectsWithTag("UnitFactory"))
            {
                if (factroy.GetComponent<UnitFactory>().hasAuthority)
                {
                    localFactory = factroy.GetComponent<UnitFactory>();
                }
            }
        }

        localFactory.CmdSpawnUnit((Unit.UnitType) type , (int)this.cardFace.star + 1, playerID, true, teamColor );
        FindObjectOfType<TacticalBehavior>().TryReinforce(playerID, enemyID);
       
    }
    public void destroy()
    {
        if (gameObject != null){Destroy(gameObject);}
    }
    void OnDisable()
    {
    }
    
    public void SetOwner(Player player)
    {
        owner = player;
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
