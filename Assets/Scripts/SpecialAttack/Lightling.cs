using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Lightling : MonoBehaviour
{

    [SerializeField] private LayerMask layerMask = new LayerMask();
    [SerializeField] private GameObject attackPoint;
    [SerializeField] private GameObject startPoint;
    [SerializeField] private GameObject endPoint;

    public int attackRange = 100;
    public int minAttackRange;
    public float SPCost = 10;

    private Button SPButton;

    private bool IsSuperAttack = false;

    private SpCost spCost;
    private GameObject hitCollider;
    private Transform MyKing;
    private RTSPlayer player;
    private TacticalBehavior TB;
    private List<GameObject> targetList = new List<GameObject>();
    private List<float> distanceList = new List<float>();
    private bool SpawnedButton;

    // Start is called before the first frame update
    void Start()
    {
        startPoint = GameObject.FindGameObjectWithTag("LightlingStartPoint");
        endPoint = GameObject.FindGameObjectWithTag("LightlingEndPoint");
        SpawnedButton = FindObjectOfType<SpButton>().InstantiateSpButton(SpecialAttackDict.SpecialAttackType.Lightling, GetComponent<Unit>());
        if (SpawnedButton) { SPButton = FindObjectOfType<SpButton>().GetButton(GetComponent<Unit>().SpBtnTicket).GetComponent<Button>(); }
        if (SPButton == null) { return; }
        SPButton.onClick.RemoveAllListeners();
        SPButton.onClick.AddListener(OnpointerDown);
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        spCost = FindObjectOfType<SpCost>();
        //searchPoint = attackPoint.transform;
        minAttackRange = (int)(transform.localScale.x * attackRange / 2);
        TB = GameObject.FindGameObjectWithTag("TacticalSystem").GetComponent<TacticalBehavior>();
    }

    public void OnpointerDown()
    {
        //Debug.Log($"FindAttackTargetInDistance");
        
        //if(SPAmount < SPCost) {return;}
        spCost.SPAmount -= (int)SPCost;

        GameObject closestTarget = null;
        //bool haveTarget = true;
        var distance = float.MaxValue;
        var localDistance = 0f;
        distanceList.Clear();
        targetList.Clear();
        MyKing = GameObject.FindGameObjectWithTag("King0").transform;
            //bool findedTarget = false;
            //Search target in a distance
            Collider[] hitColliders = Physics.OverlapBox(transform.position, transform.localScale * attackRange, Quaternion.identity, layerMask);
            int i = 0;
            while (i < hitColliders.Length)
            {
                distance = float.MaxValue;
                hitCollider = hitColliders[i++].transform.gameObject;
                // check If the target is cloestest to king && it is not in the same team && check if it already finded the target
                if ((localDistance = (hitCollider.transform.position - transform.position).sqrMagnitude) < distance && !targetList.Contains(hitCollider))
                {
                    int id = ((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1 ? 1 : player.GetPlayerID() == 0 ? 1 : 0;
                    if (hitCollider.CompareTag("Player" + id) || hitCollider.CompareTag("King" + id))
                    {
                        if (localDistance > minAttackRange)
                        {
                        startPoint.transform.position = MyKing.position;
                        endPoint.transform.position = hitCollider.transform.position;
                        hitCollider.GetComponent<Health>().DealDamage(100);
                        //Debug.Log($"change lightling point{startPoint.transform.position},  {endPoint.transform.position}");
                    }
                    }
                }
            }
        

    }
    public void AttackTarget(float distance, Transform closestTarget)
    {
        // wait three secs the attack
        //float Timer = 3f;
        //while (Timer > 0) { Timer -= Time.deltaTime; }
        // damage base on distance
        GetComponent<UnitWeapon>().ScaleDamageDeal(0, 0, distance / 100);
        GameObject.FindGameObjectWithTag("King" + player.GetPlayerID()).transform.position = closestTarget.transform.position;
        // make the king attack in update
        IsSuperAttack = true;
    }
    void Update()
    {
        
    }
}
