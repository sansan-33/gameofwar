using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class Player : MonoBehaviour
{

    public delegate void PlayerHandEvents();
    public PlayerHandEvents OnPlayerHandReveal;
    private CardSlot cardslot;
    [Header("References")]
    [SerializeField] Transform cardSlotParent;
    [SerializeField] CardSlot cardSlotPrefab;
    [SerializeField] CardDealer cardDealer;

    [Header("Layout References")]
    [SerializeField] Transform singleHandStart;
    public float screenOffset;
    [Header("Settings")]
    [SerializeField] float cardOffset = 150f; // NO Effect to change here, need to set it in inspector
    [SerializeField] float cardMoveSpeed = 10;// NO Effect to change here, need to set it in inspector
    [SerializeField] int MAXCARDSTAR = 2;
   
    [Header("Debug")]
    [SerializeField] List<List<Card>> playerHand = new List<List<Card>>();
    int totalCardSlot = 0;
    Vector3 v360 = new Vector3(0, 0, 180);

    private List<CardSlot> cardSlotlist = new List<CardSlot>();
   
    void Awake()
    {
        Reset();
    }

    public void Reset()
    {
        playerHand.Clear();
        playerHand.Add(new List<Card>()); //Add card deck for left hand
        playerHand.Add(new List<Card>()); //Add card deck for right hand
        cardSlotlist.Clear();
        while (totalCardSlot < cardDealer.MAXTOTALHAND)
        {
            cardslot = Instantiate(cardSlotPrefab).GetComponent<CardSlot>();
            cardslot.transform.SetParent(cardSlotParent);
            totalCardSlot++;
            cardslot.transform.position = singleHandStart.transform.position + new Vector3(totalCardSlot * cardOffset, 0, 0);
            cardSlotlist.Add(cardslot);
        }
    }
    public void moveCardAt(int cardMovingindex, string direction)
    {
        bool isMoveLeft = direction == "left" ? true : false;
        if (playerHand[0].Count > 0)
        {
           
            if (isMoveLeft == true)
            {
                if (cardMovingindex == 0) { return; }
               
                Card cardBefore = playerHand[0][cardMovingindex - 1];

                playerHand[0][cardMovingindex -1] = playerHand[0][cardMovingindex];
                playerHand[0][cardMovingindex] = cardBefore;
                playerHand[0][cardMovingindex ].cardPlayerHandIndex++;
                playerHand[0][cardMovingindex-1].cardPlayerHandIndex--;
                moveOneCard(cardMovingindex);
                moveOneCard(cardMovingindex - 1);
            }
            else
            {
                if (playerHand[0].Count-1 < cardMovingindex+1) { return; }
               
                Card cardAfter = playerHand[0][cardMovingindex + 1];

                playerHand[0][cardMovingindex + 1] = playerHand[0][cardMovingindex];
                playerHand[0][cardMovingindex] = cardAfter;
                playerHand[0][cardMovingindex].cardPlayerHandIndex--;
                playerHand[0][cardMovingindex + 1].cardPlayerHandIndex++;
                moveOneCard(cardMovingindex);
                moveOneCard(cardMovingindex+1);
            }
        }
    }
    public void RemoveLastCard(int index)
    {
        // Need to shift every card if merged 
        moveCard(index, true);
        /*
        if (index == (playerHand[0].Count - 1) && (playerHand[0].Count-1) == 6)
        {
            Debug.Log($"index last {index}, shift card ");
            moveCard(index,true);
        }
        else
        {
            Debug.Log($"index not last {index}, not shift card");
            moveCard(index, true);
        }
        */
        
    }
    public int GetHandTotal()
    {
        return playerHand.Count > 0 ? playerHand[0].Count : 0;
    }
    public void AddCard(Card card, bool left = true)
    {
        //Debug.Log($"AddCard ==> {card.cardFace.suit.ToString() }");
        card.SetOwner(this);
        card.cardPlayerHandIndex = playerHand[0].Count;
        playerHand[0].Add(card);
      
        card.transform.SetParent(cardSlotlist[playerHand[0].Count-1].transform);
        StartCoroutine(MoveCardTo(card.transform, cardSlotlist[playerHand[0].Count - 1].transform.position, card));
        StartCoroutine(mergeCard());

    }
    IEnumerator mergeCard()
    {
        // Debug.Log($"Calling Mereg {PrintAllCards(playerHand[0])}");
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
            if (beforeNewCard.cardFace.numbers == card.cardFace.numbers && beforeNewCard.cardFace.star == card.cardFace.star && ((int)beforeNewCard.cardFace.star + 1) < MAXCARDSTAR)
            {
                
                //Increase 1 star to before card,  Text is setting + 2 , becuase the enum cardFace.star start with 0 
                beforeNewCard.cardStar.text = "" + ((int)card.cardFace.star + 2);
                beforeNewCard.cardFace.star = (Card_Stars)((int)card.cardFace.star) + 1;
                playerHand[0][lastCardBefore] = beforeNewCard;
                //Debug.Log($"Merged card {lastCardBefore } ==> star {beforeNewCard.cardStar.text}  ");

                //playerHand[0][lastCardBefore + 1].destroy();
                //playerHand[0].RemoveAt(lastCardBefore + 1);
                //yield return new WaitForSeconds(0.5f);
               
                RemoveLastCard(lastCardBefore + 1);
                lastCardBefore = playerHand[0].Count - 2;
                maxmerge = playerHand[0].Count - 1;
                //lastCardBefore = playerHand[0].Count - 2;
            }
            lastCardBefore--;
            maxmerge--;
            //Debug.Log($"Leaving Card in hand  {PrintAllCards(playerHand[0])} , checking card index {lastCardBefore} / {lastCardBefore + 1} , megre round remain : {maxmerge} ");
        }
        yield return new WaitForSeconds(0.2f);
    }
    public void dragCardMerge()
    {
        StartCoroutine(mergeCard());
    }
    private string PrintAllCards(List<Card> cards)
    {
        string result = "";
        for (int i = 0; i < cards.Count; i++)
        {
            result += "" + (i + 1) + ":" + cards[i].cardFace.numbers + ":" + cards[i].cardFace.star + ",";
        }
        return result;
    }
    IEnumerator MoveCardTo(Transform cardTransform, Vector3 targetPosition, Card card = null)
    {
        // break if card is merged
        while (cardTransform != null && (cardTransform.position - targetPosition).sqrMagnitude > 0.00000001f)
        {
            cardTransform.position = Vector3.MoveTowards(cardTransform.position, targetPosition, Time.deltaTime * cardMoveSpeed);
            cardTransform.localEulerAngles = Vector3.Lerp(cardTransform.localEulerAngles, v360, Time.deltaTime * 5);
            yield return null;
        }
        if (cardTransform != null) { 
            cardTransform.position = targetPosition;
            cardTransform.localEulerAngles = Vector3.zero;
        }
        yield return new WaitForSeconds(0.5f);
    }

    public void moveCard(int index, bool isShiftCard = true)
    {
        if (playerHand[0].Count > 0)
        {
            playerHand[0][index].destroy();
            playerHand[0].RemoveAt(index);
        }

        if (!isShiftCard) { return; }
        int i = 0;
        
        foreach (Card card in playerHand[0])
        {
            //Debug.Log($"Shift Card {card.cardFace.numbers} {card.cardFace.star}==> from {card.cardPlayerHandIndex} to {i}  ");
            card.cardPlayerHandIndex = i;
            card.transform.SetParent(cardSlotlist[i].transform);
            card.transform.position = cardSlotlist[i].transform.position;
            i++;
        }
        
    }
    public void moveOneCard(int index)
    {
        playerHand[0][index].transform.SetParent(cardSlotlist[index].transform);
        playerHand[0][index].transform.position = cardSlotlist[index].transform.position;
    }

}
