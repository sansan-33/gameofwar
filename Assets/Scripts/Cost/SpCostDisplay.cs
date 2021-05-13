using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpCostDisplay : MonoBehaviour
{
    [SerializeField] private List<GameObject> childSprite = new List<GameObject>();
    [SerializeField] private ParticleSystem particleSystem1;
  //  [SerializeField] private ParticleSystem particleSystem2;
    [HideInInspector] public int spCost;
    private int counter = 2;
    private float Timer = 1;
    public bool useTimer;
    public int waitTime = 5;
    private bool secoundLayer = false;
    private Color color;
    private Unit unit;
    private GameObject SpPrefab;
    // Start is called before the first frame update
    void Start()
    {
        // remeber the start color
        color = childSprite[0].GetComponent<Image>().color;
        StartCoroutine(OnStart());
    }
    private IEnumerator OnStart()
    {
        yield return new WaitForSeconds(2);
        //Debug.Log($"OnStart{unit}");
        unit.OnUnitDespawned += Ondestroy;
    }
    public void Ondestroy()
    {
        if(gameObject != null)
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        unit.OnUnitDespawned -= Ondestroy;
    }
    /// <summary>
    /// Add one Sp Cost
    /// </summary>
    /// <returns></returns>
    public IEnumerator AddSpCost()
    {
        yield return new WaitForSeconds(0);
        if(spCost < childSprite.Count*2)
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
                if (spCost == 0) { break; }
                spCost--;
            }
            
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
                    Timer = waitTime;
                    StartCoroutine(AddSpCost());
            }
        }
        if(SpPrefab != null)
        {
            ISpecialAttack iSpecialAttack = SpPrefab.GetComponent(typeof(ISpecialAttack)) as ISpecialAttack;
            //Debug.Log($"button : {SpPrefab} iSpecialAttack.GetSpCost :{iSpecialAttack.GetSpCost()} spCost: {spCost}");
            if (iSpecialAttack.GetSpCost() <= spCost / 3)
            {
                particleSystem1.gameObject.SetActive(true);
                particleSystem1.Play();
                //particleSystem2.gameObject.SetActive(true);
                //particleSystem2.Play();
            }
            else
            {
                particleSystem1.gameObject.SetActive(false);
               // particleSystem2.gameObject.SetActive(false);
            }
        } 
    }
    public void SetUnit(Unit unit)
    {
        this.unit = unit;
    }
    public void SetSpPrefab(GameObject SpPrefab)
    {
        this.SpPrefab = SpPrefab;
    }
}
