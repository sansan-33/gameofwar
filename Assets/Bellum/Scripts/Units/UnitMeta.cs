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
    public static string ENEMY_USERID = "-1";
    public enum UnitKey { ARCHER, KNIGHT, MAGE, CAVALRY, SPEARMAN, HERO, MINISKELETON, GIANT, KING,
                        UNDEADHERO, UNDEADARCHER, UNDEADKING, RIDER, LICH,HUMANWALL, UNDEADWALL,
                        ODIN,THOR,LOKI,GODARCHER,GODCAVALRY,GODSPEARMAN,GODMAGE,GODKNIGHT, GODWALL,
                        ELFRANGER, ELFCAVALRY, ELFFOOTMAN, ELFMAGE, ELFGOLEM, ELFTREEANT, ELFDEMONHUNTER, ELFWALL, UNDEADQUEEN,ELFQUEEN,MULAN,
                        HUMANTOWER,HUMANBARRACK,HUMANCATAPULT, UNDEADTOWER, UNDEADBARRACK, UNDEADCATAPULT, ELFTOWER, ELFBARRACK, ELFCATAPULT, GODTOWER, GODBARRACK, GODCATAPULT
    };
    public enum UnitType { ARCHER, TANK, MAGIC, CAVALRY, FOOTMAN, WALL, TOWER, BARRACK, CATAPULT, HERO, KING, ALL };
    public enum UnitSkill { DASH, SHIELD, HEAL, TORNADO, VOLLEY, SLOW, PROVOKE, CHARGE, SNEAK, SCALE, NOTHING, DEFAULT, ARROWRAIN };
    public enum UnitPosition { FORWARD, MIDFIELDER, DEFENDER, GOALIE, WALL};
    public enum Race { HUMAN, UNDEAD, ELF, GOD, ALL };
    public enum SpeedType { ORIGINAL, CURRENT, MAX };
    public enum WeaponType { THSWORD, SHSWORD, BOW, HAMMER, SPEAR, DAGGER , SPELL,AXE, LANCE, PUNCH, NOTHING, CANNON, SPAWNER};
     
    public static Dictionary<UnitType, int> UnitSize = new Dictionary<UnitType, int>() { { UnitType.FOOTMAN, 4 }, { UnitType.ARCHER, 2 } };
    public static Dictionary<UnitKey, bool> UnitKeyRider = new Dictionary<UnitKey, bool>() { { UnitKey.CAVALRY, true }, { UnitKey.GODCAVALRY, true }, { UnitKey.RIDER, true }, { UnitKey.ELFCAVALRY, true } };
    public static Dictionary<UnitType, float> DefendRadius = new Dictionary<UnitType, float>() { { UnitType.HERO, 8f }, { UnitType.KING, 8f } };
    public static Dictionary<UnitType, int> UnitSelfDestory = new Dictionary<UnitType, int>() { { UnitType.WALL, 10 }, { UnitType.TOWER, 30 }, { UnitType.CATAPULT, 20 }, { UnitType.BARRACK, 30 } };
    public static Dictionary<UnitType, int> UnitEleixer = new Dictionary<UnitType, int>()
    {
        { UnitType.ARCHER, 3 },
        { UnitType.CAVALRY, 5 },
        { UnitType.TANK, 7 },
        { UnitType.HERO, 5 },
        { UnitType.FOOTMAN, 2 },
        { UnitType.MAGIC, 5 },
        { UnitType.WALL, 1 },
        { UnitType.BARRACK, 1 },
        { UnitType.TOWER, 1 },
        { UnitType.CATAPULT, 1 }
    };
    
    public static Dictionary<UnitType, TacticalBehavior.BehaviorSelectionType> DefaultUnitTactical = new Dictionary<UnitType, TacticalBehavior.BehaviorSelectionType>()
    {
        { UnitType.ARCHER, TacticalBehavior.BehaviorSelectionType.Attack } ,
        { UnitType.MAGIC, TacticalBehavior.BehaviorSelectionType.Attack } ,
        { UnitType.CAVALRY, TacticalBehavior.BehaviorSelectionType.Charge } ,
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
        { UnitType.WALL, UnitPosition.WALL },
        { UnitType.BARRACK, UnitPosition.WALL },
        { UnitType.TOWER, UnitPosition.WALL },
        { UnitType.CATAPULT, UnitPosition.WALL }
    };
    public static Dictionary<UnitType, UnitKey> HumanTypeKey = new  Dictionary<UnitType, UnitKey>() {

        { UnitType.ARCHER, UnitKey.ARCHER } ,
        { UnitType.TANK, UnitKey.KNIGHT} ,
        { UnitType.CAVALRY, UnitKey.CAVALRY} ,
        { UnitType.MAGIC, UnitKey.MAGE },
        { UnitType.FOOTMAN, UnitKey.SPEARMAN },
        { UnitType.HERO, UnitKey.HERO },
        //{ UnitType.HERO, UnitKey.MULAN },
        { UnitType.KING, UnitKey.KING },
        { UnitType.WALL, UnitKey.HUMANWALL },
        { UnitType.BARRACK, UnitKey.HUMANBARRACK },
        { UnitType.TOWER, UnitKey.HUMANTOWER },
        { UnitType.CATAPULT, UnitKey.HUMANCATAPULT }
    };
    public static Dictionary<UnitType, UnitKey> UndeadTypeKey = new Dictionary<UnitType, UnitKey>()
    {
        { UnitType.ARCHER, UnitKey.UNDEADARCHER } ,
        { UnitType.TANK, UnitKey.GIANT} ,
        { UnitType.CAVALRY, UnitKey.RIDER} ,
        { UnitType.MAGIC, UnitKey.LICH },
        { UnitType.FOOTMAN, UnitKey.MINISKELETON },
        { UnitType.HERO, UnitKey.UNDEADHERO },
        //{ UnitType.HERO, UnitKey.UNDEADQUEEN },
        { UnitType.KING, UnitKey.UNDEADKING },
        { UnitType.WALL, UnitKey.UNDEADWALL },
        { UnitType.BARRACK, UnitKey.UNDEADBARRACK },
        { UnitType.TOWER, UnitKey.UNDEADTOWER },
        { UnitType.CATAPULT, UnitKey.UNDEADCATAPULT }
    };
    public static Dictionary<UnitType, UnitKey> GodTypeKey = new Dictionary<UnitType, UnitKey>()
    {
        { UnitType.ARCHER, UnitKey.GODARCHER } ,
        { UnitType.TANK, UnitKey.GODKNIGHT} ,
        { UnitType.CAVALRY, UnitKey.GODCAVALRY} ,
        { UnitType.MAGIC, UnitKey.GODMAGE },
        { UnitType.FOOTMAN, UnitKey.GODSPEARMAN },
        { UnitType.HERO, UnitKey.THOR },
        { UnitType.KING, UnitKey.ODIN },
        { UnitType.WALL, UnitKey.GODWALL },
        { UnitType.BARRACK, UnitKey.GODBARRACK },
        { UnitType.TOWER, UnitKey.GODTOWER },
        { UnitType.CATAPULT, UnitKey.GODCATAPULT }
    };
    public static Dictionary<UnitType, UnitKey> ElfTypeKey = new Dictionary<UnitType, UnitKey>()
    {
        { UnitType.ARCHER, UnitKey.ELFRANGER } ,
        { UnitType.TANK, UnitKey.ELFGOLEM} ,
        { UnitType.CAVALRY, UnitKey.ELFCAVALRY} ,
        { UnitType.MAGIC, UnitKey.ELFMAGE },
        { UnitType.FOOTMAN, UnitKey.ELFFOOTMAN },
        { UnitType.HERO, UnitKey.ELFDEMONHUNTER },
        //{ UnitType.HERO, UnitKey.ELFQUEEN },
        { UnitType.KING, UnitKey.ELFTREEANT },
        { UnitType.WALL, UnitKey.ELFWALL },
        { UnitType.BARRACK, UnitKey.ELFBARRACK },
        { UnitType.TOWER, UnitKey.ELFTOWER },
        { UnitType.CATAPULT, UnitKey.ELFCATAPULT }
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
        { UnitKey.MULAN , UnitType.HERO},
        { UnitKey.KING , UnitType.KING },
        { UnitKey.HUMANWALL , UnitType.WALL },
        { UnitKey.HUMANTOWER , UnitType.TOWER },
        { UnitKey.HUMANBARRACK , UnitType.BARRACK },
        { UnitKey.HUMANCATAPULT , UnitType.CATAPULT },

        { UnitKey.UNDEADARCHER , UnitType.ARCHER   } ,
        { UnitKey.GIANT , UnitType.TANK  } ,
        { UnitKey.RIDER , UnitType.CAVALRY  } ,
        { UnitKey.LICH , UnitType.MAGIC },
        { UnitKey.MINISKELETON , UnitType.FOOTMAN  },
        { UnitKey.UNDEADHERO , UnitType.HERO  },
        { UnitKey.UNDEADQUEEN , UnitType.HERO  },
        { UnitKey.UNDEADKING , UnitType.KING  },
        { UnitKey.UNDEADWALL , UnitType.WALL },
        { UnitKey.UNDEADTOWER , UnitType.TOWER },
        { UnitKey.UNDEADBARRACK , UnitType.BARRACK },
        { UnitKey.UNDEADCATAPULT , UnitType.CATAPULT },

        { UnitKey.GODARCHER , UnitType.ARCHER } ,
        { UnitKey.GODKNIGHT , UnitType.TANK } ,
        { UnitKey.GODCAVALRY , UnitType.CAVALRY} ,
        { UnitKey.GODMAGE , UnitType.MAGIC},
        { UnitKey.GODSPEARMAN , UnitType.FOOTMAN},
        { UnitKey.THOR , UnitType.HERO},
        { UnitKey.LOKI , UnitType.HERO},
        { UnitKey.ODIN , UnitType.KING },
        { UnitKey.GODWALL , UnitType.WALL },
        { UnitKey.GODTOWER , UnitType.TOWER },
        { UnitKey.GODBARRACK , UnitType.BARRACK },
        { UnitKey.GODCATAPULT , UnitType.CATAPULT },

        { UnitKey.ELFRANGER , UnitType.ARCHER } ,
        { UnitKey.ELFGOLEM , UnitType.TANK } ,
        { UnitKey.ELFCAVALRY , UnitType.CAVALRY} ,
        { UnitKey.ELFMAGE , UnitType.MAGIC},
        { UnitKey.ELFFOOTMAN , UnitType.FOOTMAN},
        { UnitKey.ELFQUEEN , UnitType.HERO},
        { UnitKey.ELFDEMONHUNTER , UnitType.HERO},
        { UnitKey.ELFTREEANT , UnitType.KING },
        { UnitKey.ELFWALL , UnitType.WALL },
        { UnitKey.ELFTOWER , UnitType.TOWER },
        { UnitKey.ELFBARRACK , UnitType.BARRACK },
        { UnitKey.ELFCATAPULT , UnitType.CATAPULT }

    };
    public static Dictionary<UnitKey, Race> KeyRace = new Dictionary<UnitKey, Race>() {

        { UnitKey.ARCHER , Race.HUMAN } ,
        { UnitKey.KNIGHT , Race.HUMAN } ,
        { UnitKey.CAVALRY , Race.HUMAN} ,
        { UnitKey.MAGE , Race.HUMAN},
        { UnitKey.SPEARMAN , Race.HUMAN},
        { UnitKey.HERO , Race.HUMAN},
        { UnitKey.MULAN , Race.HUMAN},
        { UnitKey.KING , Race.HUMAN },
        { UnitKey.HUMANWALL , Race.HUMAN },
        { UnitKey.HUMANBARRACK , Race.HUMAN },
        { UnitKey.HUMANTOWER , Race.HUMAN },
        { UnitKey.HUMANCATAPULT , Race.HUMAN },

        { UnitKey.UNDEADARCHER , Race.UNDEAD  } ,
        { UnitKey.GIANT , Race.UNDEAD  } ,
        { UnitKey.RIDER , Race.UNDEAD  } ,
        { UnitKey.LICH , Race.UNDEAD },
        { UnitKey.MINISKELETON , Race.UNDEAD  },
        { UnitKey.UNDEADHERO , Race.UNDEAD  },
        { UnitKey.UNDEADQUEEN , Race.UNDEAD  },
        { UnitKey.UNDEADKING , Race.UNDEAD  },
        { UnitKey.UNDEADWALL , Race.UNDEAD },
        { UnitKey.UNDEADBARRACK , Race.UNDEAD },
        { UnitKey.UNDEADTOWER , Race.UNDEAD },
        { UnitKey.UNDEADCATAPULT , Race.UNDEAD },

        { UnitKey.THOR , Race.GOD},
        { UnitKey.LOKI , Race.GOD},
        { UnitKey.ODIN , Race.GOD },
        { UnitKey.GODARCHER , Race.GOD } ,
        { UnitKey.GODKNIGHT , Race.GOD } ,
        { UnitKey.GODCAVALRY , Race.GOD} ,
        { UnitKey.GODMAGE , Race.GOD},
        { UnitKey.GODSPEARMAN , Race.GOD},
        { UnitKey.GODWALL , Race.GOD },
        { UnitKey.GODBARRACK , Race.GOD },
        { UnitKey.GODTOWER , Race.GOD },
        { UnitKey.GODCATAPULT , Race.GOD },


        { UnitKey.ELFQUEEN , Race.ELF},
        { UnitKey.ELFDEMONHUNTER , Race.ELF},
        { UnitKey.ELFTREEANT , Race.ELF},
        { UnitKey.ELFRANGER , Race.ELF } ,
        { UnitKey.ELFGOLEM , Race.ELF } ,
        { UnitKey.ELFCAVALRY , Race.ELF} ,
        { UnitKey.ELFMAGE , Race.ELF},
        { UnitKey.ELFFOOTMAN , Race.ELF},
        { UnitKey.ELFWALL , Race.ELF },
        { UnitKey.ELFBARRACK , Race.ELF },
        { UnitKey.ELFTOWER , Race.ELF },
        { UnitKey.ELFCATAPULT , Race.ELF },

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
        { UnitKey.ELFCAVALRY , true },
        { UnitKey.ELFGOLEM , true }
    };

    public static Dictionary<UnitKey, WeaponType> KeyWeaponType = new Dictionary<UnitKey, WeaponType>() {

        { UnitKey.ARCHER , WeaponType.BOW } ,
        { UnitKey.KNIGHT , WeaponType.THSWORD } ,
        { UnitKey.CAVALRY , WeaponType.LANCE} ,
        { UnitKey.MAGE , WeaponType.SPELL},
        { UnitKey.SPEARMAN , WeaponType.SPEAR},
        { UnitKey.MULAN , WeaponType.SHSWORD},
        { UnitKey.HERO , WeaponType.SHSWORD},
        { UnitKey.KING , WeaponType.SHSWORD },
        { UnitKey.HUMANWALL , WeaponType.NOTHING },
        { UnitKey.HUMANTOWER , WeaponType.NOTHING },
        { UnitKey.HUMANBARRACK , WeaponType.SPAWNER },
        { UnitKey.HUMANCATAPULT , WeaponType.CANNON },

        { UnitKey.UNDEADARCHER , WeaponType.BOW  } ,
        { UnitKey.GIANT , WeaponType.AXE  } ,
        { UnitKey.RIDER , WeaponType.LANCE  } ,
        { UnitKey.LICH , WeaponType.SPELL },
        { UnitKey.MINISKELETON , WeaponType.AXE  },
        { UnitKey.UNDEADQUEEN , WeaponType.SHSWORD},
        { UnitKey.UNDEADHERO , WeaponType.SHSWORD  },
        { UnitKey.UNDEADKING , WeaponType.SHSWORD  },
        { UnitKey.UNDEADWALL , WeaponType.NOTHING },
        { UnitKey.UNDEADTOWER , WeaponType.NOTHING },
        { UnitKey.UNDEADBARRACK , WeaponType.SPAWNER },
        { UnitKey.UNDEADCATAPULT , WeaponType.CANNON },

        { UnitKey.THOR , WeaponType.HAMMER},
        { UnitKey.LOKI , WeaponType.DAGGER},
        { UnitKey.ODIN , WeaponType.SPEAR },
        { UnitKey.GODARCHER , WeaponType.BOW } ,
        { UnitKey.GODKNIGHT , WeaponType.SPEAR } ,
        { UnitKey.GODCAVALRY , WeaponType.LANCE} ,
        { UnitKey.GODMAGE , WeaponType.SPELL},
        { UnitKey.GODSPEARMAN , WeaponType.SHSWORD},
        { UnitKey.GODWALL , WeaponType.NOTHING },
        { UnitKey.GODTOWER , WeaponType.NOTHING },
        { UnitKey.GODBARRACK , WeaponType.SPAWNER },
        { UnitKey.GODCATAPULT , WeaponType.CANNON },

        { UnitKey.ELFRANGER , WeaponType.BOW  } ,
        { UnitKey.ELFGOLEM , WeaponType.PUNCH } ,
        { UnitKey.ELFCAVALRY , WeaponType.LANCE} ,
        { UnitKey.ELFMAGE , WeaponType.SPELL},
        { UnitKey.ELFQUEEN , WeaponType.SHSWORD},
        { UnitKey.ELFFOOTMAN , WeaponType.AXE },
        { UnitKey.ELFDEMONHUNTER , WeaponType.AXE },
        { UnitKey.ELFTREEANT , WeaponType.PUNCH },
        { UnitKey.ELFWALL , WeaponType.NOTHING },
        { UnitKey.ELFTOWER , WeaponType.NOTHING },
        { UnitKey.ELFBARRACK , WeaponType.SPAWNER },
        { UnitKey.ELFCATAPULT , WeaponType.CANNON }

    };
    public static Dictionary<UnitType, UnitSkill> UnitTypeSkillOne = new Dictionary<UnitType, UnitSkill>()
    {
        { UnitType.ARCHER, UnitSkill.NOTHING } ,
        { UnitType.MAGIC, UnitSkill.NOTHING} ,
        { UnitType.CAVALRY, UnitSkill.NOTHING } ,
        { UnitType.FOOTMAN, UnitSkill.NOTHING },
        { UnitType.TANK, UnitSkill.NOTHING }
    };
    public static Dictionary<UnitType, UnitSkill> UnitTypeSkillTwo = new Dictionary<UnitType, UnitSkill>()
    {
        { UnitType.ARCHER, UnitSkill.VOLLEY } ,
        { UnitType.MAGIC, UnitSkill.HEAL} ,
        { UnitType.CAVALRY, UnitSkill.DASH } ,
        { UnitType.FOOTMAN, UnitSkill.SNEAK },
        { UnitType.TANK, UnitSkill.PROVOKE }
    };
    public static Dictionary<UnitType, UnitSkill> UnitTypeSkillThree = new Dictionary<UnitType, UnitSkill>()
    {
        { UnitType.ARCHER, UnitSkill.SLOW } ,
        { UnitType.MAGIC, UnitSkill.TORNADO} ,
        { UnitType.CAVALRY, UnitSkill.CHARGE } ,
        { UnitType.FOOTMAN, UnitSkill.SCALE },
        { UnitType.TANK, UnitSkill.SHIELD }
    };
    public static Dictionary<int, Dictionary<UnitType, UnitSkill>> UnitStarSkill = new Dictionary<int, Dictionary<UnitType, UnitSkill>>()
    {
        {1 , UnitTypeSkillOne },
        {2 , UnitTypeSkillTwo },
        {3 , UnitTypeSkillThree } 
    };

    public static HashSet<UnitType> BuildingUnit = new HashSet<UnitType>()
    {
        UnitType.TOWER, UnitType.WALL, UnitType.BARRACK, UnitType.CATAPULT
    };
}
