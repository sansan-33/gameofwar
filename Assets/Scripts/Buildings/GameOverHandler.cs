using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
    public static event Action ServerOnGameOver;

    public static event Action<string> ClientOnGameOver;

    private List<UnitBase> bases = new List<UnitBase>();
    private Dictionary<string, List<Unit>> units = new Dictionary<string, List<Unit>>();

    private string PLAYERTAG = "Player0";
    private string ENEMYTAG = "Player1";

    #region Server

    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
        units.Add(PLAYERTAG, new List<Unit>());
        units.Add(ENEMYTAG, new List<Unit>());
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
    }

    [Server]
    private void ServerHandleUnitSpawned(Unit  unit )
    {
        units[unit.tag].Add(unit );
    }

    [Server]
    private void ServerHandleUnitDespawned(Unit unit)
    {
        //Debug.Log($"Total Units {unit.tag} count {units[unit.tag].Count}");
        units[unit.tag].Remove(unit);

        if (units[unit.tag].Count != 0 ) { return; }

        RpcGameOver($"{ (unit.tag == PLAYERTAG ? ENEMYTAG : PLAYERTAG) }");

        ServerOnGameOver?.Invoke();
    }

    #endregion

    #region Client

    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        ClientOnGameOver?.Invoke(winner);
    }

    #endregion
}
