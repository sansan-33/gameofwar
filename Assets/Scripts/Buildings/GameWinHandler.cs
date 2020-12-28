using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameWinHandler : NetworkBehaviour
{
    public static event Action ServerOnGameWin;

    public static event Action<string> ClientOnGameWin;

    private List<UnitBase> bases = new List<UnitBase>();

    #region Server

    public override void OnStartServer()
    {
        UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned += ServerHandleBaseDespawned;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned -= ServerHandleBaseDespawned;
    }

    [Server]
    private void ServerHandleBaseSpawned(UnitBase unitBase)
    {
        bases.Add(unitBase);
    }

    [Server]
    private void ServerHandleBaseDespawned(UnitBase unitBase)
    {
        bases.Remove(unitBase);

        if (bases.Count != 1) { return; }

        int playerId = bases[0].connectionToClient.connectionId;

        RpcGameWin($"Player {playerId}");

        ServerOnGameWin?.Invoke();
    }

    #endregion

    #region Client

    [ClientRpc]
    private void RpcGameWin(string winner)
    {
        ClientOnGameWin?.Invoke(winner);
    }

    #endregion
}
