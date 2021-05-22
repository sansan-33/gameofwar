using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoldenSlash : MonoBehaviour, ISpecialAttack
{
    [SerializeField] private LayerMask layerMask = new LayerMask();
    //[SerializeField] private GameObject attackPoint;
    
    public int attackRange = 100;
    public int minAttackRange;
    public int SPCost = 10;
    private int id;
    private Button SPButton;

    private bool IsSuperAttack = false;

    private SpCost spCost;
    private GameObject hitCollider;
    private Transform searchPoint;
    private RTSPlayer player;
    private TacticalBehavior TB;
    private List<GameObject> targetList = new List<GameObject>();
    private List<float> distanceList = new List<float>();
    private bool SpawnedButton;

    // Start is called before the first frame update
    void Start()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        //if (CompareTag("King" + player.GetEnemyID()) || CompareTag("Player" + player.GetEnemyID())) { return; }
        /*SpawnedButton = FindObjectOfType<SpButton>().InstantiateSpButton(SpecialAttackDict.SpecialAttackType.Slash, GetComponent<Unit>());
        if (SpawnedButton) { SPButton = FindObjectOfType<SpButton>().GetButton(GetComponent<Unit>().SpBtnTicket).GetComponent<Button>(); }
        if (SPButton == null) { return; }
        SPButton.onClick.RemoveAllListeners();
        SPButton.onClick.AddListener(FindAttackTargetInDistance);*/
        spCost = FindObjectOfType<SpCost>();
        
        minAttackRange = (int)(transform.localScale.x * attackRange / 2);
        TB = GameObject.FindGameObjectWithTag("TacticalSystem").GetComponent<TacticalBehavior>();
    }

    public void OnPointerDown()
    {
        //Debug.Log($"GoldenSlash FindAttackTargetInDistance");
        // if (attackPoint == null) { return; }
        if (transform.parent.CompareTag("Player1") || transform.parent.CompareTag("King1"))
        {
            SpButtonManager.enemyUnitObj.TryGetValue(GetComponentInParent<Unit>().unitKey, out GameObject obj);
            if (spCost.useSpCost == true)
            {
                if (obj.GetComponent<EnemySpManager>().spCost < SPCost) { return; }
                obj.GetComponent<EnemySpManager>().ChangeSPCost(-SPCost);
            }
        }
        else
        {
            SpButtonManager.unitBtn.TryGetValue(GetComponentInParent<Unit>().unitKey, out Button btn);
            if (spCost.useSpCost == true)
            {
                //if (spCost.SPAmount < SPCost) { return; }
                if ((btn.GetComponent<SpCostDisplay>().spCost / 3) < SPCost) { return; }
                StartCoroutine(btn.GetComponent<SpCostDisplay>().MinusSpCost(SPCost));
                spCost.UpdateSPAmount(-SPCost, null);
            }
        }

        searchPoint = transform.parent.transform;
        GameObject closestTarget = null;
        bool haveTarget = true;
        var distance = float.MaxValue;
        var localDistance = 0f;
        distanceList.Clear();
        targetList.Clear();
        while (haveTarget == true)
        {
            bool findedTarget = false;
            //Search target in a distance
            Collider[] hitColliders = Physics.OverlapBox(searchPoint.position, transform.localScale * attackRange, Quaternion.identity, layerMask);
            int i = 0;
            while (i < hitColliders.Length)
            {
                distance = float.MaxValue;
                hitCollider = hitColliders[i++].transform.gameObject;
               // check If the target is cloestest to king && it is not in the same team && check if it already finded the target
                if ((localDistance = (hitCollider.transform.position - transform.position).sqrMagnitude) < distance && !targetList.Contains(hitCollider))
                {
                    
                    if (transform.parent.CompareTag("Player1") || transform.parent.CompareTag("King1"))
                    {
                        //Debug.Log("Ice 0");
                        id = 0;
                    }
                    else
                    {
                        id = ((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1 ? 1 : player.GetPlayerID() == 0 ? 1 : 0;
                    }
                   
                    if (hitCollider.CompareTag("Player" + id) || hitCollider.CompareTag("King" + id))
                        {
                            if (localDistance > minAttackRange)
                            {
                                findedTarget = true;
                                distance = localDistance;
                                closestTarget = hitCollider;
                                //StopTacticalBehavior while using Special Attack
                                
                                // Move the searchPoint to the next target, so it will not search at the same point
                                searchPoint = closestTarget.transform;
                            }
                        }                   
                } 
            }
            
            if (transform.parent.CompareTag("Player1") || transform.parent.CompareTag("King1"))
            {
                id = 1;
            }
            else
            {
                id = player.GetPlayerID();
            }
            TB.StopTacticalBehavior(id, GetComponentInParent<Unit>().unitType);
            // if there is no more target is finded then break
            if (findedTarget == false)
            {
                break;
            }
            distanceList.Add(distance);
            targetList.Add(closestTarget);
        }
        //Debug.Log($"GoldenSlash exit while (haveTarget == true)");
        searchPoint = transform.parent.transform;
        // if it doesnot find any target return
        if (closestTarget == null) {  return; }
        for (int a = 0; a < targetList.ToArray().Length; a++)
        {
           StartCoroutine( AttackTarget(distanceList.ToArray()[a], targetList.ToArray()[a].transform));
        }
        GetComponentInParent<UnitWeapon>().ReScaleDamageDeal();

    }

    IEnumerator AttackTarget(float distance, Transform closestTarget)
    {
        // wait three secs the attack
        // float Timer = 3f;
        // while (Timer > 0) { Timer -= Time.deltaTime; }
        // damage base on distance
        GetComponentInParent<UnitWeapon>().ScaleDamageDeal(0,0,distance / 100);
        //Debug.Log(id);
        transform.parent.position = closestTarget.transform.position;
        yield return GetComponentInParent<UnitWeapon>().TryAttack();
        
    }
    public int GetSpCost()
    {
        return SPCost;
    }
    public  void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
        if (true)
        {
            //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
            Gizmos.DrawWireCube(transform.parent.transform.position, transform.localScale * attackRange);
        }
    }
    
    
}
