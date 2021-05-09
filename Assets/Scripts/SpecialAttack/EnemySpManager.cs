using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class EnemySpManager : MonoBehaviour
{
    public int spCost;
    private float Timer = 1;
    private int waitTime = 10;
    public bool useTimer = false;
    private RTSPlayer player;
    private List<GameObject> enemySp = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)//1 player mode
        {
            StartCoroutine(GetUnit());
        }
    }
    private IEnumerator GetUnit()
    {
        
        yield return new WaitForSeconds(3);
        if(transform.parent.CompareTag("Player1") || transform.parent.CompareTag("King1"))
        {
           
            this.enemySp = SpButtonManager.enemySp;
            foreach (Button button in SpButtonManager.buttons)
            {
               
                useTimer = button.GetComponent<SpCostDisplay>().useTimer;
                waitTime = button.GetComponent<SpCostDisplay>().waitTime;
                break;
            }
        } 
    }
    public void ChangeSPCost(int amount)
    {
       
        spCost += amount;
        //Debug.Log($"ChangeSPCost {spCost}");
    }
    // Update is called once per frame
    void Update()
    {
        if(enemySp != null)
        {
            //Debug.Log(1);
            foreach(GameObject SpPrefab in enemySp)
            {
                
                ISpecialAttack iSpecialAttack = SpPrefab.GetComponent(typeof(ISpecialAttack)) as ISpecialAttack;
                //Debug.Log($"iSpecialAttack.GetSpCost(){iSpecialAttack.GetSpCost()} < {spCost}");
                if (iSpecialAttack.GetSpCost() <= spCost)
                {
                    //Debug.Log($"OnPointerDown {transform.parent.tag}");
                    iSpecialAttack.OnPointerDown();
                }
            }
        }
        if (useTimer == true)
        {
            if (Timer > 0)
            {
                Timer -= Time.deltaTime;
                //Debug.Log(Timer);
            }
            else
            {
                Timer = waitTime;
                //Debug.Log("Add SP'");
                ChangeSPCost(1);
            }
        }
    }
}
