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

    public enum UnitType { ARCHER, KNIGHT, MAGE, CAVALRY, SPEARMAN, HERO, MINISKELETON, GIANT, KING };
    public static Dictionary<UnitType, int> UnitSize  = new Dictionary<UnitType, int>() { {UnitType.MINISKELETON , 10} };
    public static Dictionary<UnitType, int> UnitEleixer = new Dictionary<UnitType, int>() { { UnitType.GIANT, 7 },
        {UnitType.CAVALRY, 5 },
        {UnitType.ARCHER, 4 },
        { UnitType.HERO, 3 },
        { UnitType.KNIGHT, 3 },
        { UnitType.MAGE, 6 },
        { UnitType.MINISKELETON, 3 },
        { UnitType.SPEARMAN, 3 },
        { UnitType.KING, 99 }};
    public UnitType unitType;
    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;

    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;
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
        
       NetworkServer.Destroy(gameObject);
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
    #endregion
}
