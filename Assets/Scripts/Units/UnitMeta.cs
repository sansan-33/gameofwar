using System;
using System.Collections.Generic;

public class UnitMeta
{

    /*
     * Attack checking 
     * 1) Unit radius usually 2, attack range at least 5
     * 2) Box collider big enough to trigger TacticalAgent can see target Physics.Linecast
     * 3) Defend radius not too large otherwise some attack outside the circle
     */
    public static string PLAYERTAG = "Player0";
    public static string ENEMYTAG = "Player1";
    public static string BLUETEAM = "Blue";
    public static string REDTEAM = "Red";
    public static string KINGPLAYERTAG = "King0";
    public static string KINGENEMYTAG = "King1";
    public enum UnitKey { ARCHER, KNIGHT, MAGE, CAVALRY, SPEARMAN, HERO, MINISKELETON, GIANT, KING,
                        UNDEADHERO, UNDEADARCHER, UNDEADKING, RIDER, LICH,HUMANWALL, UNDEADWALL,
                        ODIN,THOR,LOKI,GODARCHER,GODCAVALRY,GODSPEARMAN,GODMAGE,GODKNIGHT, GODWALL,
                        ELFRANGER, ELFCAVALRY, ELFFOOTMAN, ELFMAGE, ELFGOLEM, ELFTREEANT, ELFDEMONHUNTER, ELFWALL
                        };
    public enum UnitType { ARCHER, TANK, MAGIC, CAVALRY, FOOTMAN, WALL, HERO, KING, ALL };
    public enum UnitPosition { FORWARD, MIDFIELDER, DEFENDER, GOALIE, WALL};
    public enum Race { HUMAN, UNDEAD, ELF, GOD, ALL };
    public enum SpeedType { ORIGINAL, CURRENT, MAX };
    public enum WeaponType { THSWORD, SHSWORD, BOW, HAMMER, SPEAR, DAGGER , SPELL,AXE, LANCE, NOTHING};
     
    public static Dictionary<UnitType, int> UnitSize = new Dictionary<UnitType, int>() { { UnitType.FOOTMAN, 3 }, { UnitType.ARCHER, 2 } };
    public static Dictionary<UnitKey, bool> UnitKeyRider = new Dictionary<UnitKey, bool>() { { UnitKey.CAVALRY, true }, { UnitKey.GODCAVALRY, true }, { UnitKey.RIDER, true } };
    public static Dictionary<UnitType, float> DefendRadius = new Dictionary<UnitType, float>() { { UnitType.HERO, 8f }, { UnitType.KING, 8f } };
    public static Dictionary<UnitType, int> UnitSelfDestory = new Dictionary<UnitType, int>() { { UnitType.WALL, 10 }  };
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
        { UnitType.FOOTMAN, TacticalBehavior.BehaviorSelectionType.Surround },
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
    public static Dictionary<UnitType, UnitKey> GodTypeKey = new Dictionary<UnitType, UnitKey>()
    {
        { UnitType.ARCHER, UnitKey.GODARCHER } ,
        { UnitType.TANK, UnitKey.GODKNIGHT} ,
        { UnitType.CAVALRY, UnitKey.GODCAVALRY} ,
        { UnitType.MAGIC, UnitKey.GODMAGE },
        { UnitType.FOOTMAN, UnitKey.GODSPEARMAN },
        { UnitType.HERO, UnitKey.ODIN },
        { UnitType.KING, UnitKey.THOR },
        { UnitType.WALL, UnitKey.GODWALL }
    };
    public static Dictionary<UnitType, UnitKey> ElfTypeKey = new Dictionary<UnitType, UnitKey>()
    {
        { UnitType.ARCHER, UnitKey.ELFRANGER } ,
        { UnitType.TANK, UnitKey.ELFGOLEM} ,
        { UnitType.CAVALRY, UnitKey.ELFCAVALRY} ,
        { UnitType.MAGIC, UnitKey.ELFMAGE },
        { UnitType.FOOTMAN, UnitKey.ELFFOOTMAN },
        { UnitType.HERO, UnitKey.ELFDEMONHUNTER },
        { UnitType.KING, UnitKey.ELFTREEANT },
        { UnitType.WALL, UnitKey.ELFWALL }
    };
    public static Dictionary<Race, Dictionary<UnitType,UnitKey>> UnitRaceTypeKey = new Dictionary<Race, Dictionary<UnitType, UnitKey>>()
    {
        {Race.HUMAN , HumanTypeKey },
        {Race.UNDEAD , UndeadTypeKey },
        {Race.GOD , GodTypeKey },
        {Race.ELF , ElfTypeKey }
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
        { UnitKey.UNDEADWALL , UnitType.WALL },

        { UnitKey.GODARCHER , UnitType.ARCHER } ,
        { UnitKey.GODKNIGHT , UnitType.TANK } ,
        { UnitKey.GODCAVALRY , UnitType.CAVALRY} ,
        { UnitKey.GODMAGE , UnitType.MAGIC},
        { UnitKey.GODSPEARMAN , UnitType.FOOTMAN},
        { UnitKey.THOR , UnitType.HERO},
        { UnitKey.LOKI , UnitType.HERO},
        { UnitKey.ODIN , UnitType.KING },
        { UnitKey.GODWALL , UnitType.WALL },

        { UnitKey.ELFRANGER , UnitType.ARCHER } ,
        { UnitKey.ELFGOLEM , UnitType.TANK } ,
        { UnitKey.ELFCAVALRY , UnitType.CAVALRY} ,
        { UnitKey.ELFMAGE , UnitType.MAGIC},
        { UnitKey.ELFFOOTMAN , UnitType.FOOTMAN},
        { UnitKey.ELFDEMONHUNTER , UnitType.HERO},
        { UnitKey.ELFTREEANT , UnitType.KING },
        { UnitKey.ELFWALL , UnitType.WALL },
    };
    public static Dictionary<UnitKey, Race> KeyRace = new Dictionary<UnitKey, Race>() {

        { UnitKey.ARCHER , Race.HUMAN } ,
        { UnitKey.KNIGHT , Race.HUMAN } ,
        { UnitKey.CAVALRY , Race.HUMAN} ,
        { UnitKey.MAGE , Race.HUMAN},
        { UnitKey.SPEARMAN , Race.HUMAN},
        { UnitKey.HERO , Race.HUMAN},
        { UnitKey.KING , Race.HUMAN },
        { UnitKey.HUMANWALL , Race.HUMAN },

        { UnitKey.UNDEADARCHER , Race.UNDEAD  } ,
        { UnitKey.GIANT , Race.UNDEAD  } ,
        { UnitKey.RIDER , Race.UNDEAD  } ,
        { UnitKey.LICH , Race.UNDEAD },
        { UnitKey.MINISKELETON , Race.UNDEAD  },
        { UnitKey.UNDEADHERO , Race.UNDEAD  },
        { UnitKey.UNDEADKING , Race.UNDEAD  },
        { UnitKey.UNDEADWALL , Race.UNDEAD },

        { UnitKey.THOR , Race.GOD},
        { UnitKey.LOKI , Race.GOD},
        { UnitKey.ODIN , Race.GOD },
        { UnitKey.GODARCHER , Race.GOD } ,
        { UnitKey.GODKNIGHT , Race.GOD } ,
        { UnitKey.GODCAVALRY , Race.GOD} ,
        { UnitKey.GODMAGE , Race.GOD},
        { UnitKey.GODSPEARMAN , Race.GOD},
        { UnitKey.GODWALL , Race.GOD },

        { UnitKey.ELFDEMONHUNTER , Race.ELF},
        { UnitKey.ELFTREEANT , Race.ELF},
        { UnitKey.ELFRANGER , Race.ELF } ,
        { UnitKey.ELFGOLEM , Race.ELF } ,
        { UnitKey.ELFCAVALRY , Race.ELF} ,
        { UnitKey.ELFMAGE , Race.ELF},
        { UnitKey.ELFFOOTMAN , Race.ELF},
        { UnitKey.ELFWALL , Race.ELF }
    };
    public static Dictionary<UnitKey, bool> CanCollide = new Dictionary<UnitKey, bool>(){
        { UnitKey.CAVALRY,true },
        { UnitKey.RIDER,true },
        { UnitKey.GODCAVALRY,true },
        { UnitKey.ELFCAVALRY,true }
    };
    public static Dictionary<UnitKey, bool> ShakeCamera = new Dictionary<UnitKey, bool>(){
        { UnitKey.CAVALRY,true },
        { UnitKey.RIDER,true },
        { UnitKey.GODCAVALRY , true },
        { UnitKey.ELFCAVALRY , true }
    };

    public static Dictionary<UnitKey, WeaponType> KeyWeaponType = new Dictionary<UnitKey, WeaponType>() {

        { UnitKey.ARCHER , WeaponType.BOW } ,
        { UnitKey.KNIGHT , WeaponType.THSWORD } ,
        { UnitKey.CAVALRY , WeaponType.LANCE} ,
        { UnitKey.MAGE , WeaponType.SPELL},
        { UnitKey.SPEARMAN , WeaponType.SPEAR},
        { UnitKey.HERO , WeaponType.SHSWORD},
        { UnitKey.KING , WeaponType.SHSWORD },
        { UnitKey.HUMANWALL , WeaponType.NOTHING },

        { UnitKey.UNDEADARCHER , WeaponType.BOW  } ,
        { UnitKey.GIANT , WeaponType.AXE  } ,
        { UnitKey.RIDER , WeaponType.LANCE  } ,
        { UnitKey.LICH , WeaponType.SPELL },
        { UnitKey.MINISKELETON , WeaponType.AXE  },
        { UnitKey.UNDEADHERO , WeaponType.SHSWORD  },
        { UnitKey.UNDEADKING , WeaponType.SHSWORD  },
        { UnitKey.UNDEADWALL , WeaponType.NOTHING },

        { UnitKey.THOR , WeaponType.HAMMER},
        { UnitKey.LOKI , WeaponType.DAGGER},
        { UnitKey.ODIN , WeaponType.SPEAR },
        { UnitKey.GODARCHER , WeaponType.BOW } ,
        { UnitKey.GODKNIGHT , WeaponType.SPEAR } ,
        { UnitKey.GODCAVALRY , WeaponType.LANCE} ,
        { UnitKey.GODMAGE , WeaponType.SPELL},
        { UnitKey.GODSPEARMAN , WeaponType.SHSWORD},
        { UnitKey.GODWALL , WeaponType.NOTHING },

        { UnitKey.ELFRANGER , WeaponType.BOW  } ,
        { UnitKey.ELFGOLEM , WeaponType.HAMMER } ,
        { UnitKey.ELFCAVALRY , WeaponType.LANCE} ,
        { UnitKey.ELFMAGE , WeaponType.SPELL},
        { UnitKey.ELFFOOTMAN , WeaponType.AXE },
        { UnitKey.ELFDEMONHUNTER , WeaponType.AXE },
        { UnitKey.ELFTREEANT , WeaponType.HAMMER },
        { UnitKey.ELFWALL , WeaponType.NOTHING }
    };
}
