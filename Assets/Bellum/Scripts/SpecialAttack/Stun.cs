using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Stun : NetworkBehaviour, ISpecialAttack
{
    [SerializeField] private GameObject camPrefab = null;
    private UnitWeapon UnitWeapon;
    private List<GameObject> enemyList;
    private Button SPButton;
    private RTSPlayer player;
    private SpCost spCost;
    private List<GameObject> effects;

    private float enemyReFightTimer = -10000;
    public int SPCost = 10;
    public int buttonTicket;
    private bool SpawnedButton;
    private bool CanUnFrezze = false;
    private bool CMVirtualIsOn = false;
    public int enemyFrezzeTime = 5;//second

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("Stun.cs Start()");
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        if (CompareTag("King" + player.GetEnemyID()) || CompareTag("Player" + player.GetEnemyID())) { return; }

        //Debug.Log("try to find spCost");
        spCost = FindObjectOfType<SpCost>();
        //Debug.Log($"spCost:{spCost}");

        //Instantiate SpButton and is it already spawned
        /* SpawnedButton = FindObjectOfType<SpButton>().InstantiateSpButton(SpecialAttackDict.SpecialAttackType.Stun, GetComponent<Unit>());
         //SpButton will give unit a int to get back the button that it spwaned
         if (SpawnedButton) { SPButton = FindObjectOfType<SpButton>().GetButton(GetComponent<Unit>().SpBtnTicket).GetComponent<Button>(); }
         if (SPButton == null) { return; }
         SPButton.onClick.RemoveAllListeners();
         SPButton.onClick.AddListener(OnPointerDown);*/
    }

    public void OnPointerDown()
    {

        // start() spCost not pass to here, try to find here again
        //Debug.Log("OnPointerDown() try to find spCost");
        //spCost = FindObjectOfType<SpCost>();
        //Debug.Log($"OnPointerDown() spCost:{spCost}");
       // if (transform.parent == null) { return; }
       // Debug.Log($"parent : {transform.parent}");
        if(!SpButtonManager.unitBtn.TryGetValue(GetComponentInParent<Unit>().unitKey, out Button btn))
        {
            SpButtonManager.enemyUnitBtns.TryGetValue(GetComponentInParent<Unit>().unitKey, out var _btn);
            btn = _btn;
        }


            if (spCost.useSpCost == true)
            {
            //if (spCost.SPAmount < SPCost) { return; }
            
                if ((btn.GetComponent<SpCostDisplay>().spCost / 3) < SPCost) { return; }
                StartCoroutine(btn.GetComponent<SpCostDisplay>().MinusSpCost(SPCost));
                spCost.UpdateSPAmount(-SPCost, null);
            }
        
        //Debug.Log($"b4 OnPointerDown ==> StartCoroutine {btn.tag} {btn.name} ");

        UnitRepeatAttackDelaykeys.Clear();
        UnitSpeedkeys.Clear();
        //find all enemy unit
        int id = transform.parent.CompareTag("Player0") || transform.parent.CompareTag("King0") ? player.GetEnemyID() : player.GetPlayerID();
        enemyList = GameObject.FindGameObjectsWithTag("Player" + id).ToList();

        var a = GameObject.FindGameObjectsWithTag("King" + id);
        if (a.Length > 0)
            enemyList.AddRange(a);
       
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)//1 player mode
        {
            GetComponentInParent<UnitWeapon>().CMVirtual();
            //stop enenmy
            foreach (GameObject unit in enemyList)
            {
                //Debug.Log($"name : {unit.name} tag : {unit.tag}");
                enemyReFightTimer = enemyFrezzeTime;
                CanUnFrezze = true;
                CardStats cardStats = GetComponentInParent<Unit>().GetComponent<CardStats>();
                UnitRepeatAttackDelaykeys.Add(unit, cardStats.repeatAttackDelay);
                UnitSpeedkeys.Add(unit, cardStats.speed);
                unit.GetComponent<UnitPowerUp>().SpecialEffect(float.MaxValue, 0);

            }
            FindObjectOfType<SpawnSpEffect>().CmdSpawnEffect(1, null);
            effects = FindObjectOfType<SpawnSpEffect>().GetEffect(1);
        }
        else // Multi player seneriao
        {
            //stop enenmy
            foreach (GameObject unit in enemyList)
            {
                enemyReFightTimer = enemyFrezzeTime;
                CanUnFrezze = true;
                CardStats cardStats = unit.GetComponent<CardStats>();
                UnitRepeatAttackDelaykeys.Add(unit, cardStats.repeatAttackDelay);
                UnitSpeedkeys.Add(unit, cardStats.speed);
                Debug.Log($"cardStats.star :{cardStats.star}");
                unit.GetComponent<UnitPowerUp>().SpecialEffect(float.MaxValue, 0);

            }
            FindObjectOfType<SpawnSpEffect>().CmdSpawnEffect(1, null);
            effects = FindObjectOfType<SpawnSpEffect>().GetEffect(1);
            GetComponentInParent<UnitWeapon>().CMVirtual();
        }
    }
    private void CMVIRTUAL()
    {
        CmdCMVirtual();
    }
    [Command(ignoreAuthority = true)]
    public void CmdCMVirtual()
    {
        //Debug.Log("CmdCMVirtual");
        if (GameObject.Find("camVirtual") == null)
        {
            //Debug.Log($" Spawn  camVirtual {GameObject.Find("camVirtual")}");
            //GameObject cam = Instantiate(camPrefab, new Vector2(0,300), Quaternion.Euler(new Vector3(90, 0, 0)));
            GameObject cam = Instantiate(camPrefab, new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(0, 0, 0)));
            //cam.GetComponent<CinemachineShake>().shakeTime = enemyReFightTimer;
            cam.GetComponent<CinemachineShake>().ShakeCamera(enemyReFightTimer);
            NetworkServer.Spawn(cam, connectionToClient);
        }
    }
    public int GetSpCost()
    {
        return SPCost;
    }
    // Update is called once per frame
    void Update()
    {
        if(enemyReFightTimer > 0)
        {
            enemyReFightTimer -= Time.deltaTime;
        }
        else if(CanUnFrezze == true)
        {
            foreach (GameObject unit in enemyList)
            {
                CardStats cardStats = unit.GetComponent<CardStats>();
                UnitRepeatAttackDelaykeys.TryGetValue(unit, out float repeatAttackDelay);
                UnitSpeedkeys.TryGetValue(unit, out int speed);
                unit.GetComponent<UnitPowerUp>().SpecialEffect(repeatAttackDelay, speed);  
            }
            FindObjectOfType<SpawnSpEffect>().destroyEffect(1);
            CanUnFrezze = false;
        }    
    }
    public static Dictionary<GameObject, float> UnitRepeatAttackDelaykeys = new Dictionary<GameObject, float>()
    {

    };
    public static Dictionary<GameObject, int> UnitSpeedkeys = new Dictionary<GameObject, int>()
    {

    };
}
