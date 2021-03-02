using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    private Dictionary<UnitMeta.UnitPosition, List<UnitPoint>> board = new Dictionary<UnitMeta.UnitPosition, List<UnitPoint>>();
    private static Dictionary<UnitMeta.UnitPosition, int> roundRobinPointIndex = new Dictionary<UnitMeta.UnitPosition, int>();
    public void initGameBoard()
    {
        foreach(UnitPoint unitPoint in transform.GetComponentsInChildren<UnitPoint>())
        {
            List<UnitPoint> unitTypePoints;
            if (!board.TryGetValue(unitPoint.GetPointType(), out unitTypePoints))
            {
                unitTypePoints = new List<UnitPoint>();
                board.Add(unitPoint.GetPointType(), unitTypePoints);
            }
            unitTypePoints.Add(unitPoint);
        }
        printGameBoard();
    }
    void printGameBoard()
    {
        StringBuilder sb = new StringBuilder("Pretty Game Board \n\n");

        foreach (var unitPosition in board)
        {
            sb.Append($"Unit Position {unitPosition.Key} \n");
            foreach (var unitPoint in unitPosition.Value)
            {
                sb.Append($"\t unitPoint  {unitPoint.GetPointType() }  :  {unitPoint.GetPosition() } \n");
            }
        }
        Debug.Log(sb.ToString());
    }
    public Vector3 GetUnitPoint(UnitMeta.UnitType unitType)
    {
        //return new Vector3(0, 0, 0);
        
        if (!board.TryGetValue(UnitMeta.DefaultUnitPosition[unitType], out List<UnitPoint> points))
        {
            return new Vector3(0, 0, 0);
        }
        
        if (!roundRobinPointIndex.TryGetValue(UnitMeta.DefaultUnitPosition[unitType], out int rr))
        {
            Debug.Log($"GetUnitPoint not found {UnitMeta.DefaultUnitPosition[unitType]} ");
            roundRobinPointIndex.Add(UnitMeta.DefaultUnitPosition[unitType], 0);
        }

        rr = (rr + 1) % points.Count;
        roundRobinPointIndex[UnitMeta.DefaultUnitPosition[unitType]] = rr;
        Debug.Log($"GetUnitPoint {UnitMeta.DefaultUnitPosition[unitType]} roundRobinPoint {rr}");    
        return points[rr].GetPosition();
    }

}
