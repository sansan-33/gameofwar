using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Targeter : NetworkBehaviour
{
    private Targetable target;
    [SerializeField] private Transform aimAtPoint = null;

    public Targetable GetTarget()
    {
        return target;
    }
    public Transform GetAimAtPoint()
    {
        return aimAtPoint;
    }
    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [Command]
    public void CmdSetTarget(GameObject targetGameObject)
    {
        
        if (!targetGameObject.TryGetComponent<Targetable>(out Targetable newTarget)) { return; }
       
        target = newTarget;
        //Debug.Log($"{this.transform.GetComponent<Unit>().unitType}Target-->{target}");
    }
    [Server]
    public void ClearTarget()
    {
        target = null;
    }

    [Server]
    private void ServerHandleGameOver()
    {
        ClearTarget();
    }
    
}
