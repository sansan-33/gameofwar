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
    private Image SPImage;
    private TMP_Text SPText;
    public float SPCost =10;
    private Button SPButton;
    public int SPAmount = 0;
    GameObject hitCollider;
    RTSPlayer player;
    float Timer = 3;
    public int attacklengh = 5;
    // Start is called before the first frame update
    void Start()
    {
        SPImage = GameObject.FindGameObjectWithTag("SP Bar").GetComponent<Image>();
        SPText = GameObject.FindGameObjectWithTag("SP Text").GetComponent<TextMeshProUGUI>();
        Debug.Log(SPText);
        SPButton = GameObject.FindGameObjectWithTag("SPButton").GetComponent<Button>();
        SPButton.onClick.RemoveAllListeners();
        SPButton.onClick.AddListener(FindAttackTargetInDistance);
       
        TB = GameObject.FindGameObjectWithTag("TacticalSystem").GetComponent<TacticalBehavior>();
    }
   
    public void FindAttackTargetInDistance()
    {
        if(attackPoint == null) { return; }
        //if(SPAmount != SPCost) { return; }
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        Collider[] hitColliders = Physics.OverlapBox(attackPoint.transform.position, transform.localScale * 15, Quaternion.identity, layerMask);
        var distance = float.MaxValue;
        var localDistance = 0f;
        int i = 0;
        GameObject closestTarget = null;
        while (i < hitColliders.Length)
        {
            hitCollider = hitColliders[i++].transform.gameObject;
            // Debug.Log($"i == {hitColliders[i++]},Length is {hitColliders.Length}");
            if ((localDistance = (hitCollider.transform.position - transform.position).sqrMagnitude) < distance&& hitCollider.tag!= "Player0"&& hitCollider.tag != "King0")  
            {
               
                    distance = localDistance;
                closestTarget = hitCollider;
                // hitCollider = hitColliders[i].transform.gameObject;
                Debug.Log(closestTarget);
                    TB.StopTacticalBehavior(player.GetPlayerID(), GetComponent<Unit>().unitType);
                   // targetTransform = hitColliders[i];
                   //targetDamagable = targets[i];
            }
            //i++;
           
            
        }
        if(closestTarget == null) { return; }
        Debug.Log($"attack {closestTarget}");
        GetComponent<UnitPowerUp>().cmdSpeedUp(20);
        GetComponent<UnitMovement>().CmdMove(closestTarget.transform.position);
        GetComponent<UnitWeapon>().IsKingSP = true;
        //Debug.Log(hitColliders.Length);
        //Debug.Log(player.GetPlayerID());

    }
    public  void OnDrawGizmos()
    {

      

        Gizmos.color = Color.white;
        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
        if (true)
        {
            //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
            Gizmos.DrawWireCube(attackPoint.transform.position, transform.localScale * 15);
        }
    }
    public void UpdateSPAmount()
    {
        SPAmount++;
        Debug.Log(SPText);
            SPText.text = (string)SPAmount.ToString();
            SPImage.fillAmount = (float)SPAmount / SPCost;
        
    }
    // Update is called once per frame
    void Update()
    {
        
        //while (Timer > 0) { Timer -= Time.deltaTime; }
       
        
    }
}
