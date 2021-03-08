using System;
using System.Collections.Generic;

public class UnitMeta
{
    public enum UnitKey { ARCHER, KNIGHT, MAGE, CAVALRY, SPEARMAN, HERO, MINISKELETON, GIANT, KING, UNDEADHERO, UNDEADARCHER, UNDEADKING, RIDER, LICH };
    public enum UnitType { ARCHER, TANK, MAGIC, CAVALRY, FOOTMAN, HERO, KING };
    public enum UnitPosition { FORWARD, MIDFIELDER, DEFENDER, GOALIE};
    public enum Race { HUMAN, UNDEAD, ELF, ALL };
    public static Dictionary<UnitType, int> UnitSize = new Dictionary<UnitType, int>() { { UnitType.FOOTMAN, 5 } };
    public static Dictionary<UnitType, int> UnitEleixer = new Dictionary<UnitType, int>()
    {
        { UnitType.ARCHER, 2 },
        { UnitType.CAVALRY, 3 },
        { UnitType.TANK, 4 },
        { UnitType.HERO, 5 },
        { UnitType.FOOTMAN, 1 },
        { UnitType.MAGIC, 3 }
       
    };
    public static Dictionary<UnitType, TacticalBehavior.BehaviorSelectionType> DefaultUnitTactical = new Dictionary<UnitType, TacticalBehavior.BehaviorSelectionType>()
    {
        { UnitType.ARCHER, TacticalBehavior.BehaviorSelectionType.Defend } ,
        { UnitType.MAGIC, TacticalBehavior.BehaviorSelectionType.Defend } ,
        { UnitType.CAVALRY, TacticalBehavior.BehaviorSelectionType.Defend } ,
        { UnitType.FOOTMAN, TacticalBehavior.BehaviorSelectionType.Defend },
        { UnitType.HERO, TacticalBehavior.BehaviorSelectionType.Defend },
        { UnitType.TANK, TacticalBehavior.BehaviorSelectionType.Defend },
        { UnitType.KING, TacticalBehavior.BehaviorSelectionType.Defend }
    };
    public static Dictionary<UnitType, UnitPosition> DefaultUnitPosition = new Dictionary<UnitType, UnitPosition>()
    {
        { UnitType.ARCHER, UnitPosition.MIDFIELDER } ,
        { UnitType.CAVALRY, UnitPosition.DEFENDER} ,
        { UnitType.MAGIC, UnitPosition.MIDFIELDER },
        { UnitType.TANK, UnitPosition.FORWARD },
        { UnitType.FOOTMAN, UnitPosition.FORWARD },
        { UnitType.HERO, UnitPosition.DEFENDER },
        { UnitType.KING, UnitPosition.GOALIE }
    };
    public static Dictionary<UnitType, UnitKey> HumanTypeKey = new  Dictionary<UnitType, UnitKey>() {

        { UnitType.ARCHER, UnitKey.ARCHER } ,
        { UnitType.TANK, UnitKey.KNIGHT} ,
        { UnitType.CAVALRY, UnitKey.CAVALRY} ,
        { UnitType.MAGIC, UnitKey.MAGE },
        { UnitType.FOOTMAN, UnitKey.SPEARMAN },
        { UnitType.HERO, UnitKey.HERO },
        { UnitType.KING, UnitKey.KING }
    };
    public static Dictionary<UnitType, UnitKey> UndeadTypeKey = new Dictionary<UnitType, UnitKey>()
    {
        { UnitType.ARCHER, UnitKey.UNDEADARCHER } ,
        { UnitType.TANK, UnitKey.GIANT} ,
        { UnitType.CAVALRY, UnitKey.RIDER} ,
        { UnitType.MAGIC, UnitKey.LICH },
        { UnitType.FOOTMAN, UnitKey.MINISKELETON },
        { UnitType.HERO, UnitKey.UNDEADHERO },
        { UnitType.KING, UnitKey.UNDEADKING }
    };
    public static Dictionary<Race, Dictionary<UnitType,UnitKey>> UnitRaceTypeKey = new Dictionary<Race, Dictionary<UnitType, UnitKey>>()
    {
        {Race.HUMAN , HumanTypeKey },
        {Race.UNDEAD , UndeadTypeKey }
    };
    public static Dictionary<UnitKey, UnitType> KeyType = new Dictionary<UnitKey, UnitType>() {

        { UnitKey.ARCHER , UnitType.ARCHER } ,
        { UnitKey.KNIGHT , UnitType.TANK } ,
        { UnitKey.CAVALRY , UnitType.CAVALRY} ,
        { UnitKey.MAGE , UnitType.MAGIC},
        { UnitKey.SPEARMAN , UnitType.FOOTMAN},
        { UnitKey.HERO , UnitType.HERO},
        { UnitKey.KING , UnitType.KING },

        { UnitKey.UNDEADARCHER , UnitType.ARCHER   } ,
        { UnitKey.GIANT , UnitType.TANK  } ,
        { UnitKey.RIDER , UnitType.CAVALRY  } ,
        { UnitKey.LICH , UnitType.MAGIC },
        { UnitKey.MINISKELETON , UnitType.FOOTMAN  },
        { UnitKey.UNDEADHERO , UnitType.HERO  },
        { UnitKey.UNDEADKING , UnitType.KING  }
    };

}
