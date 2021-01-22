using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitDictionary  
{
    [SerializeField] private static GameObject unitBasePrefab = null;
    [SerializeField] private static GameObject archerPrefab = null;
    [SerializeField] private static GameObject knightPrefab = null;
    [SerializeField] private static GameObject heroPrefab = null;
    [SerializeField] private static GameObject spearmanPrefab = null;
    [SerializeField] private static GameObject sampleUnitPrefab = null;

    public static Dictionary<Unit.UnitType, GameObject> unitDict = new Dictionary<Unit.UnitType, GameObject>
    {
        {Unit.UnitType.ARCHER, archerPrefab },
        {Unit.UnitType.HERO, heroPrefab },
        {Unit.UnitType.KNIGHT, knightPrefab},
        {Unit.UnitType.SPEARMAN, spearmanPrefab},
        {Unit.UnitType.SAMPLE, sampleUnitPrefab}
    };

    

   

}
