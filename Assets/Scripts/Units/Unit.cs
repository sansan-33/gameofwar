using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour 
{
    [SerializeField] private GameObject buildingPreview = null;
    [SerializeField] private int resourceCost = 10;
    [SerializeField] private Health health = null;
    [SerializeField] private UnitMovement unitMovement = null;
    [SerializeField] private int id = -1;
    [SerializeField] private TMP_Text taskStatus;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private UnityEvent onSelected = null;
    [SerializeField] private UnityEvent onDeselected = null;
    [SerializeField] private GameObject Cavalry;
    [SerializeField] private GameObject Knight;
    
    public UnitMeta.UnitType unitType;
    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;
    private UnitFactory localFactory;
    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;
    private int i = 0;
    public GameObject GetBuildingPreview()
    {
        return buildingPreview;
    }
    public int GetResourceCost()
    {
        return resourceCost;
    }
    public UnitMovement GetUnitMovement()
    {
        return unitMovement;
    }
    public TMP_Text GetTaskStatus()
    {
        return taskStatus;
    }
    public Targeter GetTargeter()
    {
        return targeter;
    }
    #region Server
    public int GetId()
    {
        return id;
    }
    public void SetTaskStatus(string status)
    {
        taskStatus.text = status;
    }
    public override void OnStartServer()
    {
        ServerOnUnitSpawned?.Invoke(this);

        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;

        ServerOnUnitDespawned?.Invoke(this);
    }

    [Server]
    private void ServerHandleDie()
    {
        if(this.unitType == UnitMeta.UnitType.CAVALRY&&i==0)
        {
            
            if (localFactory == null)
            {
                foreach (GameObject factroy in GameObject.FindGameObjectsWithTag("UnitFactory"))
                {
                    if (factroy.GetComponent<UnitFactory>().hasAuthority)
                    {
                        localFactory = factroy.GetComponent<UnitFactory>();
                    }
                }
            }
            GetComponent<Health>().Transformhealth();
            localFactory.Transform(Cavalry, Knight);
            ChangeType(this,GetComponent<UnitMovement>());
            RpcChangeType(GetComponent<GameObject>());
            i++;
        }
        else
        {
            NetworkServer.Destroy(gameObject);
        }
       
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        AuthorityOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (!hasAuthority) { return; }

        AuthorityOnUnitDespawned?.Invoke(this);
    }

    [Client]
    public void Select()
    {
        if (!hasAuthority) { return; }

        onSelected?.Invoke();
    }

    [Client]
    public void Deselect()
    {
        if (!hasAuthority) { return; }

        onDeselected?.Invoke();
    }

    private void OnMouseEnter()
    {
        Select();
    }

    private void OnMouseExit()
    {
        Deselect();
    }
    private void ChangeType(Unit unit , UnitMovement unitMovement)
    {
        unit.unitType = UnitMeta.UnitType.KNIGHT;
        unitMovement.GetNavMeshAgent().speed = 6;
    }
    [ClientRpc]
    private void RpcChangeType(GameObject unit)
    {
        ChangeType(unit.GetComponent<Unit>(), unit.GetComponent<UnitMovement>());
    }
    #endregion
}
