using System;
using System.Collections.Generic;

public class UnitMeta
{
    public enum UnitType { ARCHER, KNIGHT, MAGE, CAVALRY, SPEARMAN, HERO, MINISKELETON, GIANT, KING, UNDEADHERO };
    public enum UnitPosition { FORWARD, MIDFIELDER, DEFENDER, GOALIE};
    public static Dictionary<UnitType, int> UnitSize = new Dictionary<UnitType, int>() { { UnitType.MINISKELETON, 10 } };
    public static Dictionary<UnitType, int> UnitEleixer = new Dictionary<UnitType, int>()
    {
        { UnitType.GIANT, 7 },
        {UnitType.CAVALRY, 5 },
        {UnitType.ARCHER, 4 },
        { UnitType.HERO, 3 },
        { UnitType.KNIGHT, 3 },
        { UnitType.MAGE, 6 },
        { UnitType.MINISKELETON, 2 },
        { UnitType.SPEARMAN, 3 },
        { UnitType.KING, 99 },
        { UnitType.UNDEADHERO, 3 }
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
        { UnitType.KING, TacticalBehavior.BehaviorSelectionType.Defend },
        { UnitType.UNDEADHERO, TacticalBehavior.BehaviorSelectionType.Defend }
    };
    public static Dictionary<UnitType, UnitPosition> DefaultUnitPosition = new Dictionary<UnitType, UnitPosition>()
    {
        { UnitType.ARCHER, UnitPosition.MIDFIELDER } ,
        { UnitType.KNIGHT, UnitPosition.MIDFIELDER } ,
        { UnitType.MAGE, UnitPosition.MIDFIELDER } ,
        { UnitType.CAVALRY, UnitPosition.DEFENDER} ,
        { UnitType.SPEARMAN, UnitPosition.FORWARD },
        { UnitType.HERO, UnitPosition.DEFENDER },
        { UnitType.MINISKELETON, UnitPosition.FORWARD },
        { UnitType.GIANT, UnitPosition.FORWARD },
        { UnitType.KING, UnitPosition.GOALIE },
        { UnitType.UNDEADHERO, UnitPosition.FORWARD }
    };
}
