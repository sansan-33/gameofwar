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
    Player owner;

    public CardFace cardFace;
    public int cardPlayerHandIndex = 0;
    [SerializeField] public TMP_Text eleixerText;
    public float cardTimer = 0;
    [SerializeField] public List<Sprite> sprite = new List<Sprite>();
    private Camera mainCamera;
    private UnitFactory localFactory;
    private GameObject dealManagers;
    public int playerID = 0;
    //int enemyID = 0;
    Color teamColor;
    public eleixier eleixers;
    [SerializeField] public TMP_Text cardStar;
    [SerializeField] public Button cardSpawnButton;
    [SerializeField] public Image charIcon;

    public void Start()
    {
        eleixers = FindObjectOfType<eleixier>();
        if (NetworkClient.connection.identity == null) { return; }
        mainCamera = Camera.main;
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        playerID = player.GetPlayerID();
        //enemyID = player.GetEnemyID();
        teamColor = player.GetTeamColor();
        dealManagers = GameObject.FindGameObjectWithTag("DealManager");
    }
    public void Update()
    {
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
    }
    public void SetCard(CardFace _cardFace)
    {
        cardFace = _cardFace;
    }
    public void OnPointerDown()
    {
        if (GetComponent<DragCard>().unitPreviewInstance != null) { return; }
        int type = (int)cardFace.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length;
        int uniteleixer = 1;
        if (UnitMeta.UnitEleixer.TryGetValue((UnitMeta.UnitType)type, out int value)) { uniteleixer = value; }
        if (eleixers.eleixer < uniteleixer) { return; }
        eleixers.eleixer -= uniteleixer;
        if(type != (int) UnitMeta.UnitType.WALL)
        Destroy(gameObject);
        this.GetComponentInParent<Player>().moveCard(this.cardPlayerHandIndex);
        dealManagers.GetComponent<CardDealer>().Hit();
        Debug.Log($"Card ==> OnPointerDown {cardFace.numbers} / star {cardFace.star} / Unit Type {type} / PlayerHand index {this.cardPlayerHandIndex} playerID {playerID} localFactory is null ? {localFactory == null} ");
        localFactory.CmdSpawnUnit( (UnitMeta.Race) playerID, (UnitMeta.UnitType)type, (int)this.cardFace.star + 1, playerID, true, teamColor);
    }
    public void DropUnit(Vector3 SpwanPoint)
    {
        int type = (int)cardFace.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length;
        if (!UnitMeta.UnitSize.TryGetValue((UnitMeta.UnitType)type, out int unitsize)) { unitsize = 1; }
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
        localFactory.CmdDropUnit(0.1f, playerID, SpwanPoint, (UnitMeta.Race)playerID, (UnitMeta.UnitType)type, ((UnitMeta.UnitType) type).ToString(), unitsize,true, (int)this.cardFace.star + 1, teamColor, Quaternion.identity);
    }
    public void destroy()
    {
        if (gameObject != null){Destroy(gameObject);}
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
    ARCHER, TANK, MAGIC, CAVALRY, FOOTMAN, WALL
}
public enum Card_Deck
{
    ARCHER, TANK, MAGIC, CAVALRY, FOOTMAN
}


