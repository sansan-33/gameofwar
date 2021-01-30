using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


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
    public int maxEleixer = 10;
    private float eleixerTimer = 1f;
    public int eleixer = 10;
    public List<Card> cards;
    public List<Button> buttons;
    public int i = 1;
    public int a = 2;
    int b = -2;
    int z = 0;
    private int MAX_CARD_NUMBER = 13;
    public int MAXTOTALHAND = 6;
    [SerializeField] Transform cardDispenserSpawn;
    [SerializeField] Animator cardDispenserAnimator;
    [SerializeField] GameObject cardPrefab;

    [SerializeField] List<CardFaceCoords> cardFaceCoords = new List<CardFaceCoords>();

    [SerializeField] List<Player> players = new List<Player>();

    [SerializeField] List<CardFace> cardDeck = new List<CardFace>();
    [SerializeField] List<CardFace> cardDeckUsed = new List<CardFace>();

    bool cardSpawned = false;
    Card lastCard;
    int numbers = 0;
    [Header("Testing")]

    [SerializeField] Button testHit;

    void Awake()
    {
        //  dealer.SetAsDealer();
        ShuffleDeck();
        DealBegin();
    }
    private void Update()
    {
        eleixerTimer -= Time.deltaTime;
        if (eleixerTimer <= 0 )
        {
            eleixerTimer = 1f;
            if (eleixer < maxEleixer)
            {
                eleixer += 1;

            }
        }
        
    }
    void ShuffleDeck()
    {
        cardDeckUsed.Clear();
        // for (int i = 0; i < 6; i++) { //Shuffle in 6 decks
        foreach (Card_Suits suit in Enum.GetValues(typeof(Card_Suits)))
        {
            foreach (Card_Numbers number in Enum.GetValues(typeof(Card_Numbers)))
            {
                cardDeck.Add(new CardFace(suit, number, Card_Stars.Bronze));
            }
        }
        // }
    }

    void DealCard(Player player, int j, bool left = true)
    {
        //Debug.Log("Dealing Card to " + player.playerName);
        StartCoroutine(DealingCard(player, j, left));
    }

    public void SpawnCard()
    {
        //Triggered by animation event to ensure correct timing
        StartCoroutine(EndOfFrame());
    }

    CardFaceCoords GetCardFaceCoord(CardFace cardFace)
    {
        foreach (CardFaceCoords card in cardFaceCoords)
        {
            if (card.cardFace.suit == cardFace.suit && card.cardFace.numbers == cardFace.numbers)
            {
                //Debug.Log(card);
                return card;
            }
        }
        return null;
    }

    IEnumerator EndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        lastCard.transform.position = new Vector3(cardDispenserSpawn.position.x, cardDispenserSpawn.position.y, 0);
        lastCard.transform.rotation = cardDispenserSpawn.rotation;
        lastCard.transform.localScale = Vector3.one;

        cardSpawned = true;
    }

    IEnumerator DealingCard(Player player, int j, bool left = true)
    {

        lastCard = Instantiate(cardPrefab).GetComponent<Card>();

        CardFace randomCard = cardDeck[UnityEngine.Random.Range(0, cardDeck.Count)];
        //CardFace randomCard = cardDeck[3];
        cardDeckUsed.Add(randomCard);

        lastCard.GetComponent<Card>().SetCard(randomCard, GetCardFaceCoord(randomCard));
        lastCard.cardSpawnButton.GetComponentInChildren<Text>().text = randomCard.suit.ToString();
        int cardnumber = (int)randomCard.numbers;
        cardnumber = cardnumber % lastCard.GetComponent<Card>().sprite.Count;

        lastCard.cardSpawnButton.GetComponentInChildren<Image>().sprite  = lastCard.GetComponent<Card>().sprite[cardnumber];
         Debug.Log($"Char Sprite Index {(int)randomCard.suit}");


        // cardDeck.Remove(randomCard);

        cardSpawned = false;

        //cardDispenserAnimator.SetBool("Dealing", true);
        //Play dealing animation first

        //cardDispenserAnimator.SetTrigger("Deal");
        //Card slides out to a point in front of the dispenser
        //  while (!cardSpawned)
        yield return null;
        // }

        //cardDispenserAnimator.SetBool("Dealing", false);

        //Player takes card
        player.AddCard(lastCard, left);
        // ===================================== 
        // Set Button
        numbers++;
        if (numbers >= 3)
            {
                numbers = 3;
            }
           
            // ============================
        //}
    }

    IEnumerator DealCards(int numberOfCards, float delay, float waitTime, int j, Player player, bool left = true, bool reveal = false)
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
                DealCard(player, 1, left);
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
        StartCoroutine(DealCards(3, 0f, 0.1f, 1, players[0]));
        //  StartCoroutine(DealCards(2, 1f, 0.5f, dealer));

        players[0].Transfer(-2, 0);

    }

    [ContextMenu("Split Player Hand")]
    public void Split()
    {
        if (players.Count > 0)
        {
            players[0].SplitHand();
            StartCoroutine(DealCards(7, 0f, 0.5f, 1, players[0], true));
            StartCoroutine(DealCards(7, 0.5f, 0.5f, 1, players[0], false));

        }
    }

    [ContextMenu("Hit")]
    public void Hit()
    {
        float Timer = 1;
        while (Timer > 0) { Timer -= Time.deltaTime; }

        StartCoroutine(DealCards(1, 0f, 0.5f, 1, players[0]));
    }
    public void Hitmerge()
    {

        StartCoroutine(DealCards(1, 0f, 0.5f, 12345, players[0]));
    }

}
