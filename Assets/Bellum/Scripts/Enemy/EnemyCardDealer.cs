using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCardDealer : MonoBehaviour
{
    private List<Card> cards = new List<Card>();
    [SerializeField] TotalEleixier eleixier;
    private int elexier = 0;
    private Card nextCard;
    private bool startDeal = true;
    // Start is called before the first frame update
    void Start()
    {
        TotalEleixier.UpdateEnemyElexier += OnUpdateElexier;
    }
    private void OnDestroy()
    {
        TotalEleixier.UpdateEnemyElexier -= OnUpdateElexier;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnUpdateElexier(int elexier)
    {
        //Debug.Log($"OnUpdateElexier{elexier}");
        this.elexier = elexier;
        if (nextCard == null) { return; }
        if(nextCard.GetUnitElexier() < this.elexier)
        {
            /*RectTransform rect = nextCard.GetComponent<RectTransform>();
            float x = rect.localScale.x;
            float y = rect.localScale.y;
            float z = rect.localScale.z;
            rect.localScale = new Vector3(x /= (float)1.5, y /= (float)1.5, z /= (float)1.5);*/
            nextCard.OnPointerDown();
            StartCoroutine(SelectNextCard(1));
        }
        
    }
    private IEnumerator SelectNextCard(int waitSec)
    {
        yield return new WaitForSeconds(waitSec);
       // Debug.Log("SelectNextCard");
        nextCard = cards[Random.Range(0, cards.Count - 1)];
        RectTransform rect = nextCard.GetComponent<RectTransform>();
        float x = rect.localScale.x;
        float y = rect.localScale.y;
        float z = rect.localScale.z;
        rect.localScale = new Vector3(x *= (float)1.5, y *= (float)1.5, z *= (float)1.5);
    }
    public void SetCards(Card card)
    {
        this.cards.Add(card);
        if(cards.Count == 6 && startDeal == true)
        {
            startDeal = false;
            StartCoroutine(SelectNextCard(0));
        }
    }
}
