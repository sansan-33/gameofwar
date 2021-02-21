using System;
using System.Collections.Generic;
using UnityEngine;

public class StrengthWeakness : MonoBehaviour
{
    private Dictionary<Unit.UnitType, Unit.UnitType[]> strengthWeakness = new Dictionary<Unit.UnitType, Unit.UnitType[]>();
    //private Dictionary<string, string> weakness = new Dictionary<string, string>();
    private int StrengthDamage = 2;
    private int WeaknessDamage = 2;

    public void Start()
    {
        //DEFINE Strength Weakness of Player --> Strength , Weakness
        strengthWeakness.Add(Unit.UnitType.ARCHER, new Unit.UnitType[] { Unit.UnitType.SPEARMAN, Unit.UnitType.KNIGHT } );
        strengthWeakness.Add(Unit.UnitType.KNIGHT, new Unit.UnitType[] { Unit.UnitType.ARCHER, Unit.UnitType.SPEARMAN });
        strengthWeakness.Add(Unit.UnitType.SPEARMAN, new Unit.UnitType[] { Unit.UnitType.KNIGHT, Unit.UnitType.ARCHER });
        strengthWeakness.Add(Unit.UnitType.HERO, new Unit.UnitType[] { Unit.UnitType.HERO, Unit.UnitType.HERO }); // HERO Strength to all , weak to HEROR only
        strengthWeakness.Add(Unit.UnitType.CAVALRY, new Unit.UnitType[] { Unit.UnitType.MAGE, Unit.UnitType.KNIGHT });
        strengthWeakness.Add(Unit.UnitType.MINISKELETON, new Unit.UnitType[] { Unit.UnitType.HERO, Unit.UnitType.MAGE });
        strengthWeakness.Add(Unit.UnitType.MAGE, new Unit.UnitType[] { Unit.UnitType.MINISKELETON, Unit.UnitType.KNIGHT });
        strengthWeakness.Add(Unit.UnitType.GIANT, new Unit.UnitType[] { Unit.UnitType.MAGE, Unit.UnitType.ARCHER });
        strengthWeakness.Add(Unit.UnitType.KING, new Unit.UnitType[] { Unit.UnitType.SPEARMAN, Unit.UnitType.KING });

    }
    public float calculateDamage(Unit.UnitType player, Unit.UnitType enemy, float damage)
    {
        //Debug.Log($"calculateDamage player {player} vs  enemy {enemy} , original damage {damage} ");
        float damageResult = damage;
        Unit.UnitType[]  dict = strengthWeakness[player];
        if (dict[0] == enemy || dict[0] == Unit.UnitType.HERO)
            damageResult = damage * StrengthDamage;
        else if (dict[1] == enemy)
        {
            damageResult = damage / WeaknessDamage;
            damageResult = damageResult > 0 ? damageResult : 1;
        }
        return damageResult;
    }
}
