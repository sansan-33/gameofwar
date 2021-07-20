using System.Collections;
using UnityEngine;
using Mirror;

public class Beacon : MonoBehaviour
{
    private int playerID = 0;
    private RTSPlayer player;
    void Start()
    {
        if (NetworkClient.connection.identity == null) { return; }
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        playerID = player.GetPlayerID();
        string color = playerID == 0 ? "blue" : "red";
        GetComponent<UnitBody>().SetTeamColor(color);
        PowerUpTeam();
    }
    public void OnDestroy()
    {
    }
    public void PowerUpTeam()
    {
        foreach (Unit unit in player.GetMyUnits())
        {
            if (unit.tag.Substring(unit.tag.Length - 1) != playerID.ToString()) { continue; }
            unit.GetComponent<EffectStatus>().SetEffect(UnitMeta.EffectType.ATTACK.ToString(), 10);
            unit.GetComponent<EffectStatus>().SetEffect(UnitMeta.EffectType.HEALTH.ToString(), 100);
            unit.GetComponent<EffectStatus>().SetEffect(UnitMeta.EffectType.DEFENSE.ToString(), 10);
        }
    }
    

}