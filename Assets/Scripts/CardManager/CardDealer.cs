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

        [SerializeField] Transform cardDispenserSpawn;
        [SerializeField] Animator cardDispenserAnimator;
        [SerializeField] GameObject cardPrefab;

        [SerializeField] List<CardFaceCoords> cardFaceCoords = new List<CardFaceCoords>();

     //   [SerializeField] Player dealer;
        [SerializeField] List<Player> players = new List<Player>();

        [SerializeField] List<CardFace> cardDeck = new List<CardFace>();
    [SerializeField] List<string> attackType = new List<string>();
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

        void DealCard(Player player, bool left = true)
        {
            Debug.Log("Dealing Card to " + player.playerName);
            StartCoroutine(DealingCard(player, left));
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

        IEnumerator DealingCard(Player player, bool left = true)
        {
            if ((left && !player.leftBust) || (!left && !player.rightBust))
            { //Check for bust

                lastCard = Instantiate(cardPrefab).GetComponent<Card>();
              //  Debug.Log($"1 DealingCard -- > last card | {lastCard} |?? ");
                lastCard.transform.localScale = Vector3.zero;

                if (cardDeck.Count < 10)
                {
                    ShuffleDeck();
                }
                CardFace randomCard = cardDeck[UnityEngine.Random.Range(0, cardDeck.Count - 1)];
            string cardid = attackType[UnityEngine.Random.Range(0, attackType.Count)];
                cardDeckUsed.Add(randomCard);
                lastCard.GetComponent<Card>().SetCard(randomCard, GetCardFaceCoord(randomCard), cardid);
            //lastCard.GetComponent<Card>().SetCard(randomCard, GetCardFaceCoord(randomCard));
            cardDeck.Remove(randomCard);

                cardSpawned = false;
                cardDispenserAnimator.SetBool("Dealing", true);
                //Play dealing animation first
                cardDispenserAnimator.SetTrigger("Deal");
                //Card slides out to a point in front of the dispenser
                while (!cardSpawned)
                {
                    yield return null;
                }

                cardDispenserAnimator.SetBool("Dealing", false);

            //Player takes card
            Debug.Log($"2 DealingCard -- > Try Player add card | {lastCard} | ");

            player.AddCard(lastCard, left);
            numbers++;
            lastCard.setputtonposition(numbers);
            }
        }

       /* void CalculateHands()
        {
            int dealerTotal = dealer.CalculateHands(out bool dealerBlackjack)[0]; //Dealer cannot split hands
            if (dealerTotal <= 16 && !dealerBlackjack)
            {
                Debug.Log($"Dealer {dealerTotal}, dealing another card");
                StartCoroutine(DealCards(1, 0f, 0.5f, dealer, true, true));
                return;
            }

            foreach (Player player in players)
            {
                List<int> handTotals = player.CalculateHands(out bool playerBlackjack);
                if (playerBlackjack || dealerBlackjack)
                {
                    if (dealerBlackjack && playerBlackjack)
                    {
                        Debug.Log($"{player.playerName} draw | Dealer hand {dealerTotal}");
                        //Return bet
                        player.Transfer(player.pot[0], 0);
                    }
                    else if (playerBlackjack)
                    {
                        Debug.Log($"{player.playerName} hand wins by Blackjack | Dealer hand {dealerTotal}");
                        //1.5 to 1 win
                        player.Transfer((int)(player.pot[0] + (player.pot[0] * 1.5f)), 0);
                    }
                    else
                    {
                        Debug.Log($"Dealer Blackjack! Players lose.");
                        //Lose bet
                    }
                }
                else
                {
                    int index = 0;
                    foreach (int handTotal in handTotals)
                    {
                        if ((handTotal > dealerTotal || dealerTotal > 21) && handTotal <= 21)
                        {
                            Debug.Log($"{player.playerName} hand wins {handTotal} | Dealer hand {dealerTotal}");
                            player.Transfer(player.pot[index] + player.pot[index], index);
                            //1 to 1 win
                        }
                        else
                        {
                            Debug.Log($"{player.playerName} loses {handTotal} | Dealer hand wins {dealerTotal}");
                            //Lose bet
                        }
                        index++;
                    }
                }

                player.pot[0] = 0;
                player.pot[1] = 0;
            }
        }*/

        IEnumerator DealCards(int numberOfCards, float delay, float waitTime, Player player, bool left = true, bool reveal = false)
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
                    DealCard(player, left);
                }

                currentWait = waitTime;
                while (currentWait > 0)
                {
                    currentWait -= Time.deltaTime;
                    yield return null;
                }
            }

            if (reveal)
            {
                //Reveal();
            }
            if (player.dealer)
            {
               
            }
            else
            {
                if (player.CanSplit())
                {
                   
                }
                else
                {
                    
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
        Debug.Log(2);
            //Deal two cards to player
            StartCoroutine(DealCards(3, 0f, 0.5f, players[0]));
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
                StartCoroutine(DealCards(7, 0f, 0.5f, players[0], true));
                StartCoroutine(DealCards(7, 0.5f, 0.5f, players[0], false));

              //  testHit.gameObject.SetActive(false);
               // testHitLeft.gameObject.SetActive(true);
               // testHitRight.gameObject.SetActive(true);
                //testSplit.gameObject.SetActive(false);
            }
        }

        [ContextMenu("Hit")]
        public void Hit()
        {
            //testSplit.gameObject.SetActive(false);
            Debug.Log(1);
            StartCoroutine(DealCards(1, 0f, 0.5f, players[0]));
        }

      /*  [ContextMenu("Hit")]
        public void HitLeft()
        {
            StartCoroutine(DealCards(1, 0f, 0.5f, players[0], true));
        }

        [ContextMenu("Hit")]
        public void HitRight()
        {
            StartCoroutine(DealCards(1, 0f, 0.5f, players[0], false));
        }

        [ContextMenu("Reveal")]
        public void Reveal()
        {
            players.ForEach(player => player.Reveal());
            dealer.Reveal();
            CalculateHands();

        }

        [ContextMenu("Reset")]
        public void Reset()
        {
            players.ForEach(player => player.Reset());
            dealer.Reset();

        }*/

    }
