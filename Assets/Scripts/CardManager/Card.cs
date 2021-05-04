using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public CardFace cardFace;
    public int cardPlayerHandIndex = 0;
    [SerializeField] public TMP_Text eleixerText;
    public float cardTimer = 0;
    [SerializeField] public List<Sprite> sprite = new List<Sprite>();
    private UnitFactory localFactory;
    private CardDealer dealManagers;
    private ParticlePool appearEffectPool;
    public int playerID = 0;
    private int uniteleixer = 1;
    private int type;
    private float progressImageVelocity;
    Color teamColor;
    [SerializeField] public TMP_Text cardStar;
    [SerializeField] public Button cardSpawnButton;
    [SerializeField] public Image charIcon;
    [SerializeField] private Image cardTimerImage;
    private float effectAmount = 1f;

    public void Start()
    {
        if (NetworkClient.connection.identity == null) { return; }
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        playerID = player.GetPlayerID();
        //playerRace =  (UnitMeta.Race)Enum.Parse(typeof(UnitMeta.Race), player.GetRace());
        teamColor = player.GetTeamColor();
        dealManagers = GameObject.FindGameObjectWithTag("DealManager").GetComponent<CardDealer>();
        appearEffectPool = GameObject.FindGameObjectWithTag("EffectPool").GetComponent<ParticlePool>();
        StartCoroutine(SetLocalFactory());
        
        //if (UnitMeta.UnitEleixer.TryGetValue((UnitMeta.UnitType)type, out int value)) { uniteleixer = value; }
    }
    IEnumerator SetLocalFactory()
    {
        yield return new WaitForSeconds(1f);
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
        cardFace = new CardFace(_cardFace.suit, _cardFace.numbers, _cardFace.star, _cardFace.stats);
    }
    public void SetUnitElexier(int elexier)
    {
        this.uniteleixer = elexier;
    }
    public int GetUnitElexier()
    {
        return this.uniteleixer;
    }
    public void OnPointerDown()
    {
        Debug.Log("OnpointerDown");
        if (GetComponent<DragCard>().unitPreviewInstance != null) { return; }
        if (localFactory == null) { StartCoroutine(SetLocalFactory()); }

        int type = (int)cardFace.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length;
        if (dealManagers.totalEleixers.eleixer < uniteleixer) { return; }

        dealManagers.totalEleixers.eleixer -= uniteleixer;
        this.GetComponentInParent<Player>().moveCard(this.cardPlayerHandIndex);
        dealManagers.Hit();
        localFactory.CmdSpawnUnit( StaticClass.playerRace, (UnitMeta.UnitType)type, (int)cardFace.star + 1, playerID, cardFace.stats.cardLevel, cardFace.stats.health, cardFace.stats.attack, cardFace.stats.repeatAttackDelay, cardFace.stats.speed, cardFace.stats.defense, cardFace.stats.special, cardFace.stats.specialkey, cardFace.stats.passivekey, teamColor);
    }
    public void DropUnit(Vector3 SpwanPoint)
    {
        if (localFactory == null) { StartCoroutine(SetLocalFactory()); }
        int type = (int)cardFace.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length;
        if (!UnitMeta.UnitSize.TryGetValue((UnitMeta.UnitType)type, out int unitsize)) { unitsize = 1; }
        appearEffectPool.UseParticles(SpwanPoint);
        //Debug.Log($"Card ==> DropUnit {cardFace.numbers} / star {cardFace.star} / Unit Type {type} / Race { StaticClass.playerRace} / playerID {playerID } / SpwanPoint {SpwanPoint } / unitsize {unitsize } / Card Stats {cardFace.stats}");
        localFactory.CmdDropUnit(playerID, SpwanPoint, StaticClass.playerRace, (UnitMeta.UnitType)type, ((UnitMeta.UnitType) type).ToString(), unitsize, cardFace.stats.cardLevel, cardFace.stats.health, cardFace.stats.attack, cardFace.stats.repeatAttackDelay, cardFace.stats.speed, cardFace.stats.defense, cardFace.stats.special, cardFace.stats.specialkey, cardFace.stats.passivekey, (int)this.cardFace.star + 1, teamColor, Quaternion.identity);
    }
    public void destroy()
    {
        if (gameObject != null){Destroy(gameObject);}
    }
    private void Update()
    {
        if (cardTimerImage != null)
        {
            if (uniteleixer >= dealManagers.totalEleixers.eleixer)
            {
                cardTimerImage.gameObject.SetActive(true);
                float fillAmount = (float)dealManagers.totalEleixers.eleixer / uniteleixer;
                //Debug.Log($"eleixers:{eleixer}uniteleixer:{uniteleixer}, eleixers/uniteleixer:{fillAmount}");
                cardTimerImage.fillAmount = Mathf.SmoothDamp(cardTimerImage.fillAmount, 1 - fillAmount, ref progressImageVelocity, 0.5f);
                effectAmount = 1f;
            } else {
                effectAmount = 0.1f;
            }
            cardSpawnButton.GetComponentInChildren<Image>().material.SetFloat("_Greyscale", effectAmount);
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
    ARCHER, TANK, MAGIC, CAVALRY, FOOTMAN, WALL
}
public enum Card_Deck
{
    ARCHER, TANK, MAGIC, CAVALRY, FOOTMAN
}


