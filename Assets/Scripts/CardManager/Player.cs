using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class Player : MonoBehaviour
{

    public delegate void PlayerHandEvents();
    public PlayerHandEvents OnPlayerHandReveal;

    public string playerName = "Player";
    public int chips = 100;
    int i = 1;
    [Header("References")]
    [SerializeField] Transform cardParent;
    [SerializeField] Transform button;
    [Header("Layout References")]
    [SerializeField] Transform singleHandStart;
    [SerializeField] Transform splitHandStartLeft;
    [SerializeField] Transform splitHandStartRight;

    [Header("Settings")]
    [SerializeField] float cardOffset = 60f;
    [SerializeField] float cardMoveSpeed = 10;
    [SerializeField] int MAXCARDSTAR = 2;

    [Header("Debug")]
    [SerializeField] List<List<Card>> playerHand = new List<List<Card>>();
    bool handSplit = false;
    public bool leftBust = false;
    public bool rightBust = false;
    public int handTotal;
    float cardPosX = (float) 100;
    int l = 1;

    public bool dealer = false;
    public int[] pot = new int[2];
    [SerializeField] Text chipsText;
    [SerializeField] Text pot1Text;
    [SerializeField] Text pot2Text;

    void Awake()
    {
        Reset();
    }

    public void SetAsDealer()
    {
        dealer = true;
    }

    public bool CanSplit()
    {
        // Debug.Log($"Left hand count {playerHand[0].Count} | right hand count {playerHand[1].Count}");
        if (playerHand[0].Count == 2 && playerHand[1].Count == 0)
        {
            // Debug.Log($"CanSplit {playerHand[0][0].cardFace.numbers == playerHand[0][1].cardFace.numbers}");
            return playerHand[0][0].cardFace.numbers == playerHand[0][1].cardFace.numbers;
        }
        return false;
    }

    public void SplitHand()
    {
        if (!handSplit)
        {
            rightBust = false;
            handSplit = true;
            playerHand[1].Add(playerHand[0][1]);
            playerHand[0].RemoveAt(1); //Remove card from left hand
            StartCoroutine(MoveCardTo(playerHand[0][0].transform, splitHandStartLeft.position));
            StartCoroutine(MoveCardTo(playerHand[1][0].transform, splitHandStartRight.position));

            Transfer(-2, 1);
        }
    }

    public void Reveal()
    {
        if (OnPlayerHandReveal != null) OnPlayerHandReveal();
    }

    public void Reset()
    {
        foreach (List<Card> cardHand in playerHand)
        {
            foreach (Card card in cardHand)
            {
                StartCoroutine(RemoveFromTable(card.transform, new Vector3(0, 0.5f, 0)));
            }
        }
        playerHand.Clear();
        playerHand.Add(new List<Card>()); //Add card deck for left hand
        playerHand.Add(new List<Card>()); //Add card deck for right hand

        leftBust = false;
        rightBust = true;
        handSplit = false;

    }

    public void Transfer(int _chips, int _potIndex)
    {
        chips += _chips;
        pot[_potIndex] -= _chips;

        if (chipsText != null) chipsText.text = "Chips: " + chips.ToString();
        if (pot1Text != null) pot1Text.text = "Pot (Left): " + pot[0].ToString();
        if (pot2Text != null) pot2Text.text = "Pot (Right): " + pot[1].ToString();
    }

    public void RemoveFirstCard()
    {
        RemoveCardAt(0, true );
    }
    public void RemoveCardAt(int index)
    {
        RemoveCardAt(index, true);
    }
    public void RemoveCardAt(int index, bool isShiftCard)
    {
        if (playerHand[0].Count > 0)
        {
            playerHand[0][index].destroy();
            playerHand[0].RemoveAt(index);
        }
        if (!isShiftCard) { return; }
        int i = 1;
        foreach (Card card in playerHand[0])
        {
            card.cardPlayerHandIndex = i-1;
            StartCoroutine(MoveCardTo(card.transform, singleHandStart.position + new Vector3(i * cardOffset, 0, 0), card));
            i++;
        }
    }
    public void RemoveLastCard()
    {
        RemoveCardAt(playerHand[0].Count - 1, false);
    }
    public int GetHandTotal()
    {
        return playerHand.Count > 0 ? playerHand[0].Count : 0;
    }
    public void AddCard(Card card, bool left = true)
    {
        Debug.Log($"AddCard ==> {card.cardFace.suit.ToString() }");
        card.SetOwner(this);
        card.transform.SetParent(cardParent);
        card.cardPlayerHandIndex = playerHand[0].Count;
        playerHand[0].Add(card);

        StartCoroutine(MoveCardTo(card.transform, singleHandStart.position + new Vector3(playerHand[0].Count * cardOffset, 0, 0), card));
        StartCoroutine(mergeCard());

    }
    IEnumerator mergeCard()
    {
        //Debug.Log($"Start merge cards in hand  {playerHand[0].Count}");
        //At least 2 cards in Hand, otherwise  ignore merge
        if (!(playerHand[0].Count >= 2)) { yield return null; }
        int lastCardBefore = playerHand[0].Count - 2;
        int maxmerge = playerHand[0].Count - 1;
        Card beforeNewCard;
        Card card;
        while (maxmerge > 0)
        {
            if (lastCardBefore < 0) { yield return null; }
            beforeNewCard = playerHand[0][lastCardBefore];
            card = playerHand[0][lastCardBefore + 1];
            // Check if last card before is same card number and same card star  
            //Debug.Log($"Card {beforeNewCard.cardFace.suit} Star: {beforeNewCard.cardFace.star} VS Card {card.cardFace.suit} Star {card.cardFace.star} ");
            if (beforeNewCard.cardFace.suit == card.cardFace.suit && beforeNewCard.cardFace.star == card.cardFace.star && ((int)beforeNewCard.cardFace.star + 1) < MAXCARDSTAR)
            {
                //Increase 1 star to before card,  Text is setting + 2 , becuase the enum cardFace.star start with 0 
                beforeNewCard.cardStar.text = "" + ((int)card.cardFace.star + 2);
                beforeNewCard.cardFace.star = (Card_Stars)((int)card.cardFace.star) + 1;
                playerHand[0][lastCardBefore] = beforeNewCard;
                //Debug.Log($"Merged card {lastCardBefore } ==> star {beforeNewCard.cardStar.text}  ");

                //playerHand[0][lastCardBefore + 1].destroy();
                //playerHand[0].RemoveAt(lastCardBefore + 1);
                //yield return new WaitForSeconds(0.5f);
                RemoveLastCard();

                lastCardBefore = playerHand[0].Count - 2;
            }
            maxmerge--;
            Debug.Log($"Card in hand  {PrintAllCards(playerHand[0])} , checking card index {lastCardBefore} / {lastCardBefore + 1} , megre round remain : {maxmerge} ");
        }
        yield return new WaitForSeconds(0.2f);
    }
    private string PrintAllCards(List<Card> cards)
    {
        string result = "";
        for (int i = 0; i < cards.Count; i++)
        {
            result += "" + (i + 1) + ":" + cards[i].cardFace.suit + ":" + cards[i].cardFace.star + ",";
        }

        return result;
    }

    IEnumerator MoveCardTo(Transform cardTransform, Vector3 targetPosition, Card card = null)
    {
        //IF Card is not merged
        if (card != null)
        {
            card.cardSpawnButton.transform.position = targetPosition;
        }
        yield return new WaitForSeconds(0.5f);
    }


    IEnumerator RemoveFromTable(Transform cardTransform, Vector3 targetPosition)
    {
        Vector3 v360 = new Vector3(0, 0, 180);
        while ((cardTransform.position - targetPosition).sqrMagnitude > 0.00000001f)
        {
            // cardTransform.position = Vector3.MoveTowards(cardTransform.position, targetPosition, Time.deltaTime * cardMoveSpeed);
            cardTransform.localEulerAngles = Vector3.Lerp(cardTransform.localEulerAngles, v360, Time.deltaTime * 5);
            yield return null;
        }
        // cardTransform.position = targetPosition;
        cardTransform.localEulerAngles = Vector3.zero;

        Destroy(cardTransform.gameObject);
    }

}
