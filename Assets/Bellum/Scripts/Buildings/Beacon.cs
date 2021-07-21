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
        Unit.ClientOnUnitSpawned += PowerUpUnit;

    }
    public void OnDestroy()
    {
        Unit.ClientOnUnitSpawned -= PowerUpUnit;
    }
    public void PowerUpTeam()
    {
        foreach (Unit unit in player.GetMyUnits())
        {
            PowerUpUnit(unit);
        }
    }
    public void PowerUpUnit(Unit unit)
    {
        if (unit.tag.Substring(unit.tag.Length - 1) != playerID.ToString()) { return; }
        unit.GetComponent<EffectStatus>().SetEffect(UnitMeta.EffectType.ATTACK.ToString(), 10f);
        unit.GetComponent<EffectStatus>().SetEffect(UnitMeta.EffectType.HEALTH.ToString(), 2f);
        unit.GetComponent<EffectStatus>().SetEffect(UnitMeta.EffectType.SPEED.ToString(), 2f);
        unit.GetComponent<EffectStatus>().SetEffect(UnitMeta.EffectType.DEFENSE.ToString(), 1.1f);
        
    }

}