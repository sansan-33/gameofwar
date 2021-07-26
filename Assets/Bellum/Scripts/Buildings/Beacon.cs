using System.Collections;
using UnityEngine;
using Mirror;
using MagicArsenal;

public class Beacon : MonoBehaviour
{
    [SerializeField] private MagicBeamStatic magicBeanStatic;
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
        StartCoroutine(StartBeam());
        StartCoroutine(PowerUpUnitDelay(unit));
    }
    IEnumerator PowerUpUnitDelay(Unit unit)
    {
        yield return new WaitForSeconds(1f);
        //Debug.Log($"Beacon handle Unit spawned {unit.name} {unit.tag}");
        if (unit.tag.Substring(unit.tag.Length - 1) != playerID.ToString()) { yield break; }
        if (unit.TryGetComponent<EffectStatus>(out EffectStatus effectStatus))
        {
            effectStatus.SetEffect(UnitMeta.EffectType.ATTACK.ToString(), 10f);
            effectStatus.GetComponent<EffectStatus>().SetEffect(UnitMeta.EffectType.HEALTH.ToString(), 2f);
            effectStatus.GetComponent<EffectStatus>().SetEffect(UnitMeta.EffectType.SPEED.ToString(), 2f);
            effectStatus.GetComponent<EffectStatus>().SetEffect(UnitMeta.EffectType.DEFENSE.ToString(), 1.1f);
        }
    }
    IEnumerator StartBeam()
    {
        yield return new WaitForSeconds(1f);
        magicBeanStatic.enabled = true;
    }
        

}