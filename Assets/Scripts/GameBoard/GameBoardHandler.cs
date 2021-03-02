using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameBoardHandler : NetworkBehaviour
{

    [SerializeField] List<GameBoard> playerGameBoards = new List<GameBoard>();

    public override void OnStartServer()
    {
        initPlayerGameBoard();
    }
    void initPlayerGameBoard()
    {
        for (int i = 0; i < playerGameBoards.Count; i++)
        {
            playerGameBoards[i].initGameBoard();
        }
    }
    public Vector3 GetUnitPosition(UnitMeta.UnitType unitType, int playerid)
    {
        return playerGameBoards[playerid].GetUnitPoint(unitType);
    }
}