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
    [SerializeField] List<CardPlayer> players = new List<CardPlayer>();
    [SerializeField] List<CardFace> cardDeck = new List<CardFace>();
    [SerializeField] List<CardFace> cardDeckUsed = new List<CardFace>();
    [SerializeField] public Dictionary<string, CardStats> userCardStatsDict = new Dictionary<string, CardStats>();
    [SerializeField] public Dictionary<UnitMeta.UnitKey, Unit> playerUnitDict = new Dictionary<UnitMeta.UnitKey, Unit>();

    // Buildings Buttons reference
    [SerializeField] Card buttonWall;
    [SerializeField] Card buttonBarrack;
    [SerializeField] Card buttonTower;
    [SerializeField] Card buttonBeacon;
    [SerializeField] Card buttonSpikeTrap;

    [SerializeField] Card EnemyButtonWall;
    [SerializeField] Card EnemyButtonBarrack;
    [SerializeField] Card EnemyButtonTower;
    [SerializeField] Card EnemyButtonBeacon;
    [SerializeField] Card EnemyButtonSpikeTrap;

    [SerializeField] public TotalEleixier totalEleixers;
    [SerializeField] public Shader greyScaleShader;
    [SerializeField] private bool spawnEnemyCard = false;
    [SerializeField] public CharacterArt Arts;

    public SimpleObjectPool cardObjectPool;
    public static event Action FinishDealEnemyCard;
    private RTSPlayer rtsPlayer;
    void Start()
    {
        TacticalBehavior.UnitTagUpdated += StartShuffleDeck;
        CardPlayer.CardRemoved += RemoveCard;
    }
    void OnDestroy()
    {
        TacticalBehavior.UnitTagUpdated -= StartShuffleDeck;
        CardPlayer.CardRemoved -= RemoveCard;
    }
    private void StartShuffleDeck()
    {
        StartCoroutine(ShuffleDeck(false));
        if (spawnEnemyCard == true && ((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)
        {
            StartCoroutine(ShuffleDeck(true));
        }
       // StartCoroutine(DealCards(3, 0f, 0.1f, players[1]));
    }
    IEnumerator ShuffleDeck(bool enemySpawn)
    {
        rtsPlayer = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        if(enemySpawn == false)
        {
            yield return GetUserCard(rtsPlayer.GetUserID(), rtsPlayer.GetRace(), rtsPlayer.GetPlayerID(), rtsPlayer.GetTeamColor());
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }
        cardDeckUsed.Clear();
        string cardkey;
        SpawnEnemies spawnEnemies = FindObjectOfType<SpawnEnemies>();
        Dictionary<String, CardStats> cardstats = enemySpawn ? spawnEnemies.userCardStatsDict : userCardStatsDict;
        foreach (Card_Suits suit in Enum.GetValues(typeof(Card_Suits)))
        {
            foreach (Card_Deck number in Enum.GetValues(typeof(Card_Deck)))
            {
                cardkey = UnitMeta.GetUnitKeyByRaceType(StaticClass.playerRace, (UnitMeta.UnitType)Enum.Parse(typeof(UnitMeta.UnitType), number.ToString()) ).ToString();
                cardDeck.Add(new CardFace(suit, (Card_Numbers)number, Card_Stars.Bronze, cardstats[cardkey]));
            }
        }
        SetBuildingsCard(spawnEnemies.userCardStatsDict);
        int index = enemySpawn ? 1 : 0;
        yield return DealCards(3, 0f, 0.1f, players[index], index);
        if(enemySpawn == true)
        {
            FinishDealEnemyCard?.Invoke();
        }
    }
    void SetBuildingsCard(Dictionary<String, CardStats> _cardstats)
    {
        buttonWall.SetCard(new CardFace(Card_Suits.Clubs, Card_Numbers.WALL, Card_Stars.Bronze, _cardstats[UnitMeta.GetUnitKeyByRaceType(StaticClass.playerRace, UnitMeta.UnitType.WALL).ToString()]));
        //Debug.Log($"Set Building Card {UnitMeta.UnitRaceTypeKey[StaticClass.playerRace][UnitMeta.UnitType.BARRACK].ToString()} ");
        buttonBarrack.SetCard(new CardFace(Card_Suits.Clubs, Card_Numbers.BARRACK, Card_Stars.Bronze, _cardstats[UnitMeta.GetUnitKeyByRaceType(StaticClass.playerRace,UnitMeta.UnitType.BARRACK).ToString() ]));
        buttonTower.SetCard(new CardFace(Card_Suits.Clubs, Card_Numbers.TOWER, Card_Stars.Bronze, _cardstats[UnitMeta.GetUnitKeyByRaceType(StaticClass.playerRace,UnitMeta.UnitType.TOWER).ToString()]));
        buttonBeacon.SetCard(new CardFace(Card_Suits.Clubs, Card_Numbers.BEACON, Card_Stars.Bronze, _cardstats[UnitMeta.GetUnitKeyByRaceType(StaticClass.playerRace,UnitMeta.UnitType.BEACON).ToString()]));
        buttonSpikeTrap.SetCard(new CardFace(Card_Suits.Clubs, Card_Numbers.SPIKETRAP, Card_Stars.Bronze, _cardstats[UnitMeta.GetUnitKeyByRaceType(StaticClass.playerRace,UnitMeta.UnitType.TRAP).ToString()]));
    
        EnemyButtonWall.SetCard(new CardFace(Card_Suits.Clubs, Card_Numbers.WALL, Card_Stars.Bronze, _cardstats[UnitMeta.GetUnitKeyByRaceType(StaticClass.playerRace,UnitMeta.UnitType.WALL).ToString()]));
        EnemyButtonBarrack.SetCard(new CardFace(Card_Suits.Clubs, Card_Numbers.BARRACK, Card_Stars.Bronze, _cardstats[UnitMeta.GetUnitKeyByRaceType(StaticClass.playerRace,UnitMeta.UnitType.BARRACK).ToString()]));
        EnemyButtonTower.SetCard(new CardFace(Card_Suits.Clubs, Card_Numbers.TOWER, Card_Stars.Bronze, _cardstats[UnitMeta.GetUnitKeyByRaceType(StaticClass.playerRace,UnitMeta.UnitType.TOWER).ToString()]));
        EnemyButtonBeacon.SetCard(new CardFace(Card_Suits.Clubs, Card_Numbers.BEACON, Card_Stars.Bronze, _cardstats[UnitMeta.GetUnitKeyByRaceType(StaticClass.playerRace,UnitMeta.UnitType.BEACON).ToString()]));
        EnemyButtonSpikeTrap.SetCard(new CardFace(Card_Suits.Clubs, Card_Numbers.SPIKETRAP, Card_Stars.Bronze, _cardstats[UnitMeta.GetUnitKeyByRaceType(StaticClass.playerRace,UnitMeta.UnitType.TRAP).ToString()]));
    
    }
    void DealCard(CardPlayer player, int playersIndex,  bool left = true)
    {
        //Debug.Log("Dealing Card to " + player.playerName);
        StartCoroutine(DealingCard(player, playersIndex, left));
    }

    IEnumerator DealingCard(CardPlayer player, int playersIndex, bool left = true)
    {
        //Debug.Log("DealingCard");
        Card lastCard = cardObjectPool.GetObject().GetComponent<Card>();
        CardFace randomCard;
        if (player.isEnemy != true)
       {
            randomCard = cardDeck[0];
        }
        else
        {
            //randomCard = cardDeck[3];
            randomCard = cardDeck[UnityEngine.Random.Range(0, cardDeck.Count)];
        }
        
        
        cardDeckUsed.Add(randomCard);
        //Debug.Log(lastCard.transform.Find("CFX4 Sparks Explosion B"));
        lastCard.cardStar.text = "1";
        lastCard.cardSpawnButton.GetComponentInChildren<Text>().text = randomCard.numbers.ToString();
        int cardnumber = (int)randomCard.numbers;
        cardnumber = cardnumber % lastCard.GetComponent<Card>().sprite.Count;
        int type = (int)randomCard.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length;
        int uniteleixer = 1;
        if (UnitMeta.UnitEleixer.TryGetValue((UnitMeta.UnitType)type, out int value)) { uniteleixer = value; }
        UnitMeta.Race race = playersIndex == 0 ? StaticClass.playerRace : StaticClass.enemyRace;
        Material mat = new Material(greyScaleShader);
        //Debug.Log($"DealingCard race {race} type {type} playersIndex {playersIndex} , StaticClass.enemyRace {StaticClass.enemyRace}");
        lastCard.cardSpawnButton.GetComponentInChildren<Image>().sprite = Arts.CharacterArtDictionary[UnitMeta.GetUnitKeyByRaceType(race,(UnitMeta.UnitType)type).ToString()].image;
        //lastCard.cardSpawnButton.GetComponentInChildren<Image>().sprite  = lastCard.GetComponent<Card>().sprite[cardnumber];
        lastCard.cardSpawnButton.GetComponentInChildren<Image>().material = mat;
        lastCard.eleixerText.text = uniteleixer.ToString();
        lastCard.SetUnitElexier(uniteleixer);
        for (int j = 0; j < 3; j++) {
            lastCard.stars.transform.GetChild(j).Find("Active1").gameObject.SetActive(false);
            lastCard.stars.transform.GetChild(j).Find("Active2").gameObject.SetActive(false);
            lastCard.stars.transform.GetChild(j).Find("Active3").gameObject.SetActive(false);
            lastCard.cardFrame.transform.GetChild(j).gameObject.SetActive(false);
        }
        lastCard.cardFrame.transform.GetChild(0).gameObject.SetActive(true);
        lastCard.stars.transform.GetChild(0).Find("Active1").gameObject.SetActive(true);
        lastCard.skillIcon.gameObject.SetActive(false);
        //Debug.Log($"{player.name} is enemy = {player.isEnemy}");
        lastCard.enemyCard = player.isEnemy;
        //Debug.Log($"{lastCard.enemyCard}");
        //Debug.Log("Set card before");
        lastCard.SetCard(randomCard);
       //Debug.Log($"Set card after {lastCard.cardFace.numbers}");
        //Player takes card
        //Debug.Log($"{player.name} is enemy = {player.isEnemy} card enemy card --> {lastCard.enemyCard}");
        yield return player.AddCard(lastCard, left);  
    }
    IEnumerator DealCards(int numberOfCards, float delay, float waitTime, CardPlayer player, int playersIndex, bool left = true, bool reveal = false)
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
                yield return DealingCard(player, playersIndex, left);
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
        StartCoroutine(HandleHit(enenmyHit));
    }
    private IEnumerator HandleHit(bool enenmyHit)
    {
        yield return new WaitForSeconds(1);
        int index = enenmyHit ? 1 : 0;
        StartCoroutine(DealCards(1, 0f, 0.5f, players[index],index));
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
        //Unit unit;
        //CardStats cardStats;
        for (int i = 0; i < jsonResult.Count; i++)
        {
            if (jsonResult[i]["cardkey"] != null && jsonResult[i]["cardkey"].ToString().Length > 0)
            {
                //Debug.Log($"GetUserCard ==> {i} {jsonResult[i]["cardkey"]} ");
                userCardStatsDict.Add(jsonResult[i]["cardkey"], new CardStats(jsonResult[i]["star"], jsonResult[i]["level"], jsonResult[i]["health"], jsonResult[i]["attack"], jsonResult[i]["repeatattackdelay"], jsonResult[i]["speed"], jsonResult[i]["defense"], jsonResult[i]["special"], jsonResult[i]["specialkey"], jsonResult[i]["passivekey"]));
                /*
                if(playerUnitDict.TryGetValue( (UnitMeta.UnitKey)Enum.Parse(typeof(UnitMeta.UnitKey), jsonResult[i]["cardkey"])  , out unit)){
                    if (unit.unitType == UnitMeta.UnitType.HERO || unit.unitType == UnitMeta.UnitType.KING)
                    {
                        cardStats = userCardStatsDict[jsonResult[i]["cardkey"]];
                        //Debug.Log($"GetUserCard ==> Unit {unit.unitKey} / {unit.unitType} / {cardStats}");
                        unit.GetComponent<UnitPowerUp>().PowerUp(playerid, jsonResult[i]["cardkey"],unit.GetComponent<Unit>().GetSpawnPointIndex(), 1, cardStats.cardLevel, cardStats.health, cardStats.attack, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense, cardStats.special, cardStats.specialkey, cardStats.passivekey, teamColor);
                        //Debug.Log($"After UnitPowerUp GetUserCard ==> Unit {unit.unitKey} / {unit.unitType} / {unit.name}");
                    }
                }
                */
            }
        }
        //UserCardLoaded?.Invoke();
        //Debug.Log($"GetUserCard ==> {webReq.url } {jsonResult}");
    }
   

}



