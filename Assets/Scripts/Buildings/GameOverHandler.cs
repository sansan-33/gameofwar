using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
    [SerializeField] private TMP_Text NumberOfKnight = null;
    [SerializeField] private TMP_Text NumberOfEnemy = null;
    public static event Action ServerOnGameOver;

    public static event Action<string> ClientOnGameOver;
    private int Totalplayers = -2;
    private List<UnitEnemy> enemies = new List<UnitEnemy>();
    public List<UnitBase> bases = new List<UnitBase>();

    #region Server
    private void Update()
    {
        ClientHandlePlayerUpdated();

    }
    /*public void unit()
    {
        foreach (UnitBase bases in bases)
        { 
            if (!hasAuthority)
            {
                Totalplayers++;
                
            }
        }
    }*/
        
   

    
    private void ClientHandlePlayerUpdated()
    {


        GameObject[] armies = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemy = GameObject.FindGameObjectsWithTag("Enemy");
        if (NumberOfKnight == null) { return; }
        int Totalplayers = armies.Length;
        int Totalenemies = enemy.Length;
        NumberOfKnight.text = Totalplayers.ToString();
        NumberOfEnemy.text = Totalenemies.ToString();
    }
    public override void OnStartServer()
    {
        UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned += ServerHandleBaseDespawned;
        UnitEnemy.ServerOnEnemySpawned += ServerHandleEnemySpawned;
        UnitEnemy.ServerOnEnemyDespawned += ServerHandleEnemyDespawned;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned -= ServerHandleBaseDespawned;
        UnitEnemy.ServerOnEnemySpawned -= ServerHandleEnemySpawned;
        UnitEnemy.ServerOnEnemyDespawned -= ServerHandleEnemyDespawned;
    }

    [Server]
    private void ServerHandleBaseSpawned(UnitBase unitBase)
    {
        bases.Add(unitBase);
        //Debug.Log($"ServerHandleBaseSpawned unit count : {bases.Count } ");
    }

    [Server]
    private void ServerHandleBaseDespawned(UnitBase unitBase)
    {
        bases.Remove(unitBase);

        if (bases.Count != 1) { return; }

        int playerId = bases[0].connectionToClient.connectionId;

        RpcGameOver($"Player {playerId}");

        ServerOnGameOver?.Invoke();
    }

    [Server]
    private void ServerHandleEnemySpawned(UnitEnemy unitEnemy)
    {
        enemies.Add(unitEnemy);
    }

    [Server]
    private void ServerHandleEnemyDespawned(UnitEnemy unitEnemy)
    {
        enemies.Remove(unitEnemy);
        
        int Totalenemies = enemies.Count;
        if (Totalenemies == 0)
        {
            
            int playerId = bases[0].connectionToClient.connectionId;

            RpcGameOver($"Player {playerId}");

            ServerOnGameOver?.Invoke();
        }
       
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
