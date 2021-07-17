using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ImpectSmash : MonoBehaviour,ISpecialAttack, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] int damage = 10;
    [SerializeField] GameObject dragcCirclePrefab;
    private PlayerGround playerGround;
    private GameObject dragCircle;
    private RTSPlayer RTSplayer;
    private GameObject impectType;
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
        dragCircle = Instantiate(dragcCirclePrefab);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(pos);
        //if the floor layer is not floor it will not work!!!
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) { return; }
        dragCircle.transform.position = new Vector3(hit.point.x, 0, hit.point.z);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        Vector3 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(pos);
        //if the floor layer is not floor it will not work!!!
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) { return; }
        GameObject impect = Instantiate(impectType);
        
        if (SpecialAttackType == SpecialAttackDict.SpecialAttackType.TORNADO)
        {
            impect.GetComponent<Tornado>().SetPlayerType(RTSplayer.GetPlayerID());
            StartCoroutine(DestroyGameObjectAfterSec(impect, 5));
            impect.transform.position = hit.point;
        }
        else
        {
            impect.transform.position = new Vector3(hit.point.x, hit.point.y + 20, hit.point.z);
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
        DealDamage();
        Destroy(dragCircle);
        
    }
    private void DealDamage()
    {
        GameObject[] units = GameObject.FindGameObjectsWithTag("Player" + 1);
        GameObject king = GameObject.FindGameObjectWithTag("King" + 1);
        List<GameObject> armies = new List<GameObject>();
        armies = units.ToList();
        if (king != null)
            armies.Add(king);
        float range = 11;
        float scale = 0.5f;
        while(dragCircle.transform.localScale.x > scale)
        {
            range *= 2;
            scale += 0.5f;
        }

        foreach (GameObject unit in armies)
        {
            Debug.Log($"finded {unit} circle pos {dragCircle.transform.position} - pos {unit.transform.position} = sqrMagnitude {(dragCircle.transform.position - unit.transform.position).sqrMagnitude} range = {range}");
            if ((dragCircle.transform.position - unit.transform.position).sqrMagnitude < range)
            {
                unit.GetComponent<Health>().DealDamage(damage);
                if (SpecialAttackType == SpecialAttackDict.SpecialAttackType.ZAP)
                {
                    unit.GetComponent<AstarAI>().IS_STUNNED = true;
                    StartCoroutine(awakeUnit(unit.GetComponent<AstarAI>()));
                }
            }
        }
    }
    private IEnumerator DestroyGameObjectAfterSec(GameObject gameObject, int sec)
    {
        yield return new WaitForSeconds(sec);
        Destroy(gameObject);

    }
    private IEnumerator awakeUnit(AstarAI astarAI)
    {
        yield return new WaitForSeconds(1);
        astarAI.IS_STUNNED = false;

    }

    // Update is called once per frame
    void Update()
    {
        
       
    }
}
