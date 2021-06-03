using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Tornado : NetworkBehaviour
{
    public Transform tornadoCenter;
    public float pullForce;
    public float refreshRate;
    int playerid = 0;
    int enemyid = 0;
    [SerializeField] private string unitType;

    public override void OnStartClient()
    {
        if (NetworkClient.connection.identity == null) { return; }
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        playerid = player.GetPlayerID();
        enemyid = player.GetEnemyID();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Tornado OnTriggerEnter {other.tag} ");

        if (CanPull(other)) {
            StartCoroutine(pullObject(other, true));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (CanPull(other))
        {
            StartCoroutine(pullObject(other, false));
        }
    }

    IEnumerator pullObject(Collider x, bool shouldPull)
    {
        if (shouldPull)
        {
            Vector3 forceDir = tornadoCenter.position - x.transform.position;
            x.GetComponent<Rigidbody>().AddForce(forceDir.normalized * pullForce * Time.deltaTime);
            yield return refreshRate;
            StartCoroutine(pullObject(x, shouldPull));
        }
    }

    private bool CanPull(Collider other)
    {
        bool sameTeam = true;
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)
        {
            if ((other.tag == "Player" + enemyid || other.tag == "King" + enemyid) && unitType == "Enemy") { return false; }  
            if ((other.tag == "Player" + playerid || other.tag == "King" + playerid) && unitType == "Player") { return false; }  //check to see if it belongs to the player, if it does, do nothing
        }
        else // Multi player seneriao
        {
            if (this.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))
            {
                if (!networkIdentity.hasAuthority) { return false; }
                if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity OtherNetworkIdentity))  //try and get the NetworkIdentity component to see if it's a unit/building 
                {
                    if (OtherNetworkIdentity.hasAuthority) { sameTeam = false; }  //check to see if it belongs to the player, if it does, do nothing
                }
            }
        }
        return sameTeam;
    }
    [Server]
    public void SetPlayerType(int playerid)
    {
        this.unitType = playerid == 0 ? "Player" : "Enemy";
    }

}
