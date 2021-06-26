using System;
using Mirror;

public class GreatWallController : NetworkBehaviour
{
    public static event Action<string> GateOpened;

    //==================================== Set Skill For Unit
    public void GateOpen(string playerid)
    {
        if (isServer)
            RpcGateOpen(playerid);
        else
            CmdGateOpen(playerid);
    }
    [Command]
    public void CmdGateOpen(string playerid)
    {
        ServerGateOpen(playerid);
    }
    [ClientRpc]
    public void RpcGateOpen(string playerid)
    {
        HandleGateOpen(playerid);
    }
    [Server]
    public void ServerGateOpen(string playerid)
    {
        HandleGateOpen(playerid);
    }
    private void HandleGateOpen(string playerid)
    {
        GateOpened?.Invoke("" + playerid);
    }
}
