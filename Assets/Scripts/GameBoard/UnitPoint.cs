using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPoint : MonoBehaviour
{
    public int PointType;
  
    public UnitMeta.UnitPosition GetPointType()
    {
        return (UnitMeta.UnitPosition ) PointType;
    }
    public Vector3 GetPosition()
    {
        return this.transform.position;
    }
}
