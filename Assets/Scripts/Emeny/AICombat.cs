using System;
using System.Collections;
using UnityEngine;

public class AICombat : MonoBehaviour
{

    [SerializeField] private float attackTimer = 5f;
    [SerializeField] private float shootTimer = 1f;
    [SerializeField] private float dodgeTimer = 1f;
    [SerializeField] private Unit unitEnemy = null;
    [SerializeField] private Unit unitMain = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    private Camera mainCamera;

    private float lastFireTime;
  
    public void Start()
    {
        mainCamera = Camera.main;
    }

    
    private void Update()
    {
        
        if (Time.time > (1 / attackTimer) + lastFireTime )
        {
            GameObject enemy = GameObject.FindGameObjectWithTag("Emeny");
            GameObject main = GameObject.FindGameObjectWithTag("Player");
            //Debug.Log($"AI Combat update unitMain {main.transform.position} ......");

            StartCoroutine(MoveAttackReturn(enemy.GetComponent<Unit>(), main.transform.position, enemy.transform.position, main));

            lastFireTime = Time.time;
        }

    }

    IEnumerator MoveAttackReturn(Unit unit, Vector3 attackpos, Vector3 returnpos, GameObject main)
    {
        TryMoveAttack(unit, attackpos, 200, Targeter.AttackType.Slash, main);
        yield return new WaitForSeconds(0.1f);
        
    }

    private void TryMoveAttack(Unit unit, Vector3 point, int speed, Targeter.AttackType attackType, GameObject main)
    {
        //Debug.Log($"AI Combat TryMoveAttack enemy: {unit}, main: {main}, attack pos: {point} , unit.GetTargeter() {unit.GetTargeter()}");
        //unit.GetTargeter().CmdSetTarget(main, attackType);
        //Debug.Log($"unit.GetUnitMovement(): {unit.GetUnitMovement().name} ");
        //unit.GetUnitMovement().CmdMoveAttack(point, speed);
        //unit.GetUnitMovement().unitNetworkAnimator.SetTrigger("run");
         
    }
  
    
}
