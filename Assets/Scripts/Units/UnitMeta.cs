using System;
using System.Collections.Generic;

public class UnitMeta
{
    public enum UnitType { ARCHER, KNIGHT, MAGE, CAVALRY, SPEARMAN, HERO, MINISKELETON, GIANT, KING };
    public static Dictionary<UnitType, int> UnitSize = new Dictionary<UnitType, int>() { { UnitType.MINISKELETON, 10 } };
    public static Dictionary<UnitType, int> UnitEleixer = new Dictionary<UnitType, int>()
    {
        { UnitType.GIANT, 7 },
        {UnitType.CAVALRY, 5 },
        {UnitType.ARCHER, 4 },
        { UnitType.HERO, 3 },
        { UnitType.KNIGHT, 3 },
        { UnitType.MAGE, 6 },
        { UnitType.MINISKELETON, 3 },
        { UnitType.SPEARMAN, 3 },
        { UnitType.KING, 99 }
    };
    public static Dictionary<UnitType, TacticalBehavior.BehaviorSelectionType> DefaultUnitTactical = new Dictionary<UnitType, TacticalBehavior.BehaviorSelectionType>()
    {
        { UnitType.ARCHER, TacticalBehavior.BehaviorSelectionType.Flank } ,
        { UnitType.KNIGHT, TacticalBehavior.BehaviorSelectionType.Attack } ,
        { UnitType.MAGE, TacticalBehavior.BehaviorSelectionType.Attack } ,
        { UnitType.CAVALRY, TacticalBehavior.BehaviorSelectionType.Attack } ,
        { UnitType.SPEARMAN, TacticalBehavior.BehaviorSelectionType.Attack },
        { UnitType.HERO, TacticalBehavior.BehaviorSelectionType.Defend },
        { UnitType.MINISKELETON, TacticalBehavior.BehaviorSelectionType.Attack },
        { UnitType.GIANT, TacticalBehavior.BehaviorSelectionType.Attack },
        { UnitType.KING, TacticalBehavior.BehaviorSelectionType.Defend }
    };
}
