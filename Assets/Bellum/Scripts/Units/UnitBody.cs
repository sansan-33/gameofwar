using System.Collections;
using System.Collections.Generic;
using FoW;
using Mirror;
using Pathfinding;
using UnityEngine;

public class UnitBody : NetworkBehaviour, IBody
{

    [SerializeField] private List<Material> material;
    private SkinnedMeshRenderer[] unitRenderer;
    private MeshRenderer[] meshRenderer;
    [SerializeField] public int doorIndex;
    [SerializeField] public string doorColor;

    public override void OnStartServer()
    {
        unitRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();
        meshRenderer = GetComponentsInChildren<MeshRenderer>();
    }
    public override void OnStartClient()
    {
        unitRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();
        meshRenderer = GetComponentsInChildren<MeshRenderer>();

    }

    //==================================== Set Skill For Unit
    public void SetTeamColor(string color)
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
        if (unitRenderer != null && unitRenderer.Length > 0)
            foreach (SkinnedMeshRenderer renderer in unitRenderer)
            {
                renderer.sharedMaterial = material[color == "blue" ? 0 : 1];
            }
        else
        {
            foreach (MeshRenderer renderer in meshRenderer)
            {
                renderer.sharedMaterial = material[color == "blue" ? 0 : 1];
            }
        }
        GetComponent<FogOfWarUnit>().team = color == "blue" ? 0 : 1;
        GetComponent<FogOfWarUnit>().circleRadius = 10;

        GetComponent<GraphUpdateScene>().setTag = (color == "blue" ? 1 : 2);
        doorColor = color;
        //Debug.Log($"Door Color changed {color} ");
        StartCoroutine(GraphUpdate());
    }
    IEnumerator GraphUpdate() {
        if (GetComponent<Unit>().unitType == UnitMeta.UnitType.DOOR)
        {
            //Debug.Log($"Door Boken !!!!!!!!!!!!!!!! ");
            GetComponent<Collider>().enabled = false;
            GetComponent<UnitAnimator>().StateControl(UnitAnimator.AnimState.OPEN);
            cmShake();
            yield return new WaitForSeconds(0.5f);
        }
        //yield return new WaitForSeconds(1f);
        //Debug.Log($"Recalculate all graphs 12345678910 ");
        AstarPath.active.UpdateGraphs(GetComponent<GraphUpdateScene>().GetBounds());
        // Recalculate only the first grid graph
        var graphToScan = AstarPath.active.data.gridGraph;
        AstarPath.active.Scan(graphToScan);
        //AstarPath.active.Scan();
    }
    public void cmShake()
    {
        CinemachineManager cmManager = GameObject.FindGameObjectWithTag("CinemachineManager").GetComponent<CinemachineManager>();
        cmManager.shake();
    }
    public void SetRenderMaterial(int star)
    {
        int playerid = NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetPlayerID();
        int index = playerid == 0 ? star - 1 : 3 + star - 1;
        //Debug.Log(index);
        unitRenderer[0].sharedMaterial = material[playerid ==0 ? star - 2 : 3 + star - 2 ];
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
        return unitRenderer[0];
    }
}