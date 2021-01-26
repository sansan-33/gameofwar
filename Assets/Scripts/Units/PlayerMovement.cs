using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    
    private Camera mainCamera;
   
    #region Server

    [Command]
    private void CmdMove(Vector3 position)
    {
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }
        agent.SetDestination(hit.position);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        Debug.Log("PlayerMovement OnStartAuthority");
        mainCamera = Camera.main;
    }

    [ClientCallback]
    private void Update()
    {
        if (!hasAuthority) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) { return; }

        if (Mouse.current.leftButton.isPressed)
        {
            GameObject.FindObjectOfType<TacticalBehavior>().TryTB(3,0);
        }
        else if (Mouse.current.rightButton.isPressed)
        {
            GameObject.FindObjectOfType<TacticalBehavior>().DisableBehavior(0);
            CmdMove(hit.point);
        }
        else { return; }
    }
    #endregion
}
