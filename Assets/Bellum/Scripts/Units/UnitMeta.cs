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
    public static string PLAYERDIETAG = "Die0";
    public static string ENEMYDIETAG = "Die1";
    public static string BLUETEAM = "Blue";
    public static string REDTEAM = "Red";
    public static string KINGPLAYERTAG = "King0";
    public static string KINGENEMYTAG = "King1";
    public static string ENEMY_USERID = "-1";
    public enum UnitKey { ARCHER, KNIGHT, MAGE, CAVALRY, SPEARMAN, HERO, MINISKELETON, GIANT, KING,
                        UNDEADHERO, UNDEADARCHER, UNDEADKING, RIDER, LICH,HUMANWALL, UNDEADWALL,
                        ODIN,THOR,LOKI,GODARCHER,GODCAVALRY,GODSPEARMAN,GODMAGE,GODKNIGHT, GODWALL,
                        ELFRANGER, ELFCAVALRY, ELFFOOTMAN, ELFMAGE, ELFGOLEM, ELFTREEANT, ELFDEMONHUNTER, ELFWALL, UNDEADQUEEN,ELFQUEEN,MULAN,
                        HUMANTOWER,HUMANBARRACK,HUMANCATAPULT, UNDEADTOWER, UNDEADBARRACK, UNDEADCATAPULT, ELFTOWER, ELFBARRACK, ELFCATAPULT, GODTOWER, GODBARRACK, GODCATAPULT, DOOR,
                        HUMANSPIKETRAP, HUMANSIEGE, UNDEADSPIKETRAP, UNDEADSIEGE, ELFSPIKETRAP, ELFSIEGE, GODSPIKETRAP, GODSIEGE, HUMANBEACON, ELFBEACON, GODBEACON, UNDEADBEACON
    };
    public enum UnitType { ARCHER, TANK, MAGIC, CAVALRY, FOOTMAN, SIEGE, WALL, TOWER, BARRACK, CATAPULT, HERO, KING, ALL, DOOR, TRAP, QUEEN, BEACON };
    public enum UnitSkill { DASH, SHIELD, HEAL, TORNADO, VOLLEY, SLOW, PROVOKE, CHARGE, SNEAK, SCALE, NOTHING, DEFAULT, ARROWRAIN };
    public enum UnitPosition { FORWARD, MIDFIELDER, DEFENDER, GOALIE, WALL, QUEEN, HERO, SECRET };
    public enum Race { HUMAN, UNDEAD, ELF, GOD, ALL };
    public enum SpeedType { ORIGINAL, CURRENT, MAX };
    public enum WeaponType { THSWORD, SHSWORD, BOW, HAMMER, SPEAR, DAGGER , SPELL,AXE, LANCE, PUNCH, NOTHING, CANNON, SPAWNER,SIEGE};
    public enum EffectType { ATTACK, DEFENSE, HEALTH, SPEED, FREEZE, STUN, BURN };

    // Target Table
    // Target  | Provoke | Door      | Player    |  King    | Building
    //         | Player  | Provoke   | King      |  King    | Door
    //         | King
    public enum TargetTag { Provoke,Door,Player,King,Building };
    public static Dictionary<TargetTag, List<TargetTag>> TargetGroup = new Dictionary<TargetTag, List<TargetTag>>() {
        { TargetTag.Provoke, new List<TargetTag> {TargetTag.Player, TargetTag.King } },
        { TargetTag.Door, new List<TargetTag> {TargetTag.Provoke} },
        { TargetTag.Player, new List<TargetTag> {TargetTag.King } },
        { TargetTag.King, new List<TargetTag> {} },
        { TargetTag.Building, new List<TargetTag> { TargetTag.Door } }
    };

    public static Dictionary<UnitType, int> UnitSize = new Dictionary<UnitType, int>() { { UnitType.FOOTMAN, 4 }, { UnitType.ARCHER, 2 } };
    public static Dictionary<UnitKey, bool> UnitKeyRider = new Dictionary<UnitKey, bool>() { { UnitKey.CAVALRY, true }, { UnitKey.GODCAVALRY, true }, { UnitKey.RIDER, true }, { UnitKey.ELFCAVALRY, true } };
    public static Dictionary<UnitType, float> DefendRadius = new Dictionary<UnitType, float>() { { UnitType.HERO, 8f }, { UnitType.KING, 8f }, { UnitType.QUEEN, 8f } };
    public static Dictionary<UnitType, int> UnitSelfDestory = new Dictionary<UnitType, int>() { { UnitType.WALL, 20 }, { UnitType.TRAP, 15 } };
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
        { UnitType.CATAPULT, 1 },
        { UnitType.SIEGE, 3 },
        { UnitType.TRAP, 1 },
        { UnitType.BEACON, 1 }
    };
    
    public static Dictionary<UnitType, TacticalBehavior.BehaviorSelectionType> DefaultUnitTactical = new Dictionary<UnitType, TacticalBehavior.BehaviorSelectionType>()
    {
        { UnitType.ARCHER, TacticalBehavior.BehaviorSelectionType.Attack } ,
        { UnitType.MAGIC, TacticalBehavior.BehaviorSelectionType.Attack } ,
        { UnitType.CAVALRY, TacticalBehavior.BehaviorSelectionType.Attack } ,
        { UnitType.FOOTMAN, TacticalBehavior.BehaviorSelectionType.Attack },
        { UnitType.HERO, TacticalBehavior.BehaviorSelectionType.Defend },
        { UnitType.QUEEN, TacticalBehavior.BehaviorSelectionType.Defend },
        { UnitType.TANK, TacticalBehavior.BehaviorSelectionType.Attack },
        { UnitType.KING, TacticalBehavior.BehaviorSelectionType.Defend },
        { UnitType.SIEGE, TacticalBehavior.BehaviorSelectionType.Attack }
    };
    public static Dictionary<UnitType, UnitPosition> DefaultUnitPosition = new Dictionary<UnitType, UnitPosition>()
    {
        { UnitType.ARCHER, UnitPosition.MIDFIELDER } ,
        { UnitType.CAVALRY, UnitPosition.DEFENDER} ,
        { UnitType.MAGIC, UnitPosition.MIDFIELDER },
        { UnitType.TANK, UnitPosition.FORWARD },
        { UnitType.FOOTMAN, UnitPosition.FORWARD },
        { UnitType.QUEEN, UnitPosition.QUEEN },
        { UnitType.HERO, UnitPosition.HERO  },
        { UnitType.KING, UnitPosition.GOALIE },
        { UnitType.WALL, UnitPosition.WALL },
        { UnitType.BARRACK, UnitPosition.WALL },
        { UnitType.TOWER, UnitPosition.WALL },
        { UnitType.CATAPULT, UnitPosition.WALL },
        { UnitType.SIEGE, UnitPosition.WALL },
        { UnitType.TRAP, UnitPosition.WALL },
        { UnitType.BEACON, UnitPosition.SECRET },
    };
    public static Dictionary<UnitType, UnitKey> HumanTypeKey = new  Dictionary<UnitType, UnitKey>() {

        { UnitType.ARCHER, UnitKey.ARCHER } ,
        { UnitType.TANK, UnitKey.KNIGHT} ,
        { UnitType.CAVALRY, UnitKey.CAVALRY} ,
        { UnitType.MAGIC, UnitKey.MAGE },
        { UnitType.FOOTMAN, UnitKey.SPEARMAN },
        { UnitType.HERO, UnitKey.HERO },
        { UnitType.QUEEN, UnitKey.MULAN },
        { UnitType.KING, UnitKey.KING },
        { UnitType.WALL, UnitKey.HUMANWALL },
        { UnitType.BARRACK, UnitKey.HUMANBARRACK },
        { UnitType.TOWER, UnitKey.HUMANTOWER },
        { UnitType.CATAPULT, UnitKey.HUMANCATAPULT },
        { UnitType.SIEGE, UnitKey.HUMANSIEGE },
        { UnitType.TRAP , UnitKey.HUMANSPIKETRAP },
        { UnitType.BEACON , UnitKey.HUMANBEACON }
    };
    public static Dictionary<UnitType, UnitKey> UndeadTypeKey = new Dictionary<UnitType, UnitKey>()
    {
        { UnitType.ARCHER, UnitKey.UNDEADARCHER } ,
        { UnitType.TANK, UnitKey.GIANT} ,
        { UnitType.CAVALRY, UnitKey.RIDER} ,
        { UnitType.MAGIC, UnitKey.LICH },
        { UnitType.FOOTMAN, UnitKey.MINISKELETON },
        { UnitType.HERO, UnitKey.UNDEADHERO },
        { UnitType.QUEEN, UnitKey.UNDEADQUEEN },
        { UnitType.KING, UnitKey.UNDEADKING },
        { UnitType.WALL, UnitKey.UNDEADWALL },
        { UnitType.BARRACK, UnitKey.UNDEADBARRACK },
        { UnitType.TOWER, UnitKey.UNDEADTOWER },
        { UnitType.CATAPULT, UnitKey.UNDEADCATAPULT },
        { UnitType.SIEGE, UnitKey.UNDEADSIEGE },
        { UnitType.TRAP , UnitKey.UNDEADSPIKETRAP },
        { UnitType.BEACON , UnitKey.UNDEADBEACON }
    };
    public static Dictionary<UnitType, UnitKey> GodTypeKey = new Dictionary<UnitType, UnitKey>()
    {
        { UnitType.ARCHER, UnitKey.GODARCHER } ,
        { UnitType.TANK, UnitKey.GODKNIGHT} ,
        { UnitType.CAVALRY, UnitKey.GODCAVALRY} ,
        { UnitType.MAGIC, UnitKey.GODMAGE },
        { UnitType.FOOTMAN, UnitKey.GODSPEARMAN },
        { UnitType.QUEEN, UnitKey.LOKI },
        { UnitType.HERO, UnitKey.THOR },
        { UnitType.KING, UnitKey.ODIN },
        { UnitType.WALL, UnitKey.GODWALL },
        { UnitType.BARRACK, UnitKey.GODBARRACK },
        { UnitType.TOWER, UnitKey.GODTOWER },
        { UnitType.CATAPULT, UnitKey.GODCATAPULT },
        { UnitType.SIEGE, UnitKey.GODSIEGE },
        { UnitType.TRAP , UnitKey.GODSPIKETRAP },
        { UnitType.BEACON , UnitKey.GODBEACON }
    };
    public static Dictionary<UnitType, UnitKey> ElfTypeKey = new Dictionary<UnitType, UnitKey>()
    {
        { UnitType.ARCHER, UnitKey.ELFRANGER } ,
        { UnitType.TANK, UnitKey.ELFGOLEM} ,
        { UnitType.CAVALRY, UnitKey.ELFCAVALRY} ,
        { UnitType.MAGIC, UnitKey.ELFMAGE },
        { UnitType.FOOTMAN, UnitKey.ELFFOOTMAN },
        { UnitType.HERO, UnitKey.ELFDEMONHUNTER },
        { UnitType.QUEEN, UnitKey.ELFQUEEN },
        { UnitType.KING, UnitKey.ELFTREEANT },
        { UnitType.WALL, UnitKey.ELFWALL },
        { UnitType.BARRACK, UnitKey.ELFBARRACK },
        { UnitType.TOWER, UnitKey.ELFTOWER },
        { UnitType.CATAPULT, UnitKey.ELFCATAPULT },
        { UnitType.SIEGE, UnitKey.ELFSIEGE },
        { UnitType.TRAP , UnitKey.ELFSPIKETRAP },
        { UnitType.BEACON , UnitKey.ELFBEACON }
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
        { UnitKey.MULAN , UnitType.QUEEN},
        { UnitKey.KING , UnitType.KING },
        { UnitKey.HUMANWALL , UnitType.WALL },
        { UnitKey.HUMANTOWER , UnitType.TOWER },
        { UnitKey.HUMANBARRACK , UnitType.BARRACK },
        { UnitKey.HUMANCATAPULT , UnitType.CATAPULT },
        { UnitKey.HUMANSPIKETRAP , UnitType.TRAP },
        { UnitKey.HUMANSIEGE , UnitType.SIEGE },
        { UnitKey.HUMANBEACON , UnitType.BEACON },

        { UnitKey.UNDEADARCHER , UnitType.ARCHER   } ,
        { UnitKey.GIANT , UnitType.TANK  } ,
        { UnitKey.RIDER , UnitType.CAVALRY  } ,
        { UnitKey.LICH , UnitType.MAGIC },
        { UnitKey.MINISKELETON , UnitType.FOOTMAN  },
        { UnitKey.UNDEADHERO , UnitType.HERO  },
        { UnitKey.UNDEADQUEEN , UnitType.QUEEN  },
        { UnitKey.UNDEADKING , UnitType.KING  },
        { UnitKey.UNDEADWALL , UnitType.WALL },
        { UnitKey.UNDEADTOWER , UnitType.TOWER },
        { UnitKey.UNDEADBARRACK , UnitType.BARRACK },
        { UnitKey.UNDEADCATAPULT , UnitType.CATAPULT },
        { UnitKey.UNDEADSPIKETRAP , UnitType.TRAP },
        { UnitKey.UNDEADSIEGE , UnitType.SIEGE },
        { UnitKey.UNDEADBEACON , UnitType.BEACON },

        { UnitKey.GODARCHER , UnitType.ARCHER } ,
        { UnitKey.GODKNIGHT , UnitType.TANK } ,
        { UnitKey.GODCAVALRY , UnitType.CAVALRY} ,
        { UnitKey.GODMAGE , UnitType.MAGIC},
        { UnitKey.GODSPEARMAN , UnitType.FOOTMAN},
        { UnitKey.THOR , UnitType.HERO},
        { UnitKey.LOKI , UnitType.QUEEN},
        { UnitKey.ODIN , UnitType.KING },
        { UnitKey.GODWALL , UnitType.WALL },
        { UnitKey.GODTOWER , UnitType.TOWER },
        { UnitKey.GODBARRACK , UnitType.BARRACK },
        { UnitKey.GODCATAPULT , UnitType.CATAPULT },
        { UnitKey.GODSPIKETRAP , UnitType.TRAP },
        { UnitKey.GODSIEGE , UnitType.SIEGE },
        { UnitKey.GODBEACON , UnitType.BEACON },

        { UnitKey.ELFRANGER , UnitType.ARCHER } ,
        { UnitKey.ELFGOLEM , UnitType.TANK } ,
        { UnitKey.ELFCAVALRY , UnitType.CAVALRY} ,
        { UnitKey.ELFMAGE , UnitType.MAGIC},
        { UnitKey.ELFFOOTMAN , UnitType.FOOTMAN},
        { UnitKey.ELFQUEEN , UnitType.QUEEN},
        { UnitKey.ELFDEMONHUNTER , UnitType.HERO},
        { UnitKey.ELFTREEANT , UnitType.KING },
        { UnitKey.ELFWALL , UnitType.WALL },
        { UnitKey.ELFTOWER , UnitType.TOWER },
        { UnitKey.ELFBARRACK , UnitType.BARRACK },
        { UnitKey.ELFCATAPULT , UnitType.CATAPULT },
        { UnitKey.ELFSPIKETRAP , UnitType.TRAP },
        { UnitKey.ELFSIEGE , UnitType.SIEGE },
        { UnitKey.ELFBEACON , UnitType.BEACON }
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
        { UnitKey.HUMANSIEGE , Race.HUMAN },
        { UnitKey.HUMANSPIKETRAP , Race.HUMAN },
        { UnitKey.HUMANBEACON , Race.HUMAN },

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
        { UnitKey.UNDEADSIEGE , Race.UNDEAD },
        { UnitKey.UNDEADSPIKETRAP , Race.UNDEAD },
        { UnitKey.UNDEADBEACON , Race.UNDEAD },

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
        { UnitKey.GODSIEGE , Race.GOD },
        { UnitKey.GODSPIKETRAP , Race.GOD },
        { UnitKey.GODBEACON , Race.GOD },

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
        { UnitKey.ELFSIEGE , Race.ELF },
        { UnitKey.ELFSPIKETRAP , Race.ELF },
        { UnitKey.ELFBEACON , Race.ELF }
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
        { UnitKey.HUMANSPIKETRAP , WeaponType.NOTHING },
        { UnitKey.HUMANSIEGE , WeaponType.SIEGE },
        { UnitKey.HUMANBEACON , WeaponType.NOTHING },

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
        { UnitKey.UNDEADSPIKETRAP , WeaponType.NOTHING },
        { UnitKey.UNDEADSIEGE , WeaponType.SIEGE },
        { UnitKey.UNDEADBEACON , WeaponType.NOTHING },

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
        { UnitKey.GODSPIKETRAP , WeaponType.NOTHING },
        { UnitKey.GODSIEGE , WeaponType.SIEGE },
        { UnitKey.GODBEACON , WeaponType.NOTHING },

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
        { UnitKey.ELFCATAPULT , WeaponType.CANNON },
        { UnitKey.ELFSPIKETRAP , WeaponType.NOTHING },
        { UnitKey.ELFSIEGE , WeaponType.SIEGE },
        { UnitKey.ELFBEACON , WeaponType.NOTHING }

    };
    public static Dictionary<UnitType, UnitSkill> UnitTypeSkillOne = new Dictionary<UnitType, UnitSkill>()
    {
        { UnitType.ARCHER, UnitSkill.NOTHING } ,
        { UnitType.MAGIC, UnitSkill.TORNADO} ,
        { UnitType.CAVALRY, UnitSkill.CHARGE } ,
        { UnitType.FOOTMAN, UnitSkill.NOTHING },
        { UnitType.TANK, UnitSkill.PROVOKE },
        { UnitType.SIEGE, UnitSkill.NOTHING }
    };
    public static Dictionary<UnitType, UnitSkill> UnitTypeSkillTwo = new Dictionary<UnitType, UnitSkill>()
    {
        { UnitType.ARCHER, UnitSkill.VOLLEY } ,
        { UnitType.MAGIC, UnitSkill.HEAL} ,
        { UnitType.CAVALRY, UnitSkill.DASH } ,
        { UnitType.FOOTMAN, UnitSkill.SNEAK },
        { UnitType.TANK, UnitSkill.PROVOKE },
        { UnitType.SIEGE, UnitSkill.NOTHING }
    };
    public static Dictionary<UnitType, UnitSkill> UnitTypeSkillThree = new Dictionary<UnitType, UnitSkill>()
    {
        { UnitType.ARCHER, UnitSkill.SLOW } ,
        { UnitType.MAGIC, UnitSkill.TORNADO} ,
        { UnitType.CAVALRY, UnitSkill.CHARGE } ,
        { UnitType.FOOTMAN, UnitSkill.SCALE },
        { UnitType.TANK, UnitSkill.SHIELD },
        { UnitType.SIEGE, UnitSkill.NOTHING }
    };
    public static Dictionary<int, Dictionary<UnitType, UnitSkill>> UnitStarSkill = new Dictionary<int, Dictionary<UnitType, UnitSkill>>()
    {
        {1 , UnitTypeSkillOne },
        {2 , UnitTypeSkillTwo },
        {3 , UnitTypeSkillThree } 
    };

    public static HashSet<UnitType> BuildingUnit = new HashSet<UnitType>()
    {
        UnitType.TOWER, UnitType.WALL, UnitType.BARRACK, UnitType.CATAPULT, UnitType.TRAP, UnitType.BEACON
    };

    public static Dictionary<UnitType, int> TeamUnitType = new Dictionary<UnitType, int>()
    {
        { UnitType.KING, 0 },
        { UnitType.QUEEN, 1 },
        { UnitType.HERO, 2 }
    };

    public static Dictionary<int, UnitType> CharacterUnitType = new Dictionary<int, UnitType>()
    {
        { 0, UnitType.ALL },
        { 1, UnitType.KING },
        { 2, UnitType.ARCHER },
        { 3, UnitType.TANK },
        { 4, UnitType.MAGIC },
        { 5, UnitType.CAVALRY },
        { 6, UnitType.FOOTMAN },
        { 7, UnitType.SIEGE }
    };
}
/*
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('ARCHER',30,6,2,1,3,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('KNIGHT',150,40,2,1,2,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('MAGE',80,15,3,1,3,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('CAVALRY',70,25,2,1,10,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('SPEARMAN',15,15,1,1,2,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('HERO',500,10,0.6,1,3,180,"LIGHTNING","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('MULAN',800,5,0.6,1,3,180,"LIGHTNING","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('MINISKELETON',15,15,1,1,2,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('GIANT',150,40,2,1,2,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('KING',1000,20,1,1,4,0,"SHIELD","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('UNDEADHERO',500,10,0.6,0,3,0,"LIGHTNING","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('UNDEADQUEEN',800,5,0.6,1,3,180,"LIGHTNING","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('UNDEADARCHER',30,6,2,0,3,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('UNDEADKING',1000,20,1,0,3,0,"SHIELD","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('RIDER',70,25,2,0,10,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('LICH',80,15,3,0,3,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('GODARCHER',30,6,2,2,2,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('GODKNIGHT',150,40,2,2,2,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('GODMAGE',80,15,3,2,3,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('GODCAVALRY',70,25,2,2,10,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('GODSPEARMAN',15,15,1,2,4,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('THOR',500,10,0.6,2,4,180,"STUN","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('LOKI',800,5,0.6,2,4,180,"SHIELD","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('ODIN',1000,20,1,2,5,0,"LIGHTNING","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('GODWALL',100,0,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('ELFWALL',100,0,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('HUMANWALL',100,0,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('UNDEADWALL',100,0,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('ELFRANGER',30,6,2,2,2,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('ELFGOLEM',150,40,2,2,2,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('ELFMAGE',80,15,3,2,3,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('ELFCAVALRY',70,25,2,2,10,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('ELFFOOTMAN',15,15,1,2,4,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('ELFDEMONHUNTER',500,10,0.6,2,4,180,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('ELFQUEEN',800,5,0.6,1,3,180,"SLASH","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('ELFTREEANT',1000,2,0.6,2,4,180,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('GODBARRACK',100,0,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('ELFBARRACK',100,0,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('HUMANBARRACK',100,0,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('UNDEADBARRACK',100,0,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('GODTOWER',100,0,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('ELFTOWER',100,0,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('HUMANTOWER',100,0,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('UNDEADTOWER',100,0,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('GODCATAPULT',100,0,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('ELFCATAPULT',100,0,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('HUMANCATAPULT',100,0,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('UNDEADCATAPULT',100,0,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('GODSPIKETRAP',100,50,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('ELFSPIKETRAP',100,50,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('HUMANSPIKETRAP',100,50,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('UNDEADSPIKETRAP',100,50,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('GODSIEGE',100,150,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('ELFSIEGE',100,150,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('HUMANSIEGE',100,150,0,0,0,0,"","");
insert into cardstat (cardkey,health,attack,repeatattackdelay,defense,speed,special,specialkey,passivekey) values ('UNDEADSIEGE',100,150,0,0,0,0,"","");
*/