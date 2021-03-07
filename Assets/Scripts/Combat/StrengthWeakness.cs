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
        strengthWeakness.Add(UnitMeta.UnitType.ARCHER, new UnitMeta.UnitType[] { UnitMeta.UnitType.SPEARMAN, UnitMeta.UnitType.KNIGHT } );
        strengthWeakness.Add(UnitMeta.UnitType.KNIGHT, new UnitMeta.UnitType[] { UnitMeta.UnitType.ARCHER, UnitMeta.UnitType.SPEARMAN });
        strengthWeakness.Add(UnitMeta.UnitType.SPEARMAN, new UnitMeta.UnitType[] { UnitMeta.UnitType.KNIGHT, UnitMeta.UnitType.ARCHER });
        strengthWeakness.Add(UnitMeta.UnitType.HERO, new UnitMeta.UnitType[] { UnitMeta.UnitType.HERO, UnitMeta.UnitType.HERO }); // HERO Strength to all , weak to HEROR only
        strengthWeakness.Add(UnitMeta.UnitType.CAVALRY, new UnitMeta.UnitType[] { UnitMeta.UnitType.MAGE, UnitMeta.UnitType.KNIGHT });
        strengthWeakness.Add(UnitMeta.UnitType.MINISKELETON, new UnitMeta.UnitType[] { UnitMeta.UnitType.HERO, UnitMeta.UnitType.MAGE });
        strengthWeakness.Add(UnitMeta.UnitType.MAGE, new UnitMeta.UnitType[] { UnitMeta.UnitType.MINISKELETON, UnitMeta.UnitType.KNIGHT });
        strengthWeakness.Add(UnitMeta.UnitType.GIANT, new UnitMeta.UnitType[] { UnitMeta.UnitType.MAGE, UnitMeta.UnitType.ARCHER });
        strengthWeakness.Add(UnitMeta.UnitType.KING, new UnitMeta.UnitType[] { UnitMeta.UnitType.SPEARMAN, UnitMeta.UnitType.KING });

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
