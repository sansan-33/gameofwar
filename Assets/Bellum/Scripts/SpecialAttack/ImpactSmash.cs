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
    // Start is called before the first frame update
    void Start()
    {
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
        GameObject impect = null ;
        if (impectType != null)
        {
            impect = Instantiate(impectType);
            impect.transform.position = new Vector3(hit.point.x, 0, hit.point.z);
        }
        
        if (SpecialAttackType == SpecialAttackDict.SpecialAttackType.TORNADO)
        {
           
            impect.GetComponent<Tornado>().SetPlayerType(RTSplayer.GetPlayerID());
            impect.GetComponent<Tornado>().OnStartServer();
            //StartCoroutine(DestroyGameObjectAfterSec(impect, 5.5f));
        }
        else
        {
            //impect.transform.position = new Vector3(hit.point.x, 0 + 20, hit.point.z);
        }
        if (SpecialAttackType == SpecialAttackDict.SpecialAttackType.METEOR)
        {
            GreatWallController wallController = GameObject.FindGameObjectWithTag("GreatWallController").GetComponent<GreatWallController>();
            
            wallController.transform.position = new Vector3(wallController.transform.position.x, wallController.transform.position.y, hit.point.z);
            wallController.dynamicBlock(true);
        }
        if(SpecialAttackDict.NeedCameraShake.TryGetValue(SpecialAttackType,out bool needCameraShake))
        {
            if(needCameraShake == true)
            {
                CinemachineManager cmManager = GameObject.FindGameObjectWithTag("CinemachineManager").GetComponent<CinemachineManager>();
                cmManager.shake();
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
            Debug.Log($"finded {unit} circle pos {dragCircle.transform.position} - pos {unit.transform.position} = sqrMagnitude {(dragCircle.transform.position - unit.transform.position).sqrMagnitude} range = {range}");
            if ((dragCircle.transform.position - unit.transform.position).sqrMagnitude < range)
            {
                unit.GetComponent<Health>().DealDamage(damage);
                if (SpecialAttackType == SpecialAttackDict.SpecialAttackType.ZAP)
                {
                    StartCoroutine(AwakeUnit(unit, 1, unit.GetComponent<CardStats>().speed, unit.GetComponent<CardStats>().repeatAttackDelay, unit.GetComponentInChildren<SkinnedMeshRenderer>().material));
                    //unit.GetComponent<AstarAI>().IS_STUNNED = true;
                    unit.GetComponent<UnitPowerUp>().SpecialEffect(float.MaxValue, unit.GetComponent<CardStats>().repeatAttackDelay);
                    
                }
                if(SpecialAttackType == SpecialAttackDict.SpecialAttackType.FREEZE)
                {
                    StartCoroutine(AwakeUnit(unit, 5, unit.GetComponent<CardStats>().speed, unit.GetComponent<CardStats>().repeatAttackDelay, unit.GetComponentInChildren<SkinnedMeshRenderer>().material));
                    unit.GetComponent<UnitPowerUp>().SpecialEffect(float.MaxValue, 0);
                    unit.GetComponentInChildren<SkinnedMeshRenderer>().material = freezeMaterial;
                    
                }
            }
        }
    }
    private IEnumerator DestroyGameObjectAfterSec(GameObject unit, float sec)
    {
        yield return new WaitForSeconds(sec);
        Destroy(gameObject);

    }
    private IEnumerator AwakeUnit(GameObject unit, float sec,float speed ,float repeatAttackDelay,Material material)
    {
        yield return new WaitForSeconds(sec);
        if(speed != 0)
        {
            unit.GetComponent<UnitPowerUp>().SpecialEffect(speed, repeatAttackDelay);
            unit.GetComponentInChildren<SkinnedMeshRenderer>().material = material;
        }
        

    }

    // Update is called once per frame
    void Update()
    {
        
       
    }
}
