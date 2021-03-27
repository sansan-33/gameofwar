using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KingSP : MonoBehaviour
{
     private TacticalBehavior TB;
    [SerializeField] private LayerMask layerMask = new LayerMask();
    [SerializeField] private GameObject attackPoint;
    public int minAttackRange;
    private Image SPImage;
    private TMP_Text SPText;
    public float SPCost =10;
    private Button SPButton;
    public int SPAmount = 0;
    private bool IsSuperAttack = false;
    GameObject hitCollider;
    RTSPlayer player;
    Transform searchPoint;
    List<GameObject> targetList = new List<GameObject>();
    List<float> distanceList = new List<float>();
    public int attackRange = 100;
    // Start is called before the first frame update
    void Start()
    {
        searchPoint = attackPoint.transform;
        minAttackRange = (int)(transform.localScale.x * attackRange / 2);
        SPImage = GameObject.FindGameObjectWithTag("SP Bar").GetComponent<Image>();
        SPText = GameObject.FindGameObjectWithTag("SP Text").GetComponent<TextMeshProUGUI>();
        SPButton = GameObject.FindGameObjectWithTag("SPButton").GetComponent<Button>();
        SPButton.onClick.RemoveAllListeners();
        SPButton.onClick.AddListener(FindAttackTargetInDistance);
        TB = GameObject.FindGameObjectWithTag("TacticalSystem").GetComponent<TacticalBehavior>();
    }
   
    public void FindAttackTargetInDistance()
    {
        if(attackPoint == null) { return; }
        //if(SPAmount < SPCost) {return;}
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        SPAmount -= (int)SPCost;

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
                if ((localDistance = (hitCollider.transform.position - transform.position).sqrMagnitude) < distance && hitCollider.tag != "Player0" && hitCollider.tag != this.tag&& !targetList.Contains(hitCollider))
                { 
                    //if (localDistance > minAttackRange)
                    //{
                    findedTarget = true;
                    distance = localDistance;
                    closestTarget = hitCollider;
                    //StopTacticalBehavior while using Special Attack
                    TB.StopTacticalBehavior(player.GetPlayerID(), GetComponent<Unit>().unitType);
                    // Move the searchPoint to the next target, so it will not search at the same point
                    searchPoint = closestTarget.transform;    
                   // }
                } 
            }
            // if there is no more target is finded then break
           if(findedTarget == false)
            {
                break;
            }
            distanceList.Add(distance);
            targetList.Add(closestTarget);
        }
        searchPoint = attackPoint.transform;
        // if it doesnot find any target return
        if (closestTarget == null) {  return; }
        foreach (GameObject target in targetList)
        {
            foreach(float distances in distanceList)
            {
                AttackTarget(distances, target.transform);
            }
            
        }
        GetComponent<UnitWeapon>().ReScaleDamageDeal();
    }
    public void AttackTarget(float distance, Transform closestTarget)
    {
        // wait three secs the attack
        float Timer = 3f;
        while (Timer > 0) { Timer -= Time.deltaTime; }
        // damage base on distance
        GetComponent<UnitWeapon>().ScaleDamageDeal(distance / 100);
        transform.position = closestTarget.transform.position;
        // make the king attack in update
        IsSuperAttack = true;
    }
    public  void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
        if (true)
        {
            //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
            Gizmos.DrawWireCube(attackPoint.transform.position, transform.localScale * attackRange);
        }
    }
    public void UpdateSPAmount()
    {
        SPAmount++;
        SPText.text = (string)SPAmount.ToString();
        SPImage.fillAmount = (float)SPAmount / SPCost;
        
    }
    // Update is called once per frame
    void Update()
    {
        if (IsSuperAttack)
        {
            GetComponent<UnitWeapon>().TryAttack();
        }
    }
}
