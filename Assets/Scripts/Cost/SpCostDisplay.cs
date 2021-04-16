using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpCostDisplay : MonoBehaviour
{
    [SerializeField] private List<GameObject> childSprite = new List<GameObject>();
    [HideInInspector] public int spCost;
    private int counter = 2;
    private float Timer = 1;
    public bool useTimer;
    public int waitTime = 5;
    private bool secoundLayer = false;
    private Color color;
    // Start is called before the first frame update
    void Start()
    {
        // remeber the start color
        color = childSprite[0].GetComponent<Image>().color;
    }
    
    /// <summary>
    /// Add one Sp
    /// </summary>
    /// <returns></returns>
    public IEnumerator AddSpCost()
    {
        yield return new WaitForSeconds(0);
        if(spCost < childSprite.Count*2-1)
        {
            if (secoundLayer == true)
            {
                childSprite[spCost - 18].GetComponent<Image>().color = Color.red;
            }
            else
            {
                childSprite[spCost].SetActive(true);
            }

            switch (spCost)
            {
                case 17:
                    spCost++;
                    counter += 3;
                    secoundLayer = true;
                    break;
                default:
                    if (spCost < counter)
                    {
                        spCost++;
                        StartCoroutine(AddSpCost());
                    }
                    else
                    {
                        spCost++;
                        counter += 3;
                    }
                    break;
            }
        }
    }
    /// <summary>
    /// minus sp cost. Put how many sp you want to minus in the param
    /// </summary>
    /// <param name="cost"></param>
    /// <returns></returns>
    public IEnumerator MinusSpCost(int cost)
    {
        yield return new WaitForSeconds(0);
        cost *= 3;
        while (cost >= 0)
        {
            counter--;
            cost--;
            if (secoundLayer == true)
            {
                spCost--;
                if (spCost != 17)
                {
                    childSprite[spCost - 18].GetComponent<Image>().color = color;
                }
                else
                {
                    secoundLayer = false;
                }
            }
            else
            {
                childSprite[spCost].SetActive(false);
                spCost--;
            }
            if(spCost < 0) { break; } 
        }
    }
    private void Update()
    {
        if (useTimer != false)
        {
            if (Timer > 0)
            {
                Timer -= Time.deltaTime;
            }
            else
            {
                if(spCost >= 35)
                {
                    Timer = waitTime;
                    StartCoroutine(MinusSpCost(10));
                }
                else
                {
                    Timer = waitTime;
                    StartCoroutine(AddSpCost());
                }
            }
        }
    }
}
