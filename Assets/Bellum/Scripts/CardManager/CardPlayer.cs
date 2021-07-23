using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class CardPlayer : MonoBehaviour
{

    private CardSlot cardslot;
    [Header("References")]
    [SerializeField] Transform cardSlotParent;
    [SerializeField] CardSlot cardSlotPrefab;
    [SerializeField] CardDealer cardDealer;
    [SerializeField] EnemyAI enemyCardDealer;
    [SerializeField] public UnitSkillArt unitSkillArt;

    [Header("Layout References")]
    [SerializeField] Transform singleHandStart;
    public float screenOffset;

    [Header("Settings")]
    [SerializeField] float cardOffset = 150f; // NO Effect to change here, need to set it in inspector
    [SerializeField] float cardMoveSpeed = 10;// NO Effect to change here, need to set it in inspector
    [SerializeField] public int MAXCARDSTAR = 2;
    [SerializeField] public bool isEnemy;

    [Header("Debug")]
    [SerializeField] List<List<Card>> playerHand = new List<List<Card>>();
    int totalCardSlot = 0;
    Vector3 v360 = new Vector3(0, 0, 180);
    public List<CardSlot> cardSlotlist = new List<CardSlot>();
    public static event Action<Card> CardRemoved;
    
    void Awake()
    {
        Reset();
        unitSkillArt.initDictionary();
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
            if (isEnemy == true)
            {
                cardslot.GetComponent<RectTransform>().SetAnchor(AnchorPresets.TopCenter);
            }
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
        //Debug.Log($"Player.moveCardAt() Exit {PrintAllCards(playerHand[0])}");
    }
    public void RemoveLastCard(int index)
    {
        moveCard(index, true);
    }
    public int GetHandTotal()
    {
        return playerHand.Count > 0 ? playerHand[0].Count : 0;
    }
    public IEnumerator AddCard(Card card, bool left = true)
    {
        //Debug.Log($"Player.AddCard() ==> {name } {card.enemyCard}");
        card.cardPlayerHandIndex = playerHand[0].Count;
        playerHand[0].Add(card);
        
        card.transform.SetParent(cardSlotlist[playerHand[0].Count-1].transform);
        yield return MoveCardTo(card.transform, cardSlotlist[playerHand[0].Count - 1].transform.position, card);
        //StartCoroutine(mergeCard());

    }
    public  IEnumerator mergeCard()
    {
        //Debug.Log($"Player.mergeCard() Calling Mereg {PrintAllCards(playerHand[0])}");
        //Debug.Log($"Start merge cards in hand  {playerHand[0].Count}");
        //At least 2 cards in Hand, otherwise  ignore merge
        if (!(playerHand[0].Count >= 2)) { yield break; }
        int lastCardBefore = playerHand[0].Count - 2;
        int maxmerge = playerHand[0].Count - 1;
        Card beforeNewCard;
        Card card;
        int star = 0;
        while (maxmerge > 0)
        {
            if (lastCardBefore < 0) { yield break; }
            //Debug.Log($"Player.mergeCard() maxmerge:{maxmerge}");
            beforeNewCard = playerHand[0][lastCardBefore];
            card = playerHand[0][lastCardBefore + 1];
            // Check if last card before is same card number and same card star  
            // Debug.Log($"Card {beforeNewCard.cardFace.suit} Star: {beforeNewCard.cardFace.star} VS Card {card.cardFace.suit} Star {card.cardFace.star} ");
            if (beforeNewCard.cardFace.numbers == card.cardFace.numbers && beforeNewCard.cardFace.star == card.cardFace.star && ((int)beforeNewCard.cardFace.star + 1) < MAXCARDSTAR)
            {
                //Debug.Log(beforeNewCard.transform.Find("CFX4 Sparks Explosion B"));
                //beforeNewCard.transform.Find("CFX4 Sparks Explosion B").gameObject.SetActive(true);
                beforeNewCard.playMergeEffect();
                star = (int)card.cardFace.star;
                //Increase 1 star to before card,  Text is setting + 2 , becuase the enum cardFace.star start with 0 
                beforeNewCard.cardStar.text = "" + (star + 2);
                beforeNewCard.cardFace.star = (Card_Stars) (star + 1);
                playerHand[0][lastCardBefore] = beforeNewCard;
                if (Enum.TryParse(beforeNewCard.cardFace.numbers.ToString(), out UnitMeta.UnitType unitType))
                {
                    beforeNewCard.skillIcon.gameObject.SetActive((UnitMeta.UnitStarSkill[(star + 2)][unitType]) != UnitMeta.UnitSkill.NOTHING);
                    beforeNewCard.skillIcon.sprite = unitSkillArt.UnitSkillImageDictionary[(UnitMeta.UnitStarSkill[(star + 2)][unitType]).ToString()].image;
                }
                for (int j = 0 ; j < star + 2; j++)
                {
                    beforeNewCard.stars.transform.GetChild(j).Find("Active" + (star + 2)).gameObject.SetActive(true);
                }
                beforeNewCard.cardFrame.transform.GetChild(star + 1).gameObject.SetActive(true);
                //Debug.Log($"***** Merged card {lastCardBefore } ==> star: {beforeNewCard.cardStar.text} / {beforeNewCard.cardFace.star} ");
                //playerHand[0][lastCardBefore + 1].destroy();
                //playerHand[0].RemoveAt(lastCardBefore + 1);

                RemoveLastCard(lastCardBefore + 1);
                lastCardBefore = playerHand[0].Count - 2;
                maxmerge = playerHand[0].Count - 1;
                //lastCardBefore = playerHand[0].Count - 2;

                // 2021-05-14 Anthea call yield return null to return control to unity. code will continue
                yield return null;
            }
            lastCardBefore--;
            maxmerge--;
            //Debug.Log($"Leaving Card in hand  {PrintAllCards(playerHand[0])} , checking card index {lastCardBefore} / {lastCardBefore + 1} , megre round remain : {maxmerge} ");
        }
        yield return null;
    }
    public void dragCardMerge()
    {
        //Debug.Log($"----- Player.dragCardMerge() call mergeCard()");
        StartCoroutine(mergeCard());
    }
    private string PrintAllCards(List<Card> cards)
    {
        string result = "";
        for (int i = 0; i < cards.Count; i++)
        {
            RectTransform rect = cards[i].GetComponent<RectTransform>();
            result += "" + (i + 1) + ":" + cards[i].cardFace.numbers + ":" + cards[i].cardFace.star + ":" + rect.position.x + ",";
        }
        return result;
    }
    IEnumerator MoveCardTo(Transform cardTransform, Vector3 targetPosition, Card card = null)
    {
        // break if card is merged
        while (cardTransform != null && (cardTransform.position - targetPosition).sqrMagnitude > 0.00000001f && isEnemy == false)
        {
            cardTransform.position = Vector3.MoveTowards(cardTransform.position, targetPosition, Time.deltaTime * cardMoveSpeed);
            cardTransform.localEulerAngles = Vector3.Lerp(cardTransform.localEulerAngles, v360, Time.deltaTime * 5);
            //Debug.Log($"inside MoveCardTo() while loop cardTransform.position:{cardTransform.position}");
            //yield return null;
        }
        if (cardTransform != null) { 
            cardTransform.position = targetPosition;
            cardTransform.localEulerAngles = Vector3.zero;
            //Debug.Log($"inside MoveCardTo() if (cardTransform != null) cardTransform.position:{cardTransform.position}");
        }
        //Debug.Log($"+++++ Player.MoveCardTo() call mergeCard()");
        if (isEnemy == true)
        {
            enemyCardDealer.SetCards(card);
        }
        yield return mergeCard();
        
    }
    public List<Card> GetCards()
    {
        return playerHand[0];
    }
    public void moveCard(int index, bool isShiftCard = true)
    {
        //Debug.Log($"Player.moveCard() isEnemy:{isEnemy} Remove:{playerHand[0][index]} {enemyCardDealer.cards.IndexOf(playerHand[0][index])}");
       
        if (isEnemy == true)
        {
            int b = enemyCardDealer.cards.IndexOf(playerHand[0][index]);
            enemyCardDealer.cards.RemoveAt(b);
        }
        
        if (playerHand[0].Count > 0)
        {
            //playerHand[0][index].destroy();
            
            playerHand[0][index].enemyCard = false;
            CardRemoved?.Invoke(playerHand[0][index]);
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
        //Debug.Log($"moveCard() exit {PrintAllCards(playerHand[0])}");
    }
    public void moveOneCard(int index)
    {
        playerHand[0][index].transform.SetParent(cardSlotlist[index].transform);
        //playerHand[0][index].transform.position = cardSlotlist[index].transform.position;
        playerHand[0][index].GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
    }

}
