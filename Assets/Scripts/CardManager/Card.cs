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
    float X = (float)-800;
    
    [SerializeField] Transform button;
    [SerializeField] Image gameobject;
    [SerializeField] List<Sprite> sprite = new List<Sprite>();

    void Awake()
        {
            animator = GetComponent<Animator>();
            cardFrontMat = cardRenderer.materials[0];
        }

        public void SetCard(CardFace _cardFace, CardFaceCoords coord)
    {
        

        cardFace = _cardFace;
        int a = (int)cardFace.numbers;
       // gameobject = GetComponent<Button>();
        Debug.Log(a);
        gameobject.sprite = sprite[a];
      
        if (coord != null)
        {
            
            cardFrontMat.SetTextureOffset("_BaseMap", coord.coord);
                cardRenderer.materials[0] = cardFrontMat;
            }
        }
   
   
    public void OnPointerDown()
    {
        Destroy(gameObject);
        GameObject[] b = GameObject.FindGameObjectsWithTag("Card(clone)");
        foreach (GameObject cards in b)
        {
            Vector3 cardsTransform = cards.transform.localPosition;
            cardsTransform -= new Vector3(90, 0, 0);
            cards.transform.localPosition = cardsTransform;
        }
        Debug.Log(this.cardFace.suit);
        Debug.Log(this.cardFace.numbers);
        GameObject agentGroup = GameObject.FindGameObjectWithTag("AgentGroup");
        agentGroup.GetComponent<BehaviorSelection>().TryTB((int) this.cardFace.numbers);

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
            Debug.Log($"before-->{button.transform.position}");
            button.transform.position = new Vector3(X, 600, 0);
            button.position= new Vector3(X, 600, 0);
            button.localPosition= new Vector3(X, 63, 0);
            Debug.Log($"after-->{button.transform.position}");
            Debug.Log(button);
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
