using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] Vector3 cardOffset = new Vector3(0.005f, 0.005f, 0.005f); //5mm
    [SerializeField] float cardMoveSpeed = 10;

    [Header("Debug")]
    [SerializeField] List<List<Card>> playerHand = new List<List<Card>>();
    bool handSplit = false;
    public bool leftBust = false;
    public bool rightBust = false;
    public int handTotal;
    float x = (float)-28;
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
        if(playerHand[0].Count > 0)
            playerHand[0].RemoveAt(0);
    }

    public void AddCard(Card card, bool left = true)
    {
        Debug.Log($"Add Card {card}");
        card.SetOwner(this);
        card.transform.SetParent(cardParent);
        if (!handSplit)
        {
            playerHand[0].Add(card);

            StartCoroutine(MoveCardTo(card.transform, singleHandStart.position/* + (playerHand[0].Count * cardOffset*/, card, playerHand[0].IndexOf(card)));
        }
        else
        {
            if (left)
            {

                playerHand[0].Add(card);
                StartCoroutine(MoveCardTo(card.transform, splitHandStartLeft.position + ((playerHand[0].Count - 1) * cardOffset), card, playerHand[0].IndexOf(card)));
            }
            else
            {

                playerHand[1].Add(card);
                StartCoroutine(MoveCardTo(card.transform, splitHandStartRight.position + ((playerHand[1].Count - 1) * cardOffset), card, playerHand[1].IndexOf(card)));
            }
        }
        mergeCard(card);
    }
    public void mergeCard(Card card)
    {
        Debug.Log($"number card player in Hand  {playerHand[0].Count}");
        //IF only one card in Hand, ignore merge
        if (playerHand[0].Count == 1) { return; }
        int lastCardBefore = playerHand[0].Count - 2;
        //for (int i = 0; i < playerHand[0].Count - 1; i++)
        //{

            if (playerHand[0][lastCardBefore].cardFace.numbers == card.cardFace.numbers && playerHand[0][lastCardBefore].GetComponentInChildren<Image>().color != Color.gray)
            {
                Debug.Log($"try merge");

                //card.destroy();
                playerHand[0][lastCardBefore + 1].destroy();
                playerHand[0][lastCardBefore].GetComponentInChildren<Image>().color = Color.gray;
                playerHand[0][lastCardBefore].GetComponentInChildren<Image>().tag = "Card(clone gray)";
                playerHand[0].RemoveAt(lastCardBefore + 1);

                /*
                if (z == 1)
                {
                    buttons[buttons.Count - 2]
                    .transform.localPosition =
                    new Vector3(buttons[buttons.Count - 2]
                    .transform.localPosition.x - 100, -200, 0);
                }
                z++;
                */
                //bigMerge(cardbefore, buttons[buttons.Count - 2]);
            }
        //}
    }
    /*
    public void bigMerge(Card cardbefore, Button button)
    {
        Debug.Log(3);
        if (b >= 2)
        {
            Debug.Log(cards[cards.IndexOf(cardbefore) - 1]);
            if (cards[cards.IndexOf(cardbefore) - 1] != null)
            {
                Card cardBeforeMore = cards[cards.IndexOf(cardbefore) - 1];
                Debug.Log(2);
                while ((int)cardBeforeMore.cardFace.numbers > 13)
                {
                    cardBeforeMore.cardFace.numbers -= 13;


                }
                if (cardbefore.cardFace.numbers == cardBeforeMore.cardFace.numbers && buttons[buttons.IndexOf(button) - 1].tag == "Card(clone Gray)")
                {
                    if (cardBeforeMore == null) { return; }
                    cardbefore.GetComponent<Card>().destroy();
                    buttons[buttons.Count - 1].GetComponent<Image>().color = Color.yellow;
                    Debug.Log($"try big merge");
                    Hitmerge();
                }
            }


        }
        Hitmerge();
    }
    */
    IEnumerator MoveCardTo(Transform cardTransform, Vector3 targetPosition, Card card = null, int index = 0)
    {
        Vector3 v360 = new Vector3(0, 0, 180);
        // while ((cardTransform.position - targetPosition).sqrMagnitude > 0.00000001f)
        {

            //cardTransform.position = Vector3.MoveTowards(cardTransform.position, targetPosition, Time.deltaTime * cardMoveSpeed);

            //  cardTransform.localEulerAngles = Vector3.Lerp(cardTransform.localEulerAngles, v360, Time.deltaTime * 5);
            yield return null;
        }
        if (cardTransform != null)
        {

            cardTransform.position = new Vector3(x, 38, (float)-22.7);
            x += (float)5;

            //   cardTransform.localEulerAngles = targetPosition;

            //Flip Card
            card?.Flip(index);
            var rotationVector = transform.rotation.eulerAngles;
            rotationVector.x = 90;
            rotationVector.z = 5;
            cardTransform.rotation = Quaternion.Euler(rotationVector);
            cardTransform.localScale = new Vector3((float)17, 17, 1);

            yield return null;
        }
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
