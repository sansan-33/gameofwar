using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ImpactSmash : MonoBehaviour,ISpecialAttack, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private LayerMask floorMask = new LayerMask();
    [SerializeField] Material freezeMaterial;
    [SerializeField] int damage = 10;
    [SerializeField] GameObject dragcCirclePrefab;
    [SerializeField] private bool needMinusSP = false;
    private GameObject dragCircle;
    private RTSPlayer RTSplayer;
    private GameObject impectType;
    private PlayerGround playerGround;
    private SpecialAttackDict.SpecialAttackType SpecialAttackType;
    private SpecialAttackManager specialAttackManager;
    private int key = 0;
    private GreatWallController wallController;
    private CinemachineManager cmManager;
    private Unit parentUnit;
    private int cost = 3;
    private float progressUnitVelocity;
    private CardPlayer cardPlayer;
    // Start is called before the first frame update
    void Start()
    {
        cardPlayer = GameObject.FindGameObjectWithTag("PlayerDeck").GetComponent<CardPlayer>();
        specialAttackManager = GameObject.FindGameObjectWithTag("SpecialAttackManager").GetComponent<SpecialAttackManager>();
        RTSplayer = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        GetPlayerGroung();
    }
    public void SetUnit(Unit unit)
    {
        parentUnit = unit;
    }
    public void SetImpectType(GameObject prefab)
    {
        impectType = prefab;
    }
    public void SetSpecialAttackType(SpecialAttackDict.SpecialAttackType type)
    {
        SpecialAttackType = type;
        if (SpecialAttackDict.SpecialAttackCost.TryGetValue(SpecialAttackType, out int cost))
        {
            this.cost = cost;
        }
    }
    public SpecialAttackDict.SpecialAttackType GetSpecialAttackType()
    {
        return SpecialAttackType;
    }
    private void GetPlayerGroung()
    {
        GameObject[] grounds = GameObject.FindGameObjectsWithTag("FightGround");
        foreach (GameObject ground in grounds)
        {
            if (ground.TryGetComponent(out PlayerGround pg))
            {
                playerGround = pg;
                break;
            }
        }
    }
    public void OnPointerDown()
    {
        switch (SpecialAttackType)
        {
            case SpecialAttackDict.SpecialAttackType.REMOVEGAUGE:
                List<Button> buttons = SpButtonManager.buttons;
                foreach (Button button in buttons)
                {
                    if (button.GetComponent<SpCostDisplay>().GetUnit().CompareTag("Player" + RTSplayer.GetEnemyID()) || button.GetComponent<SpCostDisplay>().GetUnit().CompareTag("King" + RTSplayer.GetEnemyID()))
                    {
                        StartCoroutine(button.GetComponent<SpCostDisplay>().MinusSpCost(10));
                        specialAttackManager.SpawnPrefab(button.GetComponent<SpCostDisplay>().GetUnit().transform.position, SpecialAttackType.ToString());
                    }
                }
                break;
            case SpecialAttackDict.SpecialAttackType.CARDRANKUP:
                Debug.Log("rank up");
                foreach(Card card in cardPlayer.GetCards())
                {
                    card.RankUp(cardPlayer.unitSkillArt, cardPlayer.MAXCARDSTAR);
                }
                break;
        }
    }
    public int GetSpCost()
    {
        return 1;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if ((GetComponentInParent<SpCostDisplay>().spCost / 3) < cost && needMinusSP == true) { return; }
        if(playerGround == null) { GetPlayerGroung(); }
        playerGround.SetALlLayer();
        dragCircle = Instantiate(dragcCirclePrefab);
        if(SpecialAttackDict.RangeScale.TryGetValue(SpecialAttackType,out float scale))
        {
            dragCircle.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if ((GetComponentInParent<SpCostDisplay>().spCost / 3) < cost && needMinusSP == true) { return; }
        if (SpecialAttackType == SpecialAttackDict.SpecialAttackType.REMOVEGAUGE){ return; }
        Vector3 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(pos);
        //if the floor layer is not floor it will not work!!!

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) { return; }
        dragCircle.transform.position = new Vector3(hit.point.x, 2, hit.point.z);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        
        if ((GetComponentInParent<SpCostDisplay>().spCost / 3) < cost && needMinusSP == true) { return; }
        StartCoroutine(GetComponentInParent<SpCostDisplay>().MinusSpCost(cost));
        if (SpecialAttackType == SpecialAttackDict.SpecialAttackType.REMOVEGAUGE) { return; }
        Destroy(dragCircle);
        Vector3 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(pos);
        //if the floor layer is not floor it will not work!!!
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) {return; }
        Debug.Log($"{new Vector3(hit.point.x, 0, hit.point.z) }{SpecialAttackType.ToString()}");
        Debug.Log($"attack type {SpecialAttackType}");
        if(SpecialAttackType != SpecialAttackDict.SpecialAttackType.LIGHTNING && SpecialAttackType != SpecialAttackDict.SpecialAttackType.GRAB)
        {
            specialAttackManager.SpawnPrefab(new Vector3(hit.point.x, 0, hit.point.z), SpecialAttackType.ToString());
        }
       
       

        if (SpecialAttackType == SpecialAttackDict.SpecialAttackType.METEOR)
        {
            if (wallController == null)
            {
                wallController = GameObject.FindGameObjectWithTag("GreatWallController").GetComponent<GreatWallController>();
            }
            
            wallController.dynamicBlock(false);
            //Debug.Log($"GreatWallController move to {hit.point.z}");
            Vector3 wallPos = wallController.transform.position;
            wallPos.z =  hit.point.z;
            wallController.transform.position = wallPos;
            wallController.dynamicBlock(true);
        }
        if(SpecialAttackDict.NeedCameraShake.TryGetValue(SpecialAttackType,out bool needCameraShake))
        {
            if(needCameraShake == true)
            {
                if (SpecialAttackType == SpecialAttackDict.SpecialAttackType.METEOR)
                {
                    Invoke("ShakeCam", 1.5f);
                }
                else
                {
                    ShakeCam();
                }
            }
        }
        playerGround.resetLayer();
        DealDamage();
        
    }
    private void DealDamage()
    {
        GameObject[] units = GameObject.FindGameObjectsWithTag("Player" + 1);
        GameObject[] provokeUnit = GameObject.FindGameObjectsWithTag("Provoke" + 1);
        GameObject[] sneakUnit = GameObject.FindGameObjectsWithTag("Sneaky" + 1);
        GameObject king = GameObject.FindGameObjectWithTag("King" + 1);
        List<GameObject> armies = new List<GameObject>();
        List<GameObject> army = new List<GameObject>();
        List<GameObject> topThreeHealthUnit = new List<GameObject>();
        armies = units.ToList();

        provokeUnit.CopyTo(armies);
        if (king != null) { armies.Add(king); }
           
        //Debug.Log($"1 {armies.Count}");
        sneakUnit.CopyTo(armies);
        //Debug.Log($"2 {armies.Count}");
        float range = 11;
        float scale = 0.5f;

        //Grab
        float distance = 10000;
        GameObject closestUnit  = null;

        while (dragCircle.transform.localScale.x > scale)
        {
            range *= 2;
            scale += 0.5f;
        }
        foreach (GameObject unit in armies)
        {//
            //Debug.Log($"finded {unit} circle pos {dragCircle.transform.position} - pos {unit.transform.position} = sqrMagnitude {(dragCircle.transform.position - unit.transform.position).sqrMagnitude} range = {range}");
            if ((dragCircle.transform.position - unit.transform.position).sqrMagnitude < range)
            {
                if (SpecialAttackType == SpecialAttackDict.SpecialAttackType.LIGHTNING)
                {
                    if (topThreeHealthUnit.Count < 3)
                    {
                        //Debug.Log("ADD unit" + unit);
                        topThreeHealthUnit.Add(unit);
                    }
                    else
                    {
                        
                        foreach (GameObject obj in topThreeHealthUnit.ToArray())
                        {
                           // Debug.Log($"{obj.name}:{obj.GetComponent<Health>().getMaxHealth()} < {unit.GetComponent<Health>().getMaxHealth()}");
                            if (obj.GetComponent<Health>().getMaxHealth() < unit.GetComponent<Health>().getMaxHealth())
                            {
                                topThreeHealthUnit.Remove(obj);
                                topThreeHealthUnit.Add(unit);
                            }
                        }
                    }
                }
                else
                {
                    unit.GetComponent<Health>().DealDamage(damage);
                }

                switch (SpecialAttackType)
                {
                    case SpecialAttackDict.SpecialAttackType.ZAP:
                        StartCoroutine(AwakeUnit(unit, 1, unit.GetComponent<CardStats>().speed, unit.GetComponent<CardStats>().repeatAttackDelay, unit.GetComponentInChildren<SkinnedMeshRenderer>().material));
                        //unit.GetComponent<AstarAI>().IS_STUNNED = true;
                        unit.GetComponent<UnitPowerUp>().SpecialEffect(float.MaxValue, unit.GetComponent<CardStats>().repeatAttackDelay);
                        break;
                    case SpecialAttackDict.SpecialAttackType.FREEZE:

                        key++;
                        if (UnitSpeedkeys.ContainsKey(unit.GetComponent<Health>().freezeKey))
                        {
                            UnitRepeatAttackDelaykeys.Remove(unit.GetComponent<Health>().freezeKey);
                            UnitSpeedkeys.Remove(unit.GetComponent<Health>().freezeKey);
                            UnitMaterial.Remove(unit.GetComponent<Health>().freezeKey);
                        }
                        unit.GetComponent<Health>().freezeKey = key;
                        unit.GetComponent<Health>().IsFrezze = true;
                        UnitSpeedkeys.Add(key, unit.GetComponent<CardStats>().speed);
                        UnitMaterial.Add(key, unit.GetComponentInChildren<SkinnedMeshRenderer>().material);
                        UnitRepeatAttackDelaykeys.Add(key, unit.GetComponent<CardStats>().repeatAttackDelay);
                        StartCoroutine(AwakeUnit(unit, 5000, unit.GetComponent<CardStats>().speed, unit.GetComponent<CardStats>().repeatAttackDelay, unit.GetComponentInChildren<SkinnedMeshRenderer>().material));
                        unit.GetComponent<UnitPowerUp>().SpecialEffect(float.MaxValue, 0);
                        unit.GetComponentInChildren<SkinnedMeshRenderer>().material = freezeMaterial;
                        break;
                    case SpecialAttackDict.SpecialAttackType.STUN:
                        StartCoroutine(AwakeUnit(unit, 3, unit.GetComponent<CardStats>().speed, unit.GetComponent<CardStats>().repeatAttackDelay, unit.GetComponentInChildren<SkinnedMeshRenderer>().material));
                        unit.GetComponent<UnitPowerUp>().SpecialEffect(float.MaxValue, 0);
                        break;
                    case SpecialAttackDict.SpecialAttackType.GRAB:
                        //Debug.Log($"parentUnit.transform.position {parentUnit.transform.position} - unit pos {unit.transform.position} = {(parentUnit.transform.position - unit.transform.position).sqrMagnitude} < distance {distance}");
                        if ((parentUnit.transform.position - unit.transform.position).sqrMagnitude < distance)
                        {
                            //Debug.Log($"change target to {unit}");
                            distance = (parentUnit.transform.position - unit.transform.position).sqrMagnitude;
                            closestUnit = unit;
                        }
                        break;
                }             
            }
        }
        if (SpecialAttackType == SpecialAttackDict.SpecialAttackType.LIGHTNING)
        {
            foreach (GameObject unit in topThreeHealthUnit)
            {
                //Debug.Log($"deal damge to {unit.name}");
                unit.GetComponent<Health>().DealDamage(damage);
                specialAttackManager.SpawnPrefab(new Vector3(unit.transform.position.x, 0, unit.transform.position.z), SpecialAttackType.ToString());
            }
        }
        if(SpecialAttackType == SpecialAttackDict.SpecialAttackType.GRAB)
        {
            //StartCoroutine(MoveUnitZ(closestUnit.transform));
            //StartCoroutine(MoveUnit(closestUnit.transform));
            specialAttackManager.SpawnGrabPrefab(closestUnit.transform.position, parentUnit.transform.position, SpecialAttackType.ToString(), closestUnit.gameObject);
        }

    }
    private IEnumerator MoveUnit(Transform closestUnit)
    {
        
        float timer = 5;
        while (closestUnit.position.x != parentUnit.transform.position.x || closestUnit.position.z != parentUnit.transform.position.z)
        {
            //Debug.Log("move unit");
            timer -= Time.deltaTime;
            if (closestUnit != null)
            {
               // Debug.Log($" from {closestUnit.position} x move to {parentUnit.transform.position}");
                float targetX = parentUnit.transform.position.x;
                Vector3 zero = Vector3.zero;
                closestUnit.position = Vector3.SmoothDamp(closestUnit.position, parentUnit.transform.position, ref zero, 0.3f);
                //closestUnit.position = new Vector3(x, closestUnit.transform.position.y, closestUnit.transform.position.z);

            }
            yield return new WaitForSeconds(0.01f);
            if (timer <= 0) { Debug.Log("Time break"); break; }
        }
    }
    private void ShakeCam()
    {
        cmManager = GameObject.FindGameObjectWithTag("CinemachineManager").GetComponent<CinemachineManager>();
        cmManager.shake();
    }
    private IEnumerator DestroyGameObjectAfterSec(GameObject unit, float sec)
    {
        yield return new WaitForSeconds(sec);
        Destroy(gameObject);

    }
    private IEnumerator AwakeUnit(GameObject unit, float sec,float speed ,float repeatAttackDelay,Material material)
    {
        yield return new WaitForSeconds(sec);
        if(speed != 0 && unit != null)
        {
            unit.GetComponent<UnitPowerUp>().SpecialEffect(speed, repeatAttackDelay);
            unit.GetComponentInChildren<SkinnedMeshRenderer>().material = material;
        }
        

    }
    public Dictionary<int, float> UnitRepeatAttackDelaykeys = new Dictionary<int, float>()
    {

    };
    public Dictionary<int, float> UnitSpeedkeys = new Dictionary<int, float>()
    {

    };
    public Dictionary<int, Material> UnitMaterial = new Dictionary<int, Material>()
    {

    };
    // Update is called once per frame

}
