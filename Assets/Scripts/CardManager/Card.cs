using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    
    
        bool flipped = false;
        Animator animator;
        Player owner;

        public CardFace cardFace;
    public CardFaceCoords coord;
        [SerializeField] Renderer cardRenderer;
        Material cardFrontMat;
    float X = (float)90;
    
    [SerializeField] Transform button;
    [SerializeField] Button gameobject;


    void Awake()
        {
            animator = GetComponent<Animator>();
            cardFrontMat = cardRenderer.materials[0];
        }

        public void SetCard(CardFace _cardFace, CardFaceCoords coord, string cardid)
    {
        
       // GameObject other = GameObject.FindGameObjectWithTag("Card(clone)");
        //BehaviorSelection a = other.GetComponent<BehaviorSelection>();
        //string attack = "attack";
      //  if (cardid == attack)
        //{
            
        //    gameobject.onClick.AddListener(a.TryAttack); //subscribe to the onClick 
          
       // }
        
             //subscribe to the onClick
       
        cardFace = _cardFace;
        
        
        if (coord != null)
        {
            
            cardFrontMat.SetTextureOffset("_BaseMap", coord.coord);
                cardRenderer.materials[0] = cardFrontMat;
            }
        }

   
    void OnPointerDown()
    {
        Debug.Log(this.cardFace.suit);
        Debug.Log(this.cardFace.numbers);

    }
    
        public void FadeOut()
        {
            StartCoroutine(FadeOutCard());
        }

        IEnumerator FadeOutCard()
        {
            Color col = cardFrontMat.GetColor("_BaseColor");
            Color colTarget = Color.grey;
            float change = 0;
            while (change < 1)
            {
                change += Time.deltaTime * 10;
                cardFrontMat.SetColor("_BaseColor", Color.Lerp(col, colTarget, change));
                yield return null;
            }
        }

        void OnDisable()
        {
            if (owner != null)
                owner.OnPlayerHandReveal -= Reveal;
        }

        void Reveal()
        {
            if (!flipped) Flip(0);
        }

        public void SetOwner(Player player)
        {
            owner = player;
            owner.OnPlayerHandReveal += Reveal;
        }

        public void Flip(int cardIndex)
        {
            if (cardIndex == 1 && owner.dealer)
            {
                //First dealer card - don't flip yet
            }
            else
            {
                animator.SetTrigger("Flip");
                flipped = true;
            }
        
    }
    public void setputtonposition(int number)
    {
        while (number > 0)
        {
            button.transform.position = new Vector3(X, 230, 0);
          
            X += 90;
            number--;
        }
      
    }

    }

    public enum Card_Suits
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades
    }

    public enum Card_Numbers
    {
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
