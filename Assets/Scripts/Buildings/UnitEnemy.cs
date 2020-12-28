using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitEnemy : NetworkBehaviour
{
    [SerializeField] private Health health = null;

    public static event Action<int> ServerOnPlayerDie;
    public static event Action<UnitEnemy> ServerOnEnemySpawned;
    public static event Action<UnitEnemy> ServerOnEnemyDespawned;

    #region Server

    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDie;

        ServerOnEnemySpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnEnemyDespawned?.Invoke(this);

        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie()
    {
        ServerOnPlayerDie?.Invoke(connectionToClient.connectionId);

        NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Client

    #endregion
}
