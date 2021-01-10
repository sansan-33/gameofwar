using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace At0m1c.Blackjack {

    public class Card : MonoBehaviour {
        bool flipped = false;
        Animator animator;
        Player owner;

        public CardFace cardFace;
        [SerializeField] Renderer cardRenderer;
        Material cardFrontMat;

        void Awake () {
            animator = GetComponent<Animator> ();
            cardFrontMat = cardRenderer.materials[0];
        }

        public void SetCard (CardFace _cardFace, CardFaceCoords coord) {
            cardFace = _cardFace;
            if (coord != null) {
                cardFrontMat.SetTextureOffset ("_BaseMap", coord.coord);
                cardRenderer.materials[0] = cardFrontMat;
            }
        }

        public void FadeOut () {
            StartCoroutine (FadeOutCard ());
        }

        IEnumerator FadeOutCard () {
            Color col = cardFrontMat.GetColor("_BaseColor");
            Color colTarget = Color.grey;
            float change = 0;
            while (change < 1){
                change += Time.deltaTime * 10;
                cardFrontMat.SetColor("_BaseColor", Color.Lerp(col, colTarget, change));
                yield return null;
            }
        }

        void OnDisable () {
            if (owner != null)
                owner.OnPlayerHandReveal -= Reveal;
        }

        void Reveal () {
            if (!flipped) Flip (0);
        }

        public void SetOwner (Player player) {
            owner = player;
            owner.OnPlayerHandReveal += Reveal;
        }

        public void Flip (int cardIndex) {
            if (cardIndex == 1 && owner.dealer) {
                //First dealer card - don't flip yet
            } else {
                animator.SetTrigger ("Flip");
                flipped = true;
            }
        }
    }

    public enum Card_Suits {
        Hearts,
        Diamonds,
        Clubs,
        Spades
    }

    public enum Card_Numbers {
        Ace,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King
    }
}