using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitBody : NetworkBehaviour, IBody
{

    [SerializeField] private List< Material> material;
    [SerializeField] private  Renderer unitRenderer;
    [SerializeField] private Transform unitTransform;
    [SerializeField] private GameObject changeBody;

    public void SetRenderMaterial(int star)
    {
        int playerid = NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetPlayerID();
        int index = playerid == 0 ? star - 1 : 3 + star - 1;
        Debug.Log($"{index},{star}");

        unitRenderer.sharedMaterial = material[playerid ==0 ? star - 2 : 3 + star - 2 ];
    }
    public void SetRenderMaterial(int playerid, int star)
    {
        int index = playerid == 0 ? star - 1 : 3 + star - 1;
  
        unitRenderer.sharedMaterial = material[index];
    }
    public void SetUnitSize(int star)
    {
        unitTransform.localScale += new Vector3(star, star, star);
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
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        changeBody.SetActive(true);
        unit.unitType = UnitMeta.UnitType.KNIGHT;
        unit.GetComponentInParent<UnitMovement>().GetNavMeshAgent().speed = 6;
    }
    [ClientRpc]
    private void RpcChangeType(GameObject unit)
    {
        ChangeType(unit.GetComponent<Unit>());
    }
}