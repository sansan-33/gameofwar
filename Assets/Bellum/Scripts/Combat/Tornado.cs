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
    bool destoryTornado = false;
    [SerializeField] private string unitType;
    [SerializeField] private float destroyAfterSeconds = 5f;

    public override void OnStartClient()
    {
        if (NetworkClient.connection.identity == null) { return; }
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        playerid = player.GetPlayerID();
        enemyid = player.GetEnemyID();
    }
    public override void OnStartServer()
    {
        Invoke(nameof(TurnOffPulling), destroyAfterSeconds-1f);
        Invoke(nameof(DestroySelf), destroyAfterSeconds);
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
    void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<Rigidbody>().velocity == Vector3.zero && other.GetComponent<Rigidbody>().velocity == Vector3.zero) { return; }
        if (CanPull(other) && destoryTornado)
        {
            StartCoroutine(pullObject(other, false));
        }
    }
    IEnumerator pullObject(Collider x, bool shouldPull)
    {
        if (shouldPull)
        {
            if (x == null) { yield break; }
            Vector3 center = new Vector3(tornadoCenter.position.x, x.transform.position.y, tornadoCenter.position.z);
            Vector3 forceDir = center - x.transform.position;
            x.GetComponent<Rigidbody>().AddForce(forceDir.normalized * pullForce * Time.deltaTime);
            yield return refreshRate;
            //yield return new WaitForSeconds(destroyAfterSeconds-1f);
            StartCoroutine(pullObject(x, shouldPull));
        }
        else
        {
            x.GetComponent<Rigidbody>().velocity = Vector3.zero;
            x.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            //Debug.Log($"pullObject {x.name} velocity {x.GetComponent<Rigidbody>().velocity} / {x.GetComponent<Rigidbody>().angularVelocity}");
        }
    }

    private bool CanPull(Collider other)
    {
        bool sameTeam = true;
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)
        {
            if ( other.tag.Contains(enemyid.ToString()) && unitType == "Enemy") { return false; }  
            if ( other.tag.Contains(playerid.ToString()) && unitType == "Player") { return false; }  //check to see if it belongs to the player, if it does, do nothing
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
    private void TurnOffPulling()
    {
        destoryTornado = true;
    }
    [Server]
    public void SetPlayerType(int playerid)
    {
        this.unitType = playerid == 0 ? "Player" : "Enemy";
    }
    [Server]
    private void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}
