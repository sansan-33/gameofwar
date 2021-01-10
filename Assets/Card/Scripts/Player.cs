using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace At0m1c.Blackjack {

    public class Player : MonoBehaviour {

        public delegate void PlayerHandEvents ();
        public PlayerHandEvents OnPlayerHandReveal;

        public string playerName = "Player";
        public int chips = 100;

        [Header ("References")]
        [SerializeField] Transform cardParent;

        [Header ("Layout References")]
        [SerializeField] Transform singleHandStart;
        [SerializeField] Transform splitHandStartLeft;
        [SerializeField] Transform splitHandStartRight;

        [Header ("Settings")]
        [SerializeField] Vector3 cardOffset = new Vector3 (0.005f, 0.005f, 0.005f); //5mm
        [SerializeField] float cardMoveSpeed = 10;

        [Header ("Debug")]
        [SerializeField] List<List<Card>> playerHand = new List<List<Card>> ();
        bool handSplit = false;
        public bool leftBust = false;
        public bool rightBust = false;
        public int handTotal;

        public bool dealer = false;
        public int[] pot = new int[2];
        [SerializeField] Text chipsText;
        [SerializeField] Text pot1Text;
        [SerializeField] Text pot2Text;

        void Awake () {
            Reset ();
        }

        public void SetAsDealer () {
            dealer = true;
        }

        public bool CanSplit () {
            Debug.Log ($"Left hand count {playerHand[0].Count} | right hand count {playerHand[1].Count}");
            if (playerHand[0].Count == 2 && playerHand[1].Count == 0) {
                Debug.Log ($"CanSplit {playerHand[0][0].cardFace.numbers == playerHand[0][1].cardFace.numbers}");
                return playerHand[0][0].cardFace.numbers == playerHand[0][1].cardFace.numbers;
            }
            return false;
        }

        public void SplitHand () {
            if (!handSplit) {
                rightBust = false;
                handSplit = true;
                playerHand[1].Add (playerHand[0][1]);
                playerHand[0].RemoveAt (1); //Remove card from left hand
                StartCoroutine (MoveCardTo (playerHand[0][0].transform, splitHandStartLeft.position));
                StartCoroutine (MoveCardTo (playerHand[1][0].transform, splitHandStartRight.position));

                Transfer (-2, 1);
            }
        }

        public void Reveal () {
            if (OnPlayerHandReveal != null) OnPlayerHandReveal ();
        }

        public void Reset () {
            foreach (List<Card> cardHand in playerHand) {
                foreach (Card card in cardHand) {
                    StartCoroutine (RemoveFromTable (card.transform, new Vector3 (0, 0.5f, 0)));
                }
            }
            playerHand.Clear ();
            playerHand.Add (new List<Card> ()); //Add card deck for left hand
            playerHand.Add (new List<Card> ()); //Add card deck for right hand

            leftBust = false;
            rightBust = true;
            handSplit = false;

        }

        public void Transfer (int _chips, int _potIndex) {
            chips += _chips;
            pot[_potIndex] -= _chips;

            if (chipsText != null) chipsText.text = "Chips: " + chips.ToString ();
            if (pot1Text != null) pot1Text.text = "Pot (Left): " + pot[0].ToString ();
            if (pot2Text != null) pot2Text.text = "Pot (Right): " + pot[1].ToString ();
        }

        public List<int> CalculateHands (out bool blackjack) {
            List<int> handTotals = new List<int> ();
            blackjack = false;

            //Check for Blackjack
            if (playerHand.Count == 1) {
                //A Blackjack, or natural, has a total of 21 in your first two cards. 
                //A Blackjack is therefore an ace and any 10-valued card, with the additional requirement that these must be your first two cards.
                if (playerHand[0].Count == 2) {
                    if ((int) playerHand[0][0].cardFace.numbers == 0 && (int) playerHand[0][1].cardFace.numbers + 1 == 10) {
                        //BLACKJACK
                        handTotals.Add (-1);
                        blackjack = true;
                        handTotal = handTotals[0];
                        return handTotals;
                    }
                }
            }

            bool left = false;
            //No Blackjack, keep going
            foreach (List<Card> cardHand in playerHand) {
                left = !left;
                int total = 0;
                int aces = 0;
                foreach (Card card in cardHand) {
                    int cardIndex = (int) card.cardFace.numbers;
                    if (cardIndex == 0) aces++;
                    else {
                        //All face cards are 10
                        if (cardIndex + 1 >= 10) {
                            total += 10;
                        } else {
                            total += cardIndex + 1;
                        }
                    }
                }

                //Calculate aces
                //Two aces (11) will always cause bust, therefore add 1 for every ace until the last one then determine if it is 1 or 11
                Debug.Log ($"{playerName} aces {aces}");
                for (int i = 0; i < aces; i++) {
                    if (dealer) {
                        if (total + 11 >= 17) {
                            total += 11;
                        } else {
                            total += 1;
                        }
                    } else {
                        if (i < aces - 1) {
                            Debug.Log ($"{playerName} | More aces to come, adding 1");
                            total += 1;
                        } else {
                            if (total < 11) {
                                Debug.Log ($"{playerName} | Total is {total}, last ace, adding 11");
                                total += 11;
                            } else {
                                Debug.Log ($"{playerName} | Total is {total}, adding 1");
                                total += 1;
                            }
                        }
                    }
                }

                handTotals.Add (total);
                if (total > 21) {
                    if (left) leftBust = true;
                    else rightBust = true;
                    foreach (Card card in cardHand) {
                        card.FadeOut ();
                    }
                }
                Debug.Log ($"{playerName} hand total {total}");
            }

            handTotal = handTotals[0];
            return handTotals;
        }

        public void AddCard (Card card, bool left = true) {
            card.SetOwner (this);
            card.transform.SetParent (cardParent);
            if (!handSplit) {
                playerHand[0].Add (card);
                StartCoroutine (MoveCardTo (card.transform, singleHandStart.position + (playerHand[0].Count * cardOffset), card, playerHand[0].IndexOf (card)));
            } else {
                if (left) {
                    playerHand[0].Add (card);
                    StartCoroutine (MoveCardTo (card.transform, splitHandStartLeft.position + ((playerHand[0].Count - 1) * cardOffset), card, playerHand[0].IndexOf (card)));
                } else {
                    playerHand[1].Add (card);
                    StartCoroutine (MoveCardTo (card.transform, splitHandStartRight.position + ((playerHand[1].Count - 1) * cardOffset), card, playerHand[1].IndexOf (card)));
                }
            }
            CalculateHands (out bool bj);
        }

        IEnumerator MoveCardTo (Transform cardTransform, Vector3 targetPosition, Card card = null, int index = 0) {
            Vector3 v360 = new Vector3 (0, 0, 180);
            while ((cardTransform.position - targetPosition).sqrMagnitude > 0.00000001f) {
                cardTransform.position = Vector3.MoveTowards (cardTransform.position, targetPosition, Time.deltaTime * cardMoveSpeed);
                cardTransform.localEulerAngles = Vector3.Lerp (cardTransform.localEulerAngles, v360, Time.deltaTime * 5);
                yield return null;
            }
            cardTransform.position = targetPosition;
            cardTransform.localEulerAngles = Vector3.zero;

            //Flip Card
            card?.Flip (index);
            yield return null;
        }

        IEnumerator RemoveFromTable (Transform cardTransform, Vector3 targetPosition) {
            Vector3 v360 = new Vector3 (0, 0, 180);
            while ((cardTransform.position - targetPosition).sqrMagnitude > 0.00000001f) {
                cardTransform.position = Vector3.MoveTowards (cardTransform.position, targetPosition, Time.deltaTime * cardMoveSpeed);
                cardTransform.localEulerAngles = Vector3.Lerp (cardTransform.localEulerAngles, v360, Time.deltaTime * 5);
                yield return null;
            }
            cardTransform.position = targetPosition;
            cardTransform.localEulerAngles = Vector3.zero;

            Destroy (cardTransform.gameObject);
        }

    }

}