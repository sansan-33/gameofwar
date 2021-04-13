using System;
using System.Collections.Generic;
using UnityEngine;

public class StrengthWeakness 
{
    private static int StrengthDamage = 2;
    private static int WeaknessDamage = 2;
    private static Dictionary<UnitMeta.UnitType, UnitMeta.UnitType[]> strengthWeakness = new Dictionary<UnitMeta.UnitType, UnitMeta.UnitType[]>() {

        //DEFINE Strength Weakness of Player --> Strength , Weakness
        { UnitMeta.UnitType.ARCHER, new UnitMeta.UnitType[] { UnitMeta.UnitType.FOOTMAN, UnitMeta.UnitType.TANK } },
        { UnitMeta.UnitType.TANK, new UnitMeta.UnitType[] { UnitMeta.UnitType.CAVALRY, UnitMeta.UnitType.FOOTMAN }},
        { UnitMeta.UnitType.FOOTMAN, new UnitMeta.UnitType[] { UnitMeta.UnitType.TANK, UnitMeta.UnitType.MAGIC }},
        { UnitMeta.UnitType.HERO, new UnitMeta.UnitType[] { UnitMeta.UnitType.HERO, UnitMeta.UnitType.HERO }}, // HERO Strength to all , weak to HEROR only
        { UnitMeta.UnitType.CAVALRY, new UnitMeta.UnitType[] { UnitMeta.UnitType.FOOTMAN, UnitMeta.UnitType.TANK }},
        { UnitMeta.UnitType.MAGIC, new UnitMeta.UnitType[] { UnitMeta.UnitType.FOOTMAN, UnitMeta.UnitType.TANK }},
        { UnitMeta.UnitType.KING, new UnitMeta.UnitType[] { UnitMeta.UnitType.KING, UnitMeta.UnitType.KING } }
    };
    public static float calculateDamage(UnitMeta.UnitType player, UnitMeta.UnitType enemy, float damage)
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
