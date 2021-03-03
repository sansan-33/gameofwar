using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public int PointType;
    public int spawnPointIndex;

    public UnitMeta.UnitPosition GetPointType()
    {
        return (UnitMeta.UnitPosition ) PointType;
    }
    public GameObject GetSpawnPointObject()
    {
        return this.gameObject;
    }
}
