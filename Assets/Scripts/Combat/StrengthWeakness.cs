using System;
using System.Collections.Generic;
using UnityEngine;

public class StrengthWeakness : MonoBehaviour
{
    private Dictionary<UnitMeta.UnitType, UnitMeta.UnitType[]> strengthWeakness = new Dictionary<UnitMeta.UnitType, UnitMeta.UnitType[]>();
    //private Dictionary<string, string> weakness = new Dictionary<string, string>();
    private int StrengthDamage = 2;
    private int WeaknessDamage = 2;

    public void Start()
    {
        //DEFINE Strength Weakness of Player --> Strength , Weakness
        strengthWeakness.Add(UnitMeta.UnitType.ARCHER, new UnitMeta.UnitType[] { UnitMeta.UnitType.FOOTMAN, UnitMeta.UnitType.TANK } );
        strengthWeakness.Add(UnitMeta.UnitType.TANK, new UnitMeta.UnitType[] { UnitMeta.UnitType.CAVALRY, UnitMeta.UnitType.FOOTMAN });
        strengthWeakness.Add(UnitMeta.UnitType.FOOTMAN, new UnitMeta.UnitType[] { UnitMeta.UnitType.TANK, UnitMeta.UnitType.MAGIC });
        strengthWeakness.Add(UnitMeta.UnitType.HERO, new UnitMeta.UnitType[] { UnitMeta.UnitType.HERO, UnitMeta.UnitType.HERO }); // HERO Strength to all , weak to HEROR only
        strengthWeakness.Add(UnitMeta.UnitType.CAVALRY, new UnitMeta.UnitType[] { UnitMeta.UnitType.FOOTMAN, UnitMeta.UnitType.TANK });
        strengthWeakness.Add(UnitMeta.UnitType.MAGIC, new UnitMeta.UnitType[] { UnitMeta.UnitType.FOOTMAN, UnitMeta.UnitType.TANK });
        strengthWeakness.Add(UnitMeta.UnitType.KING, new UnitMeta.UnitType[] { UnitMeta.UnitType.KING, UnitMeta.UnitType.KING });

    }
    public float calculateDamage(UnitMeta.UnitType player, UnitMeta.UnitType enemy, float damage)
    {
        //Debug.Log($"calculateDamage player {player} vs  enemy {enemy} , original damage {damage} ");
        float damageResult = damage;
        UnitMeta.UnitType[]  dict = strengthWeakness[player];
        if (dict[0] == enemy || dict[0] == UnitMeta.UnitType.HERO)
            damageResult = damage * StrengthDamage;
        else if (dict[1] == enemy)
        {
            damageResult = damage / WeaknessDamage;
            damageResult = damageResult > 0 ? damageResult : 1;
        }
        return (int)damageResult;
    }
}
