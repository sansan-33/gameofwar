using UnityEngine;

public class AstarGraphController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Unit.ClientOnUnitDespawned += ReScanGraph;
    }

    public void OnDestroy()
    {
        Unit.ClientOnUnitDespawned -= ReScanGraph;
    }
    public void ReScanGraph(Unit unit)
    {
        if(unit.unitType != UnitMeta.UnitType.WALL) { return;  }
        // Recalculate only the first grid graph
        var graphToScan = AstarPath.active.data.gridGraph;
        AstarPath.active.Scan(graphToScan);
        Debug.Log("Graph Updated");
    }
}
