using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[System.Serializable]
public struct CardFace
{
    public Card_Suits suit;
    public Card_Numbers numbers;
    public Card_Stars star;

    public CardFace(Card_Suits suit, Card_Numbers numbers, Card_Stars star)
    {
        this.suit = suit;
        this.numbers = numbers;
        this.star = star;
    }
}

[System.Serializable]
public class CardFaceCoords
{
    public CardFace cardFace;
    [SerializeField] private Vector2 _coord;
    public Vector2 coord
    {
        get { return new Vector2(_coord.y / 9, -(_coord.x / 6)); }
        set { _coord = value; }
    }
}

public class CardDealer : MonoBehaviour
{
    
    public List<Card> cards;
    public List<Button> buttons;
    public int MAXTOTALHAND = 6;
    [SerializeField] Transform cardDispenserSpawn;
    [SerializeField] Animator cardDispenserAnimator;
    [SerializeField] GameObject cardPrefab;

    [SerializeField] List<CardFaceCoords> cardFaceCoords = new List<CardFaceCoords>();
    [SerializeField] GameObject costText;
    [SerializeField] List<Player> players = new List<Player>();

    [SerializeField] List<CardFace> cardDeck = new List<CardFace>();
    [SerializeField] List<CardFace> cardDeckUsed = new List<CardFace>();

    bool cardSpawned = false;
    Card lastCard;
    [Header("Testing")]

    [SerializeField] Button testHit;

    void Awake()
    {
        //  dealer.SetAsDealer();
        ShuffleDeck();
        DealBegin();
    }
    
    void ShuffleDeck()
    {
        cardDeckUsed.Clear();
        foreach (Card_Suits suit in Enum.GetValues(typeof(Card_Suits)))
        {
            foreach (Card_Deck number in Enum.GetValues(typeof(Card_Deck)))
            {
                cardDeck.Add(new CardFace(suit, (Card_Numbers)number, Card_Stars.Bronze));
            }
        }
    }

    void DealCard(Player player,  bool left = true)
    {
        //Debug.Log("Dealing Card to " + player.playerName);
        StartCoroutine(DealingCard(player, left));
    }

    public void SpawnCard()
    {
        //Triggered by animation event to ensure correct timing
        StartCoroutine(EndOfFrame());
    }

    IEnumerator EndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        lastCard.transform.position = new Vector3(cardDispenserSpawn.position.x, cardDispenserSpawn.position.y, 0);
        lastCard.transform.rotation = cardDispenserSpawn.rotation;
        lastCard.transform.localScale = Vector3.one;

        cardSpawned = true;
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
        cardSpawned = false;
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

    /*
    ================
        TESTING
    ================
    */

    [ContextMenu("Play Blackjack")]
    public void DealBegin()
    {
        //Deal two cards to player
        StartCoroutine(DealCards(3, 0f, 0.1f,players[0]));
    }

    [ContextMenu("Hit")]
    public void Hit()
    {
        float Timer = 1;
        while (Timer > 0) { Timer -= Time.deltaTime; }
     
        StartCoroutine(DealCards(1, 0f, 0.5f,  players[0]));
    }
    public void Hitmerge()
    {
        StartCoroutine(DealCards(1, 0f, 0.5f,  players[0]));
    }

}
