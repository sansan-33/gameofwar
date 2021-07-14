using System;
using Mirror;
using UnityEngine;

public class GreatWallController : NetworkBehaviour
{
    public static event Action<string, string> GateOpened;

    //==================================== Set Skill For Unit
    public void GateOpen(string playerid, string doorIndex)
    {
        if (isServer)
            RpcGateOpen(playerid, doorIndex);
        else
            CmdGateOpen(playerid, doorIndex);
    }
    [Command]
    public void CmdGateOpen(string playerid, string doorIndex)
    {
        ServerGateOpen(playerid, doorIndex);
    }
    [ClientRpc]
    public void RpcGateOpen(string playerid, string doorIndex)
    {
        HandleGateOpen(playerid, doorIndex);
    }
    [Server]
    public void ServerGateOpen(string playerid, string doorIndex)
    {
        HandleGateOpen(playerid, doorIndex);
    }
    private void HandleGateOpen(string playerid, string doorIndex)
    {
        GateOpened?.Invoke("" + playerid, doorIndex);
    }
    void OnEnable()
    {
        foreach (MeshRenderer wall in GetComponentsInChildren<MeshRenderer>())
        {
            wall.enabled = false;
        }
    }
}
