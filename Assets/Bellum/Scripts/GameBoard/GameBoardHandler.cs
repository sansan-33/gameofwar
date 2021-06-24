using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameBoardHandler : NetworkBehaviour
{

    [SerializeField] List<GameBoard> playerGameBoards = new List<GameBoard>();
    [SerializeField] public Transform middleLinePoint;
    [SerializeField] public Transform middleDoorPoint;
    [SerializeField] public Transform leftDoorPoint;
    [SerializeField] public Transform rightDoorPoint;

    public override void OnStartServer()
    {
        initPlayerGameBoard();
    }
    public void initPlayerGameBoard()
    {
        for (int i = 0; i < playerGameBoards.Count; i++)
        {
            playerGameBoards[i].initGameBoard();
        }
    }
    public GameObject GetSpawnPointObject(UnitMeta.UnitType unitType, int playerid)
    {
        return playerGameBoards[playerid].GetUnitPoint(unitType);
    }
    public GameObject GetSpawnPointObjectByIndex(UnitMeta.UnitType unitType, int playerid, int index)
    {
        //Debug.Log($"GetSpawnPointObjectByIndex {unitType} , playerid {playerid} index {index}");
        return playerGameBoards[playerid].GetUnitPointByIndex(unitType , index);
    }
}