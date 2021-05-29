using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleJSON;
using UnityEngine.Networking;
using System.Text;
using Mirror;
using System.Linq;

[System.Serializable]
public struct CardFace
{
    public Card_Suits suit;
    public Card_Numbers numbers;
    public Card_Stars star;
    public CardStats stats;

    public CardFace(Card_Suits suit, Card_Numbers numbers, Card_Stars star, CardStats stats)
    {
        this.suit = suit;
        this.numbers = numbers;
        this.star = star;
        this.stats = stats;
    }
}
public class CardDealer : MonoBehaviour
{
    public int MAXTOTALHAND = 6;
    [SerializeField] List<Player> players = new List<Player>();
    [SerializeField] List<CardFace> cardDeck = new List<CardFace>();
    [SerializeField] List<CardFace> cardDeckUsed = new List<CardFace>();
    [SerializeField] public Dictionary<string, CardStats> userCardStatsDict = new Dictionary<string, CardStats>();
    [SerializeField] public Dictionary<UnitMeta.UnitKey, Unit> playerUnitDict = new Dictionary<UnitMeta.UnitKey, Unit>();
    [SerializeField] Card buttonWall;
    [SerializeField] public TotalEleixier totalEleixers;
    [SerializeField] public Shader greyScaleShader;

    public static event Action UserCardLoaded;
    public SimpleObjectPool cardObjectPool;

    void Start()
    {
        TacticalBehavior.UnitTagUpdated += StartShuffleDeck;
        Player.CardRemoved += RemoveCard;
    }
    void OnDestroy()
    {
        TacticalBehavior.UnitTagUpdated -= StartShuffleDeck;
        Player.CardRemoved -= RemoveCard;
    }
    private void StartShuffleDeck()
    {
        StartCoroutine(ShuffleDeck(false));
        StartCoroutine(ShuffleDeck(true));
       // StartCoroutine(DealCards(3, 0f, 0.1f, players[1]));
    }
    IEnumerator ShuffleDeck(bool enemySpawn)
    {
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        if(enemySpawn == false)
        {
            yield return GetUserCard(player.GetUserID(), player.GetRace(), player.GetPlayerID(), player.GetTeamColor());
        }
        else
        {
            yield return new WaitForSeconds(1);
        }
        cardDeckUsed.Clear();
        string cardkey;
        SpawnEnemies spawnEnemies = FindObjectOfType<SpawnEnemies>();
        Dictionary<String, CardStats> cardstats = enemySpawn ? spawnEnemies.userCardStatsDict : userCardStatsDict;
        foreach (Card_Suits suit in Enum.GetValues(typeof(Card_Suits)))
        {
            foreach (Card_Deck number in Enum.GetValues(typeof(Card_Deck)))
            {
                cardkey = UnitMeta.UnitRaceTypeKey[StaticClass.playerRace][(UnitMeta.UnitType)Enum.Parse(typeof(UnitMeta.UnitType), number.ToString())].ToString();
                
                cardDeck.Add(new CardFace(suit, (Card_Numbers)number, Card_Stars.Bronze, cardstats[cardkey]));
            }
        }
        buttonWall.SetCard(new CardFace(Card_Suits.Clubs, Card_Numbers.WALL, Card_Stars.Bronze, cardstats[ UnitMeta.UnitRaceTypeKey[StaticClass.playerRace][UnitMeta.UnitType.WALL].ToString() ]));
        int index = enemySpawn ? 1 : 0;
        yield return DealCards(3, 0f, 0.1f, players[index]); 
    }

    void DealCard(Player player,  bool left = true)
    {
        //Debug.Log("Dealing Card to " + player.playerName);
        StartCoroutine(DealingCard(player, left));
    }

    IEnumerator DealingCard(Player player, bool left = true)
    {
        Debug.Log("DealingCard");
        Card lastCard = cardObjectPool.GetObject().GetComponent<Card>();
        CardFace randomCard = cardDeck[UnityEngine.Random.Range(0, cardDeck.Count)];
        //CardFace randomCard = cardDeck[3];
        cardDeckUsed.Add(randomCard);
        if (player.isEnemy == true)
        {
            Debug.Log("enemy card");
            lastCard.enemyCard = true;
        }
        lastCard.SetCard(randomCard);
        lastCard.cardStar.text = "1";
        lastCard.cardSpawnButton.GetComponentInChildren<Text>().text = randomCard.numbers.ToString();
        int cardnumber = (int)randomCard.numbers;
        cardnumber = cardnumber % lastCard.GetComponent<Card>().sprite.Count;
        int type = (int)randomCard.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length;
        int uniteleixer = 1;
        if (UnitMeta.UnitEleixer.TryGetValue((UnitMeta.UnitType)type, out int value)) { uniteleixer = value; }

        Material mat = new Material(greyScaleShader);
        lastCard.cardSpawnButton.GetComponentInChildren<Image>().sprite  = lastCard.GetComponent<Card>().sprite[cardnumber];
        lastCard.cardSpawnButton.GetComponentInChildren<Image>().material = mat;
        lastCard.eleixerText.text = uniteleixer.ToString();
        lastCard.SetUnitElexier(uniteleixer);
        for (int j = 0; j < 3; j++) {
            lastCard.stars.transform.GetChild(j).Find("Active").gameObject.SetActive(false);
            lastCard.cardFrame.transform.GetChild(j).gameObject.SetActive(false);
        }
        lastCard.cardFrame.transform.GetChild(0).gameObject.SetActive(true);
        lastCard.stars.transform.GetChild(0).Find("Active").gameObject.SetActive(true);
        lastCard.skillIcon.gameObject.SetActive(false);
        
        //Player takes card
        yield return player.AddCard(lastCard, left);  
    }
    IEnumerator DealCards(int numberOfCards, float delay, float waitTime, Player player, bool left = true, bool reveal = false)
    {
        float currentWait = waitTime;

        while (delay > 0)
        {
            delay -= Time.deltaTime;
            yield return null;
        }

        int handTotal = player.GetHandTotal();
        //Debug.Log($"DealCards ==> handTotal: {handTotal} MAXTOTALHAND :{MAXTOTALHAND}  Players count {players.Count} ");
        while (handTotal < MAXTOTALHAND)
        {
            if (players.Count > 0)
            {
                yield return DealingCard(player, left);
            }
            currentWait = waitTime;
            while (currentWait > 0)
            {
                currentWait -= Time.deltaTime;
                yield return null;
            }
            handTotal = player.GetHandTotal();
        }

    }
    public void Hit(bool enenmyHit)
    {
        float Timer = 1;
        while (Timer > 0) { Timer -= Time.deltaTime; }
        int index = enenmyHit ? 1 : 0;
        StartCoroutine(DealCards(1, 0f, 0.5f,  players[index]));
    }
    public void RemoveCard(Card _card)
    {
        cardObjectPool.ReturnObject(_card.gameObject);
    }
    // sends an API request - returns a JSON file
    IEnumerator GetUserCard(string userid, string race, int playerid, Color teamColor)
    {
        //Debug.Log($"Card Dealer => Get User Card {userid}  / {playerid}");
        yield return new WaitForSeconds(0.1f);
        GameObject[] units = GameObject.FindGameObjectsWithTag("Player" + playerid);
        GameObject king = GameObject.FindGameObjectWithTag("King" + playerid);
        List<GameObject> armies = new List<GameObject>();
        //Debug.Log($"Card Dealer => Get User Card armies size {armies.Count} ");

        armies = units.ToList();
        if (king != null)
            armies.Add(king);
        foreach (GameObject child in armies)
        {
            playerUnitDict.Add(child.GetComponent<Unit>().unitKey, child.GetComponent<Unit>());
        }
        //Debug.Log($"Card Dealer => playerUnitDict {playerUnitDict.Count}");

        userCardStatsDict.Clear();
        JSONNode jsonResult;
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();
        webReq.url = string.Format("{0}/{1}/{2}", APIConfig.urladdress, APIConfig.cardService, userid, race);
        yield return webReq.SendWebRequest();

        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);
        jsonResult = JSON.Parse(rawJson);

        //Debug.Log($"GetUserCard ==> User Card Count {jsonResult.Count} ");
        Unit unit;
        CardStats cardStats;
        for (int i = 0; i < jsonResult.Count; i++)
        {
            if (jsonResult[i]["cardkey"] != null && jsonResult[i]["cardkey"].ToString().Length > 0)
            {
                //Debug.Log($"GetUserCard ==> {i} {jsonResult[i]["cardkey"]} ");
                userCardStatsDict.Add(jsonResult[i]["cardkey"], new CardStats(jsonResult[i]["star"], jsonResult[i]["level"], jsonResult[i]["health"], jsonResult[i]["attack"], jsonResult[i]["repeatattackdelay"], jsonResult[i]["speed"], jsonResult[i]["defense"], jsonResult[i]["special"], jsonResult[i]["specialkey"], jsonResult[i]["passivekey"]));
                if(playerUnitDict.TryGetValue( (UnitMeta.UnitKey)Enum.Parse(typeof(UnitMeta.UnitKey), jsonResult[i]["cardkey"])  , out unit)){
                    //Debug.Log($"GetUserCard ==> Unit {unit.unitKey} ");
                    if (unit.unitType == UnitMeta.UnitType.HERO || unit.unitType == UnitMeta.UnitType.KING)
                    {
                        cardStats = userCardStatsDict[jsonResult[i]["cardkey"]];
                        unit.GetComponent<UnitPowerUp>().PowerUp(playerid, jsonResult[i]["cardkey"],unit.GetComponent<Unit>().GetSpawnPointIndex(), 1, cardStats.cardLevel, cardStats.health, cardStats.attack, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense, cardStats.special, cardStats.specialkey, cardStats.passivekey, teamColor);
                    }
                }
            }
        }
        UserCardLoaded?.Invoke();
        //Debug.Log($"GetUserCard ==> {webReq.url } {jsonResult}");
    }
   

}



