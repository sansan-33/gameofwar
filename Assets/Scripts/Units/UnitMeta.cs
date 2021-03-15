using System;
using System.Collections.Generic;

public class UnitMeta
{
    public static string PLAYERTAG = "Player0";
    public static string ENEMYTAG = "Player1";
    public static string KINGPLAYERTAG = "King0";
    public static string KINGENEMYTAG = "King1";
    public enum UnitKey { ARCHER, KNIGHT, MAGE, CAVALRY, SPEARMAN, HERO, MINISKELETON, GIANT, KING, UNDEADHERO, UNDEADARCHER, UNDEADKING, RIDER, LICH,HUMANWALL, UNDEADWALL };
    public enum UnitType { ARCHER, TANK, MAGIC, CAVALRY, FOOTMAN, WALL, HERO, KING  };
    public enum UnitPosition { FORWARD, MIDFIELDER, DEFENDER, GOALIE, WALL};
    public enum Race { HUMAN, UNDEAD, ELF, ALL };
  
    public static Dictionary<UnitType, int> UnitSize = new Dictionary<UnitType, int>() { { UnitType.FOOTMAN, 3 }, { UnitType.ARCHER, 2 } };
    public static Dictionary<UnitType, int> UnitSelfDestory = new Dictionary<UnitType, int>() { { UnitType.WALL, 30 }  };
    public static Dictionary<UnitType, int> UnitEleixer = new Dictionary<UnitType, int>()
    {
        { UnitType.ARCHER, 2 },
        { UnitType.CAVALRY, 3 },
        { UnitType.TANK, 4 },
        { UnitType.HERO, 5 },
        { UnitType.FOOTMAN, 1 },
        { UnitType.MAGIC, 3 },
        { UnitType.WALL, 1 }

    };
    public static Dictionary<UnitType, TacticalBehavior.BehaviorSelectionType> DefaultUnitTactical = new Dictionary<UnitType, TacticalBehavior.BehaviorSelectionType>()
    {
        { UnitType.ARCHER, TacticalBehavior.BehaviorSelectionType.Attack } ,
        { UnitType.MAGIC, TacticalBehavior.BehaviorSelectionType.Attack } ,
        { UnitType.CAVALRY, TacticalBehavior.BehaviorSelectionType.Attack } ,
        { UnitType.FOOTMAN, TacticalBehavior.BehaviorSelectionType.Attack },
        { UnitType.HERO, TacticalBehavior.BehaviorSelectionType.Defend },
        { UnitType.TANK, TacticalBehavior.BehaviorSelectionType.Attack },
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
        { UnitType.KING, UnitPosition.GOALIE },
        { UnitType.WALL, UnitPosition.WALL }
    };
    public static Dictionary<UnitType, UnitKey> HumanTypeKey = new  Dictionary<UnitType, UnitKey>() {

        { UnitType.ARCHER, UnitKey.ARCHER } ,
        { UnitType.TANK, UnitKey.KNIGHT} ,
        { UnitType.CAVALRY, UnitKey.CAVALRY} ,
        { UnitType.MAGIC, UnitKey.MAGE },
        { UnitType.FOOTMAN, UnitKey.SPEARMAN },
        { UnitType.HERO, UnitKey.HERO },
        { UnitType.KING, UnitKey.KING },
        { UnitType.WALL, UnitKey.HUMANWALL }
    };
    public static Dictionary<UnitType, UnitKey> UndeadTypeKey = new Dictionary<UnitType, UnitKey>()
    {
        { UnitType.ARCHER, UnitKey.UNDEADARCHER } ,
        { UnitType.TANK, UnitKey.GIANT} ,
        { UnitType.CAVALRY, UnitKey.RIDER} ,
        { UnitType.MAGIC, UnitKey.LICH },
        { UnitType.FOOTMAN, UnitKey.MINISKELETON },
        { UnitType.HERO, UnitKey.UNDEADHERO },
        { UnitType.KING, UnitKey.UNDEADKING },
        { UnitType.WALL, UnitKey.UNDEADWALL }
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
        { UnitKey.HUMANWALL , UnitType.WALL },

        { UnitKey.UNDEADARCHER , UnitType.ARCHER   } ,
        { UnitKey.GIANT , UnitType.TANK  } ,
        { UnitKey.RIDER , UnitType.CAVALRY  } ,
        { UnitKey.LICH , UnitType.MAGIC },
        { UnitKey.MINISKELETON , UnitType.FOOTMAN  },
        { UnitKey.UNDEADHERO , UnitType.HERO  },
        { UnitKey.UNDEADKING , UnitType.KING  },
        { UnitKey.UNDEADWALL , UnitType.WALL }
    };
    public static Dictionary<UnitKey, bool> CanCollide = new Dictionary<UnitKey, bool>(){
        { UnitKey.RIDER,true }
    };
    public static Dictionary<UnitKey, bool> ShakeCamera = new Dictionary<UnitKey, bool>(){
        { UnitKey.CAVALRY,true }
    };

}
