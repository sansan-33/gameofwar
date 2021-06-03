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
    [SerializeField] public Dictionary<UnitMeta.UnitSkill , Sprite> SkillImageDictionary = new Dictionary<UnitMeta.UnitSkill, Sprite>();

    private UnitFactory localFactory;
    private CardDealer dealManagers;
    private ParticlePool appearEffectPool;
    public int playerID = 0;
    public bool enemyCard = false;
    private int uniteleixer = 1;
    private int type;
    private float progressImageVelocity;
    Color teamColor;
    [SerializeField] public TMP_Text cardStar;
    [SerializeField] public Button cardSpawnButton;
    [SerializeField] public Image charIcon;
    [SerializeField] public Image skillIcon;
    [SerializeField] public GameObject stars;
    [SerializeField] public GameObject cardFrame;
    [SerializeField] private Image cardTimerImage;
    private float effectAmount = 1f;
    private float originalx;
    private float originaly;
    private float originalz;
    RTSPlayer player;
    public void Start()
    {
        if (NetworkClient.connection.identity == null) { return; }
        //Debug.Log("Start");
        RectTransform rect = GetComponent<RectTransform>();
        originalx = rect.localScale.x;
        originaly = rect.localScale.y;
        originalz = rect.localScale.z;
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        playerID = player.GetPlayerID();
        //playerRace =  (UnitMeta.Race)Enum.Parse(typeof(UnitMeta.Race), player.GetRace());
        teamColor = player.GetTeamColor();
        dealManagers = GameObject.FindGameObjectWithTag("DealManager").GetComponent<CardDealer>();
        appearEffectPool = GameObject.FindGameObjectWithTag("EffectPool").GetComponent<ParticlePool>();
        StartCoroutine(SetLocalFactory());
       
        //if (UnitMeta.UnitEleixer.TryGetValue((UnitMeta.UnitType)type, out int value)) { uniteleixer = value; }
    }
    IEnumerator HandleScale()
    {
       /*(Debug.Log("handle scale");
        if (GetComponentInParent<Player>())
        {
            Debug.Log($"HandleScale {enemyCard} player -->{GetComponentInParent<Player>().name}");
        }*/
        
        if(enemyCard == true)
        {
           Debug.Log($"Getting scale");
            RectTransform rect = GetComponent<RectTransform>();
            float x = rect.localScale.x;
            float y = rect.localScale.y;
            float z = rect.localScale.z;
            rect.localScale = new Vector3( (float)0.5,  (float)0.5,(float)0.5);
            GetComponentInChildren<Button>().enabled = false;
        }
        else
        {
            RectTransform rect = GetComponent<RectTransform>();
            rect.localScale = new Vector3(1, 1, 1);
            GetComponentInChildren<Button>().enabled = true;
        }
        yield return null;
        //enemyCard = false;
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
        //Debug.Log("calling set card");
        cardFace = new CardFace(_cardFace.suit, _cardFace.numbers, _cardFace.star, _cardFace.stats);
        StartCoroutine(HandleScale());
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
        //Debug.Log("OnpointerDown");
        if (GetComponent<DragCard>().unitPreviewInstance != null) { return; }
        if (localFactory == null) { StartCoroutine(SetLocalFactory()); }

        int type = (int)cardFace.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length;
        //Debug.Log(enemyCard);
        if(enemyCard == true)
        {
            if (dealManagers.totalEleixers.enemyEleixer < uniteleixer) { return; }
            dealManagers.totalEleixers.enemyEleixer -= uniteleixer;
            playerID = player.GetEnemyID();
            teamColor = player.GetTeamEnemyColor();
        }
        else
        {
            if (dealManagers.totalEleixers.eleixer < uniteleixer) { return; }
            dealManagers.totalEleixers.eleixer -= uniteleixer;
            playerID = player.GetPlayerID();
            teamColor = player.GetTeamColor();
        }

        //Debug.Log(GetComponent<RectTransform>().localScale);
        ResetScale();
        this.GetComponentInParent<Player>().moveCard(this.cardPlayerHandIndex);
        var _enemyCard = enemyCard;
        enemyCard = false;
        dealManagers.Hit(_enemyCard);
       
        //Debug.Log("re set enemy card");
        localFactory.CmdSpawnUnit( StaticClass.playerRace, (UnitMeta.UnitType)type, (int)cardFace.star + 1, playerID, cardFace.stats.cardLevel, cardFace.stats.health, cardFace.stats.attack, cardFace.stats.repeatAttackDelay, cardFace.stats.speed, cardFace.stats.defense, cardFace.stats.special, cardFace.stats.specialkey, cardFace.stats.passivekey, teamColor);
    }
    public void ResetScale()
    {
        Debug.Log("Reset Scale");
        GetComponent<RectTransform>().localScale = new Vector3(originalx, originaly, originalz);
    }
    public void DropUnit(Vector3 spawnPoint)
    {
        StartCoroutine(HandleDropUnit(spawnPoint));
    }
    public IEnumerator HandleDropUnit(Vector3 spawnPoint)
    {
        if (localFactory == null) { yield return SetLocalFactory(); }
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        enemyCard = false;
        int type = (int)cardFace.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length;
        if (!UnitMeta.UnitSize.TryGetValue((UnitMeta.UnitType)type, out int unitsize)) { unitsize = 1; }
        appearEffectPool.UseParticles(spawnPoint);
        if (enemyCard == false)
        {
            playerID = player.GetPlayerID();
            teamColor = player.GetTeamColor();
        }
       // if(cardFace.stats == null)
       // {
       //     localFactory.CmdDropUnit(playerID, spawnPoint, StaticClass.playerRace, (UnitMeta.UnitType)type, ((UnitMeta.UnitType)type).ToString(), unitsize, 1, 1, 1, 1, 1, 1, 1, null, cardFace.stats.passivekey, (int)cardFace.star + 1, teamColor, Quaternion.identity);

       // }
      //  else
       // {
            localFactory.CmdDropUnit(playerID, spawnPoint, StaticClass.playerRace, (UnitMeta.UnitType)type, ((UnitMeta.UnitType)type).ToString(), unitsize, cardFace.stats.cardLevel, cardFace.stats.health, cardFace.stats.attack, cardFace.stats.repeatAttackDelay, cardFace.stats.speed, cardFace.stats.defense, cardFace.stats.special, cardFace.stats.specialkey, cardFace.stats.passivekey, (int)cardFace.star + 1, teamColor, Quaternion.identity);
       // }
        //Debug.Log($"Card ==> DropUnit {cardFace.numbers} / star {cardFace.star} / Unit Type {type} / Race { StaticClass.playerRace} / playerID {playerID } / SpwanPoint {spawnPoint } / unitsize {unitsize } / Card Stats {cardFace.stats}");
        yield return null;
    }
    public void destroy()
    {
        if (gameObject != null){Destroy(gameObject);}
    }
    private void Update()
    {
        int elexier;
        if (cardTimerImage != null)
        {
            elexier = enemyCard ? dealManagers.totalEleixers.enemyEleixer : dealManagers.totalEleixers.eleixer;
            if (elexier < uniteleixer)
            {
                cardTimerImage.gameObject.SetActive(true);
                float fillAmount = (float)elexier / uniteleixer;
                //Debug.Log($"eleixers:{eleixer}uniteleixer:{uniteleixer}, eleixers/uniteleixer:{fillAmount}");
                cardTimerImage.fillAmount = Mathf.SmoothDamp(cardTimerImage.fillAmount, 1 - fillAmount, ref progressImageVelocity, 0.5f);
                effectAmount = 1f;
            } else {
                cardTimerImage.fillAmount = 1f;
                cardTimerImage.gameObject.SetActive(false);
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


