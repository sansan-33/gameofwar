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
        { UnitType.ARCHER, TacticalBehavior.BehaviorSelectionType.Defend } ,
        { UnitType.KNIGHT, TacticalBehavior.BehaviorSelectionType.Defend } ,
        { UnitType.MAGE, TacticalBehavior.BehaviorSelectionType.Defend } ,
        { UnitType.CAVALRY, TacticalBehavior.BehaviorSelectionType.Defend } ,
        { UnitType.SPEARMAN, TacticalBehavior.BehaviorSelectionType.Defend },
        { UnitType.HERO, TacticalBehavior.BehaviorSelectionType.Defend },
        { UnitType.MINISKELETON, TacticalBehavior.BehaviorSelectionType.Defend },
        { UnitType.GIANT, TacticalBehavior.BehaviorSelectionType.Defend },
        { UnitType.KING, TacticalBehavior.BehaviorSelectionType.Defend }
    };
}
