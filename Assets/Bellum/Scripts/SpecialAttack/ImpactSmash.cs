using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ImpactSmash : MonoBehaviour,ISpecialAttack, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private LayerMask floorMask = new LayerMask();
    [SerializeField] Material freezeMaterial;
    [SerializeField] int damage = 10;
    [SerializeField] GameObject dragcCirclePrefab;
    private GameObject dragCircle;
    private RTSPlayer RTSplayer;
    private GameObject impectType;
    private PlayerGround playerGround;
    private SpecialAttackDict.SpecialAttackType SpecialAttackType;
    private SpecialAttackManager specialAttackManager;
    private int key = 0;
    private GreatWallController wallController;
    private CinemachineManager cmManager;
    private float i= 3;
    // Start is called before the first frame update
    void Start()
    {
        specialAttackManager = GameObject.FindGameObjectWithTag("SpecialAttackManager").GetComponent<SpecialAttackManager>();
        RTSplayer = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
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
    public void SetImpectType(GameObject prefab)
    {
        impectType = prefab;
    }
    public void SetSpecialAttackType(SpecialAttackDict.SpecialAttackType type)
    {
        SpecialAttackType = type;
    }
    public SpecialAttackDict.SpecialAttackType GetSpecialAttackType()
    {
        return SpecialAttackType;
    }
    public void OnPointerDown()
    {
   
    }
    public int GetSpCost()
    {
        return 0;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        playerGround.SetALlLayer();
        dragCircle = Instantiate(dragcCirclePrefab);
        if(SpecialAttackDict.RangeScale.TryGetValue(SpecialAttackType,out float scale))
        {
            dragCircle.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(pos);
        //if the floor layer is not floor it will not work!!!

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) { return; }
        dragCircle.transform.position = new Vector3(hit.point.x, 2, hit.point.z);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        Vector3 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(pos);
        //if the floor layer is not floor it will not work!!!
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) {return; }
        //Debug.Log($"{new Vector3(hit.point.x, 0, hit.point.z) }{SpecialAttackType.ToString()}");
        Debug.Log($"attack type {SpecialAttackType}");
        if(SpecialAttackType != SpecialAttackDict.SpecialAttackType.LIGHTNING)
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
        Destroy(dragCircle);
        
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
        if (king != null)
            army.Add(king);
        //Debug.Log($"1 {armies.Count}");
        sneakUnit.CopyTo(armies);
        //Debug.Log($"2 {armies.Count}");
        float range = 11;
        float scale = 0.5f;
        while(dragCircle.transform.localScale.x > scale)
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
                   
                    if (SpecialAttackType == SpecialAttackDict.SpecialAttackType.ZAP)
                    {
                        StartCoroutine(AwakeUnit(unit, 1, unit.GetComponent<CardStats>().speed, unit.GetComponent<CardStats>().repeatAttackDelay, unit.GetComponentInChildren<SkinnedMeshRenderer>().material));
                        //unit.GetComponent<AstarAI>().IS_STUNNED = true;
                        unit.GetComponent<UnitPowerUp>().SpecialEffect(float.MaxValue, unit.GetComponent<CardStats>().repeatAttackDelay);

                    }
                    if (SpecialAttackType == SpecialAttackDict.SpecialAttackType.FREEZE)
                    {
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
                    }
                    if (SpecialAttackType == SpecialAttackDict.SpecialAttackType.STUN)
                    {
                        StartCoroutine(AwakeUnit(unit, 3, unit.GetComponent<CardStats>().speed, unit.GetComponent<CardStats>().repeatAttackDelay, unit.GetComponentInChildren<SkinnedMeshRenderer>().material));
                        unit.GetComponent<UnitPowerUp>().SpecialEffect(float.MaxValue, 0);
                       // unit.GetComponentInChildren<SkinnedMeshRenderer>().material = freezeMaterial;
                    }                
            }
        }
        if (SpecialAttackType == SpecialAttackDict.SpecialAttackType.LIGHTNING)
        {
            foreach (GameObject unit in topThreeHealthUnit)
            {
                Debug.Log($"deal damge to {unit.name}");
                unit.GetComponent<Health>().DealDamage(damage);
                specialAttackManager.SpawnPrefab(new Vector3(unit.transform.position.x, 0, unit.transform.position.z), SpecialAttackType.ToString());
            }
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
