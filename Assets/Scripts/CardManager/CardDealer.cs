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
    [SerializeField] GameObject cardPrefab;
    [SerializeField] List<Player> players = new List<Player>();
    [SerializeField] List<CardFace> cardDeck = new List<CardFace>();
    [SerializeField] List<CardFace> cardDeckUsed = new List<CardFace>();
    [SerializeField] public Dictionary<string, CardStats> userCardStatsDict = new Dictionary<string, CardStats>();
    [SerializeField] Card buttonWall;

    Card lastCard;
    UnitMeta.Race UnitRace; 
    void Awake()
    {
        //  dealer.SetAsDealer();
        //ShuffleDeck();
        //DealBegin();
    }
    private void Start()
    {
        StartCoroutine(ShuffleDeck());
    }
    IEnumerator ShuffleDeck()
    {
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        UnitRace = StaticClass.playerRace;
        yield return GetUserCard(player.GetUserID(), player.GetRace());
        cardDeckUsed.Clear();
        string cardkey;
        foreach (Card_Suits suit in Enum.GetValues(typeof(Card_Suits)))
        {
            foreach (Card_Deck number in Enum.GetValues(typeof(Card_Deck)))
            {
                cardkey = UnitMeta.UnitRaceTypeKey[UnitRace][(UnitMeta.UnitType)Enum.Parse(typeof(UnitMeta.UnitType), number.ToString())].ToString();
                cardDeck.Add(new CardFace(suit, (Card_Numbers)number, Card_Stars.Bronze, userCardStatsDict[ cardkey ]));
            }
        }
        buttonWall.SetCard(new CardFace(Card_Suits.Clubs, Card_Numbers.WALL, Card_Stars.Bronze, userCardStatsDict[ UnitMeta.UnitRaceTypeKey[UnitRace][UnitMeta.UnitType.WALL].ToString() ]));
        
        yield return DealCards(3, 0f, 0.1f, players[0]); 
    }

    void DealCard(Player player,  bool left = true)
    {
        //Debug.Log("Dealing Card to " + player.playerName);
        StartCoroutine(DealingCard(player, left));
    }

    IEnumerator DealingCard(Player player, bool left = true)
    {
        lastCard = Instantiate(cardPrefab).GetComponent<Card>();

        CardFace randomCard = cardDeck[UnityEngine.Random.Range(0, cardDeck.Count)];
        //CardFace randomCard = cardDeck[3];
        cardDeckUsed.Add(randomCard);

        lastCard.GetComponent<Card>().SetCard(randomCard);
        lastCard.cardSpawnButton.GetComponentInChildren<Text>().text = randomCard.numbers.ToString();
        int cardnumber = (int)randomCard.numbers;
        cardnumber = cardnumber % lastCard.GetComponent<Card>().sprite.Count;
        int type = (int)randomCard.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length;
        int uniteleixer = 1;
        if (UnitMeta.UnitEleixer.TryGetValue((UnitMeta.UnitType)type, out int value)) { uniteleixer = value; }
        
        lastCard.cardSpawnButton.GetComponentInChildren<Image>().sprite  = lastCard.GetComponent<Card>().sprite[cardnumber];
        lastCard.eleixerText.text = uniteleixer.ToString();
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
    public void Hit()
    {
        float Timer = 1;
        while (Timer > 0) { Timer -= Time.deltaTime; }
        StartCoroutine(DealCards(1, 0f, 0.5f,  players[0]));
    }

    // sends an API request - returns a JSON file
    IEnumerator GetUserCard(string userid, string race)
    {
        userCardStatsDict.Clear();
        JSONNode jsonResult;
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();
        webReq.url = string.Format("{0}/{1}/{2}", APIConfig.urladdress, APIConfig.cardService, userid, race);
        yield return webReq.SendWebRequest();

        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);
        jsonResult = JSON.Parse(rawJson);
        for (int i = 0; i < jsonResult.Count; i++)
        {
            if (jsonResult[i]["cardkey"] != null && jsonResult[i]["cardkey"].ToString().Length > 0)
            {
                userCardStatsDict.Add(jsonResult[i]["cardkey"], new CardStats(jsonResult[i]["star"], jsonResult[i]["level"], jsonResult[i]["health"], jsonResult[i]["attack"], jsonResult[i]["repeatattackdelay"], jsonResult[i]["speed"], jsonResult[i]["defense"], jsonResult[i]["special"]));
                /*
                if (jsonResult[i]["cardkey"].ToString().ToLower().Contains("wall"))
                {
                    string cardkey = jsonResult[i]["cardkey"];
                    Debug.Log($"Wall {cardkey} stat {userCardStatsDict[cardkey] }");
                }
                */
            }
        }
        //Debug.Log($"GetUserCard ==> {webReq.url } {jsonResult}");
    }

}



