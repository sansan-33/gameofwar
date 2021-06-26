using System.Collections.Generic;
using Mirror;
using Pathfinding;
using UnityEngine;

public class UnitBody : NetworkBehaviour, IBody
{

    [SerializeField] private List< Material> material;
    private  SkinnedMeshRenderer unitRenderer;
    
    public override void OnStartServer()
    {
        unitRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }
    public override void OnStartClient()
    {
        unitRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }

    //==================================== Set Skill For Unit
    public void SetRenderMaterial(string color)
    {
        if (isServer)
            RpcRenderMaterial(color);
        else
            CmdRenderMaterial(color);
    }
    [Command]
    public void CmdRenderMaterial(string color)
    {
        ServerRenderMaterial(color);
    }
    [ClientRpc]
    public void RpcRenderMaterial(string color)
    {
        HandleRenderMaterial(color);
    }
    [Server]
    public void ServerRenderMaterial(string color)
    {
        HandleRenderMaterial(color);
    }
    private void HandleRenderMaterial(string color)
    {
        unitRenderer.sharedMaterial = material[color == "blue" ? 0 : 1];
        GetComponent<GraphUpdateScene>().setTag = (color == "blue" ? 1 : 2);
        //Debug.Log($"Recalculate all graphs 12345678910 ");
        // Recalculate all graphs
        //AstarPath.active.Scan();
    }
     
    public void SetRenderMaterial(int star)
    {
        int playerid = NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetPlayerID();
        int index = playerid == 0 ? star - 1 : 3 + star - 1;
        //Debug.Log(index);
        unitRenderer.sharedMaterial = material[playerid ==0 ? star - 2 : 3 + star - 2 ];
    }
    public void SetRenderMaterial(int playerid, int star)
    {
       int index = playerid == 0 ? star - 1 : 3 + star - 1;
       //Debug.Log($"SetRenderMaterial index-->{index}playerid-->{playerid}star-->{star}unit-->{gameObject.name}");
       GetComponent<UnitBody>().GetUnitRenderer().sharedMaterial = material[index];
       // Debug.Log(unit.GetComponent<UnitBody>().GetUnitRenderer());
    }
    [Server]
    public void ServerChangeUnitRenderer(int playerid, int star)
    {
        //Debug.Log("ServerChangeUnitRenderer");
        RpcChangeUnitRenderer(playerid, star);
    }
    [ClientRpc]
    public void RpcChangeUnitRenderer(int playerid, int star)
    {
        //Debug.Log("RpcChangeUnitRenderer");
        SetRenderMaterial(playerid,star);
    }
    public void SetUnitSize(int star)
    {
        transform.localScale += new Vector3(star, star, star);
    }
    [Server]
    public void ServeChangeType(Unit unit)
    {
        ChangeType(unit);
        RpcChangeType(unit.transform.gameObject);
    }
    private void ChangeType(Unit unit)
    {
        unit.GetComponentInParent<Health>().Transformhealth();
        //transform.Find("Horseman__Polyart_Standard").gameObject.SetActive(false);
        gameObject.transform.GetChild(8).gameObject.transform.GetChild(0).gameObject.SetActive(false);
        //Debug.Log(gameObject.transform.GetChild(8).gameObject.transform.GetChild(0));
        unit.unitType = UnitMeta.UnitType.TANK;
        unit.GetUnitMovement().SetSpeed( UnitMeta.SpeedType.CURRENT, 6);
    }
    [ClientRpc]
    private void RpcChangeType(GameObject unit)
    {
        ChangeType(unit.GetComponent<Unit>());
    }
    public Renderer GetUnitRenderer()
    {
        return unitRenderer;
    }
}