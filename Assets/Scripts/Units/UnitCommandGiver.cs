using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class UnitCommandGiver : MonoBehaviour
{
    [SerializeField] private UnitSelectionHandler unitSelectionHandler = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    private Camera mainCamera;
   
    private bool isClicked = false;
    private GameObject closest;
    private GameObject target;
    private string targetTag = "Enemy";
    private void Start()
    {
        targetTag = (FindObjectOfType<NetworkManager>().numPlayers ==1) ? "Enemy" : "Player";
        mainCamera = Camera.main;

        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
       
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Update()
    {

        isClicked = false;
        Vector3 pos = new Vector3() ;
       
        try
        {
            if (isDoubleTap())//|| (Mouse.current != null && Mouse.current.rightButton != null && Mouse.current.rightButton.wasPressedThisFrame ) )
            {
                pos = Input.GetTouch(0).position;
                isClicked = true;
            }
            else if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                pos = Mouse.current.position.ReadValue();
                isClicked = true;
            }
            else if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Space))
            {
                triggerShoot();
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                triggerAttack();
            }
        }
        catch (Exception) { }

        if (isClicked )
        {
            //Debug.Log($"is clicked {pos}");
            Ray ray = mainCamera.ScreenPointToRay(pos);
            
            //if we don't hit anything then return
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }
            //Debug.Log($"Physics ");
            // Check whether we hit own targetable unit or others
            if (hit.collider.TryGetComponent<Targetable>(out Targetable target))
            {
                Debug.Log($"Target ");
                if (target.hasAuthority)
                {
                    //Debug.Log($"Right Clicked Mouse try move {hit.point}");
                    TryMove(hit.point, Targeter.AttackType.Nothing);
                    return;
                }

                TryTarget(target, Targeter.AttackType.Shoot);
                return;
            }
            TryMove(hit.point, Targeter.AttackType.Nothing);
        }
    }

   
    IEnumerator MoveAttackReturn(Unit unit, Vector3 attackpos , Vector3 returnpos, Targetable enemy)
    {
        TryMoveAttack(unit,attackpos, 200, Targeter.AttackType.Slash);
        yield return new WaitForSeconds(0.1f);
        TryAttack(unit, enemy, Targeter.AttackType.Slash);
        yield return new WaitForSeconds(0.5f);
        TryMoveAttack(unit, returnpos, 200, Targeter.AttackType.Slash);
        yield return new WaitForSeconds(0.1f);
        TryIdle();
    }


    public void triggerShoot()
    {
        //Debug.Log($" unitSelectionHandler.SelectedUnits try shoot {unitSelectionHandler.SelectedUnits.Count}");
        Ray ray = mainCamera.ScreenPointToRay(findNearest(targetTag, 10000));
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }
        if (hit.collider.TryGetComponent<Targetable>(out Targetable target))
        {
            //Debug.Log($"Trigger shoot {target} ");
            TryTarget(target, Targeter.AttackType.Shoot);
        }
    }
    public void triggerDodge()
    {
        //Debug.Log($" unitSelectionHandler.SelectedUnits  try dodge {unitSelectionHandler.SelectedUnits.Count}" );
        System.Random ran = new System.Random();
        int ranIndex = ran.Next(0, GameObject.FindGameObjectsWithTag("SpawnPoints").Length);
        Debug.Log($"ranIndex {ranIndex} GameObject.FindGameObjectsWithTag SpawnPoints  {GameObject.FindGameObjectsWithTag("SpawnPoints")[ranIndex].transform.position}");
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            Vector3 oldpos = unit.transform.position;
            Vector3 pos = GameObject.FindGameObjectsWithTag("SpawnPoints")[ranIndex].transform.position ;
            oldpos = pos;
            Debug.Log($"Trigger Dodge {oldpos} {pos} ");
            TryMoveAttack(unit, pos, 1000, Targeter.AttackType.Slash);
            //Debug.Log($"after triggerDodge .... ");
        }
    }
    public void triggerAttack(){

        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            Vector3 pos = findNearest(targetTag, 10000);
            Vector3 oldpos = unit.transform.position;

            //Vector3 directionOfTravel =  mainCamera.ScreenToWorldPoint(pos) - unit.transform.position;
            //Vector3 finalDirection = directionOfTravel + directionOfTravel.normalized * 10;
            //oldpos = mainCamera.ScreenToWorldPoint(pos) + finalDirection;
            
            //Debug.Log($"Attack oldpos {oldpos} / new pos { mainCamera.ScreenToWorldPoint(pos)}");
            if (!Physics.Raycast(mainCamera.ScreenPointToRay(pos), out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }
            //Debug.Log($"Attack oldpos {oldpos} / new pos { hit.point}");

            StartCoroutine(MoveAttackReturn(unit, hit.point, oldpos, hit.collider.GetComponent<Targetable>()));
        }
    }

    private void TryMove(Vector3 point , Targeter.AttackType attackType)
    {
        //Debug.Log($" unitSelectionHandler.SelectedUnits  try move {unitSelectionHandler.SelectedUnits.Count}");
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            //Debug.Log($" unit {unit} to point {point}");
            unit.GetTargeter().CmdSetAttackType(attackType);
            unit.GetUnitMovement().CmdMove(point);
            unit.GetUnitMovement().unitNetworkAnimator.SetTrigger("run");
        }
    }
    private void TryMoveAttack(Unit unit, Vector3 point, int speed, Targeter.AttackType attackType)
    {
        //Debug.Log($"Move Attack selected unit {unit} , point {point} ");
        //foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        //{
            unit.GetTargeter().CmdSetTarget(target.gameObject, attackType);
            unit.GetUnitMovement().CmdMoveAttack(point,speed );
            unit.GetUnitMovement().unitNetworkAnimator.SetTrigger("run");
        //}
    }
    private void TryIdle( )
    {
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetUnitMovement().unitNetworkAnimator.SetTrigger("wait");
        }
    }
    private void TryAttack(Unit unit, Targetable target, Targeter.AttackType attackType)
    {
        //foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        //{
            unit.GetTargeter().CmdSetTarget(target.gameObject, attackType);
            // Set or unset the attack animation
            unit.GetUnitMovement().unitNetworkAnimator.SetTrigger("attack");
        //}

    }
    private void TryTarget(Targetable target, Targeter.AttackType attackType)
    {
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            //Debug.Log($"Try Target {target.gameObject} , {attackType}");
            unit.GetTargeter().CmdSetTarget(target.gameObject, attackType);
        }
    }

    private void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }

    private bool isDoubleTap()
    {
        bool doubleTap = false;
        for (var i = 0; i < Input.touchCount; ++i)
        {
            if (Input.GetTouch(i).phase == UnityEngine.TouchPhase.Began)
            {
                if (Input.GetTouch(i).tapCount == 2)
                {
                    //Debug.Log("Double Tap");
                    doubleTap = true;
                    break;
                }
            }
        }
        return doubleTap;
    }

    public Vector3 findNearest(string enemyTag, int range)
    {

        target = null;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortesDistance = Mathf.Infinity;
        Vector3 pos;
        GameObject otherPlayerEnemy = null;
        //Debug.Log($"enemies {enemies.Length}");
        foreach (GameObject enemy in enemies)
        {
            
            //Debug.Log($"enemy {enemy} / hasAuthority {enemy.GetComponent<Unit>().hasAuthority} , num players : {FindObjectOfType<NetworkManager>().numPlayers }");
            if(FindObjectOfType<NetworkManager>().numPlayers > 1 && enemy.GetComponent<Unit>().hasAuthority){ continue;}
            if (enemy != null && enemy != this.gameObject  )
            {
                otherPlayerEnemy = enemy;
                //Debug.Log($"otherPlayerEnemy {otherPlayerEnemy}");
                float distanceToEnemy = Vector3.Distance(transform.position, otherPlayerEnemy.transform.position);
                //targetEnemy = nearestEnemy.GetComponent<Enemy>();
                if (distanceToEnemy < shortesDistance && distanceToEnemy <= range)
                {
                    shortesDistance = distanceToEnemy;
                    target = otherPlayerEnemy;
                }
            }
            //Debug.Log($"target {target} ");
        }
        pos = target.transform.position;
        pos = mainCamera.WorldToScreenPoint(pos);
        pos.z = 0.0f;
        target.transform.Find("SelectedHighlight").gameObject.GetComponent<SpriteRenderer>().enabled = true;
        target.transform.Find("SelectedHighlight").gameObject.GetComponent<SpriteRenderer>().color = UnityEngine.Random.ColorHSV();

        return pos;
    }
}
