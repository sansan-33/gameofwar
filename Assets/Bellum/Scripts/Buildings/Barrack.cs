using System.Collections;
using UnityEngine;
using Mirror;

public class Barrack : MonoBehaviour
{
    private UnitFactory localFactory;

    private int playerID = 0;
    private Color teamColor;
    private CardDealer cardDealer;
    public Transform barrackTransform;

    void Start()
    {
        if (NetworkClient.connection.identity == null) { return; }
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        playerID = player.GetPlayerID();
        teamColor = player.GetTeamColor();
        LoadEnemies();
    }
    public void OnDestroy()
    {
    }
    public void LoadEnemies()
    {
        foreach (GameObject factroy in GameObject.FindGameObjectsWithTag("UnitFactory"))
        {
            if (factroy.GetComponent<UnitFactory>().hasAuthority)
            {
                localFactory = factroy.GetComponent<UnitFactory>();
                StartCoroutine(HandleLoadEnemies(UnitMeta.UnitType.FOOTMAN, playerID, teamColor, StaticClass.playerRace));
            }
        }
    }
    IEnumerator HandleLoadEnemies(UnitMeta.UnitType unitType, int _playerid, Color _teamColor, UnitMeta.Race race)
    {
        cardDealer = GameObject.FindGameObjectWithTag("DealManager").GetComponent<CardDealer>();
        CardStats cardStats;
        while (true)
        {
            cardStats = cardDealer.userCardStatsDict[UnitMeta.GetUnitKeyByRaceType(race,unitType).ToString()];
            localFactory.CmdDropUnit(_playerid, barrackTransform.position , race, unitType, unitType.ToString(), 1, cardStats.cardLevel, cardStats.health, cardStats.attack, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense, cardStats.special, cardStats.specialkey, cardStats.passivekey, 1, _teamColor, Quaternion.identity);
            yield return new WaitForSeconds(1.5f);
        }
    }
   
}