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

    public CardFace(Card_Suits suit, Card_Numbers numbers)
    {
        this.suit = suit;
        this.numbers = numbers;
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
    public int i = 1;
    public int a = 2;
    int b = -2;
    int z = 0;
    private int MAX_CARD_NUMBER = 13;
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

    void ShuffleDeck()
    {
        cardDeckUsed.Clear();
        // for (int i = 0; i < 6; i++) { //Shuffle in 6 decks
        foreach (Card_Suits suit in Enum.GetValues(typeof(Card_Suits)))
        {
            foreach (Card_Numbers number in Enum.GetValues(typeof(Card_Numbers)))
            {
                cardDeck.Add(new CardFace(suit, number));
            }
        }
        // }

    }

    void DealCard(Player player, int j, bool left = true)
    {
        Debug.Log("Dealing Card to " + player.playerName);
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
                Debug.Log(card);
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
        // if ((left && !player.leftBust) || (!left && !player.rightBust))
        //{ //Check for bust

            lastCard = Instantiate(cardPrefab).GetComponent<Card>();
            //  Debug.Log($"1 DealingCard -- > last card | {lastCard} |?? ");
            lastCard.transform.localScale = Vector3.zero;

            //if (cardDeck.Count < 10)
            //  {
            //    ShuffleDeck();
            // }

            // CardFace randomCard = cardDeck[UnityEngine.Random.Range(0, cardDeck.Count - 13)];
            CardFace randomCard = cardDeck[1];

            cardDeckUsed.Add(randomCard);

            lastCard.GetComponent<Card>().SetCard(randomCard, GetCardFaceCoord(randomCard));

            // cardDeck.Remove(randomCard);

            cardSpawned = false;

            cardDispenserAnimator.SetBool("Dealing", true);
            //Play dealing animation first

            cardDispenserAnimator.SetTrigger("Deal");
            //Card slides out to a point in front of the dispenser
            //  while (!cardSpawned)

            yield return null;
            // }

            cardDispenserAnimator.SetBool("Dealing", false);

            //Player takes card
            // Debug.Log($"2 DealingCard -- > Try Player add card | {lastCard} | ");

            player.AddCard(lastCard, left);
            // ===================================== 
            // Set Button
            numbers++;
            if (numbers >= 3)
            {
                numbers = 3;
            }
            lastCard.setbuttonposition(numbers);
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

        for (int i = 0; i < numberOfCards; i++)
        {
            if (players.Count > 0)
            {

                DealCard(player, j, left);
            }

            currentWait = waitTime;
            while (currentWait > 0)
            {
                currentWait -= Time.deltaTime;
                yield return null;
            }
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
        StartCoroutine(DealCards(3, 0f, 0.5f, 1, players[0]));
        //  StartCoroutine(DealCards(2, 1f, 0.5f, dealer));

        players[0].Transfer(-2, 0);

        //testHit.gameObject.SetActive(false);


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


        //testSplit.gameObject.SetActive(false);

        StartCoroutine(DealCards(1, 0f, 0.5f, 1, players[0]));
    }
    public void Hitmerge()
    {

        //testSplit.gameObject.SetActive(false);

        StartCoroutine(DealCards(1, 0f, 0.5f, 12345, players[0]));
    }

}
