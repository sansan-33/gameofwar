using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Unit : NetworkBehaviour 
{
    [SerializeField] private GameObject buildingPreview = null;
    [SerializeField] private int resourceCost = 10;
    [SerializeField] private Health health = null;
    [SerializeField] private AstarAI astarAI = null;
    [SerializeField] private UnitPowerUp unitPowerUp = null;
    [SerializeField] private int id = -1;
    [SerializeField] private TMP_Text taskStatus;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private UnityEvent onSelected = null;
    [SerializeField] private UnityEvent onDeselected = null;

    public UnitMeta.UnitType unitType;
    public UnitMeta.Race race;
    public UnitMeta.UnitKey unitKey;
    public bool isLeader = false;
    public bool isScaled = false;
    public int SpBtnTicket;
    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;
    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;
    public static event Action<Unit> ClientOnUnitSpawned;
    public static event Action<Unit> ClientOnUnitDespawned;

    [SyncVar]
    [SerializeField] private int spawnPointIndex = 0;

    public GameObject GetBuildingPreview()
    {
        return buildingPreview;
    }
    public int GetResourceCost()
    {
        return resourceCost;
    }
    public IUnitMovement GetUnitMovement()
    {
        //return unitMovement;
        return astarAI;
    }
    public UnitPowerUp GetUnitPowerUp()
    {
        return unitPowerUp;
    }
    public TMP_Text GetTaskStatus()
    {
        return taskStatus;
    }
    public Targeter GetTargeter()
    {
        return targeter;
    }
    public void SetTargeter(Targeter target)
    {
        targeter = target;
    }
    public int GetSpawnPointIndex( )
    {
        return spawnPointIndex;
    }
    public int GetId()
    {
        return id;
    }
    public void SetSpawnPointIndex(int index)
    {
        spawnPointIndex = index;
    }
    public void SetTaskStatus(string status)
    {
        taskStatus.text = status;
    }
    public override void OnStartServer()
    {
        ServerOnUnitSpawned?.Invoke(this);

        health.ServerOnDie += ServerHandleDie;
        if(UnitMeta.UnitSelfDestory.TryGetValue(unitType, out int value ) )
            Invoke(nameof(ServerHandleDie), value);      
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;

        ServerOnUnitDespawned?.Invoke(this);
    }
    #region Server

    [Server]
    private void ServerHandleDie()
    {
       // Debug.Log($"Destroy unit");
        NetworkServer.Destroy(gameObject);
    }
   
    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        AuthorityOnUnitSpawned?.Invoke(this);
    }
    public override void OnStartClient()
    {
        ClientOnUnitSpawned?.Invoke(this);
    }
    public override void OnStopClient()
    {
        ClientOnUnitDespawned?.Invoke(this);

        if (!hasAuthority) { return; }

        AuthorityOnUnitDespawned?.Invoke(this);
    }

    [Client]
    public void Select()
    {
        if (!hasAuthority ) { return; }

        onSelected?.Invoke();
    }

    [Client]
    public void Deselect()
    {
        if (!hasAuthority ) { return; }

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
    
    #endregion
}
