using System;
using System.Collections.Generic;

public class UnitMeta
{

    public class MyUnit
    {
        public UnitKey key;
        public Race race;
        public UnitType type;
        public WeaponType weapontype;

        public MyUnit(UnitKey _key, UnitType _type, Race _race, WeaponType _weapontype)
        {
            this.key = _key;
            this.type = _type;
            this.race = _race;
            this.weapontype = _weapontype;
        }
    }

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
        UNDEADHERO, UNDEADARCHER, UNDEADKING, RIDER, LICH, HUMANWALL, UNDEADWALL,
        ODIN, THOR, LOKI, GODARCHER, GODCAVALRY, GODSPEARMAN, GODMAGE, GODKNIGHT, GODWALL,
        ELFRANGER, ELFCAVALRY, ELFFOOTMAN, ELFMAGE, ELFGOLEM, ELFTREEANT, ELFDEMONHUNTER, ELFWALL, UNDEADQUEEN, ELFQUEEN, MULAN,
        HUMANTOWER, HUMANBARRACK, HUMANCATAPULT, UNDEADTOWER, UNDEADBARRACK, UNDEADCATAPULT, ELFTOWER, ELFBARRACK, ELFCATAPULT, GODTOWER, GODBARRACK, GODCATAPULT, DOOR,
        HUMANSPIKETRAP, HUMANSIEGE, UNDEADSPIKETRAP, UNDEADSIEGE, ELFSPIKETRAP, ELFSIEGE, GODSPIKETRAP, GODSIEGE, HUMANBEACON, ELFBEACON, GODBEACON, UNDEADBEACON
    };
    public enum UnitType { ARCHER, TANK, MAGIC, CAVALRY, FOOTMAN, SIEGE, WALL, TOWER, BARRACK, CATAPULT, HERO, KING, ALL, DOOR, TRAP, QUEEN, BEACON };
    public enum UnitSkill { DASH, SHIELD, HEAL, TORNADO, VOLLEY, SLOW, PROVOKE, CHARGE, SNEAK, SCALE, NOTHING, DEFAULT, ARROWRAIN };
    public enum UnitPosition { FORWARD, MIDFIELDER, DEFENDER, GOALIE, WALL, QUEEN, HERO, SECRET };
    public enum Race { HUMAN, UNDEAD, ELF, GOD, ALL };
    public enum SpeedType { ORIGINAL, CURRENT, MAX };
    public enum WeaponType { THSWORD, SHSWORD, BOW, HAMMER, SPEAR, DAGGER, SPELL, AXE, LANCE, PUNCH, NOTHING, CANNON, SPAWNER, SIEGE };
    public enum EffectType { ATTACK, DEFENSE, HEALTH, SPEED, FREEZE, STUN, BURN };

    // Target Table
    // Target  | Provoke | Door      | Player    |  King    | Building
    //         | Player  | Provoke   | King      |  King    | Door
    //         | King
    public enum TargetTag { Provoke, Door, Player, King, Building };
    public static Dictionary<TargetTag, List<TargetTag>> TargetGroup = new Dictionary<TargetTag, List<TargetTag>>() {
        { TargetTag.Provoke, new List<TargetTag> { TargetTag.Provoke, TargetTag.Player, TargetTag.King } },
        { TargetTag.Door, new List<TargetTag> { TargetTag.Door, TargetTag.Provoke} },
        { TargetTag.Player, new List<TargetTag> { TargetTag.Player, TargetTag.King } },
        { TargetTag.King, new List<TargetTag> { TargetTag.King } },
        { TargetTag.Building, new List<TargetTag> { TargetTag.Building, TargetTag.Door } }
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

    public static Dictionary<UnitKey, MyUnit> UnitStruct = new Dictionary<UnitKey, MyUnit>() {
        { UnitKey.ARCHER , new MyUnit(UnitKey.ARCHER, UnitType.ARCHER, Race.HUMAN , WeaponType.BOW) },
        { UnitKey.KNIGHT , new MyUnit(UnitKey.KNIGHT, UnitType.TANK, Race.HUMAN , WeaponType.THSWORD) },
        { UnitKey.CAVALRY, new MyUnit(UnitKey.CAVALRY, UnitType.CAVALRY, Race.HUMAN , WeaponType.LANCE) },
        { UnitKey.MAGE , new MyUnit(UnitKey.MAGE, UnitType.MAGIC, Race.HUMAN, WeaponType.SPELL ) },
        { UnitKey.SPEARMAN , new MyUnit(UnitKey.SPEARMAN, UnitType.FOOTMAN, Race.HUMAN, WeaponType.SPEAR ) },
        { UnitKey.HERO , new MyUnit(UnitKey.HERO, UnitType.HERO, Race.HUMAN , WeaponType.SHSWORD) },
        { UnitKey.MULAN , new MyUnit(UnitKey.MULAN, UnitType.QUEEN, Race.HUMAN , WeaponType.SHSWORD) },
        { UnitKey.KING , new MyUnit(UnitKey.KING, UnitType.KING, Race.HUMAN, WeaponType.SHSWORD ) },
        { UnitKey.HUMANWALL , new MyUnit(UnitKey.HUMANWALL, UnitType.WALL, Race.HUMAN , WeaponType.NOTHING) },
        { UnitKey.HUMANTOWER , new MyUnit(UnitKey.HUMANTOWER, UnitType.TOWER, Race.HUMAN, WeaponType.NOTHING ) },
        { UnitKey.HUMANBARRACK , new MyUnit(UnitKey.HUMANBARRACK, UnitType.BARRACK, Race.HUMAN , WeaponType.SPAWNER) },
        { UnitKey.HUMANCATAPULT , new MyUnit(UnitKey.HUMANCATAPULT, UnitType.CATAPULT, Race.HUMAN, WeaponType.CANNON ) },
        { UnitKey.HUMANSPIKETRAP , new MyUnit(UnitKey.HUMANSPIKETRAP, UnitType.TRAP, Race.HUMAN, WeaponType.NOTHING ) },
        { UnitKey.HUMANSIEGE , new MyUnit(UnitKey.HUMANSIEGE, UnitType.SIEGE, Race.HUMAN, WeaponType.SIEGE ) },
        { UnitKey.HUMANBEACON , new MyUnit(UnitKey.HUMANBEACON, UnitType.BEACON, Race.HUMAN , WeaponType.NOTHING) },

        { UnitKey.UNDEADARCHER , new MyUnit(UnitKey.UNDEADARCHER, UnitType.ARCHER, Race.UNDEAD, WeaponType.BOW ) },
        { UnitKey.GIANT , new MyUnit(UnitKey.GIANT, UnitType.TANK, Race.UNDEAD, WeaponType.AXE ) },
        { UnitKey.RIDER , new MyUnit(UnitKey.RIDER, UnitType.CAVALRY, Race.UNDEAD, WeaponType.LANCE ) },
        { UnitKey.LICH , new MyUnit(UnitKey.LICH, UnitType.MAGIC, Race.UNDEAD, WeaponType.SPELL ) },
        { UnitKey.MINISKELETON , new MyUnit(UnitKey.MINISKELETON, UnitType.FOOTMAN, Race.UNDEAD , WeaponType.AXE) },
        { UnitKey.UNDEADHERO , new MyUnit(UnitKey.UNDEADHERO, UnitType.HERO, Race.UNDEAD , WeaponType.SHSWORD) },
        { UnitKey.UNDEADQUEEN , new MyUnit(UnitKey.UNDEADQUEEN, UnitType.QUEEN, Race.UNDEAD , WeaponType.SHSWORD) },
        { UnitKey.UNDEADKING , new MyUnit(UnitKey.UNDEADKING, UnitType.KING, Race.UNDEAD , WeaponType.SHSWORD) },
        { UnitKey.UNDEADWALL , new MyUnit(UnitKey.UNDEADWALL, UnitType.WALL, Race.UNDEAD , WeaponType.NOTHING) },
        { UnitKey.UNDEADTOWER , new MyUnit(UnitKey.UNDEADTOWER, UnitType.TOWER, Race.UNDEAD, WeaponType.NOTHING ) },
        { UnitKey.UNDEADBARRACK , new MyUnit(UnitKey.UNDEADBARRACK, UnitType.BARRACK, Race.UNDEAD , WeaponType.SPAWNER) },
        { UnitKey.UNDEADCATAPULT , new MyUnit(UnitKey.UNDEADCATAPULT, UnitType.CATAPULT, Race.UNDEAD , WeaponType.CANNON) },
        { UnitKey.UNDEADSPIKETRAP , new MyUnit(UnitKey.UNDEADSPIKETRAP, UnitType.TRAP, Race.UNDEAD , WeaponType.NOTHING) },
        { UnitKey.UNDEADSIEGE , new MyUnit(UnitKey.UNDEADSIEGE, UnitType.SIEGE, Race.UNDEAD , WeaponType.SIEGE) },
        { UnitKey.UNDEADBEACON , new MyUnit(UnitKey.UNDEADBEACON, UnitType.BEACON, Race.UNDEAD, WeaponType.NOTHING ) },

        { UnitKey.GODARCHER , new MyUnit(UnitKey.GODARCHER, UnitType.ARCHER, Race.GOD, WeaponType.BOW ) },
        { UnitKey.GODKNIGHT , new MyUnit(UnitKey.GODKNIGHT, UnitType.TANK, Race.GOD , WeaponType.SPEAR) },
        { UnitKey.GODCAVALRY , new MyUnit(UnitKey.GODCAVALRY, UnitType.CAVALRY, Race.GOD , WeaponType.LANCE) },
        { UnitKey.GODMAGE , new MyUnit(UnitKey.GODMAGE, UnitType.MAGIC, Race.GOD , WeaponType.SPELL) },
        { UnitKey.GODSPEARMAN, new MyUnit(UnitKey.GODSPEARMAN, UnitType.FOOTMAN, Race.GOD , WeaponType.SHSWORD) },
        { UnitKey.THOR , new MyUnit(UnitKey.THOR, UnitType.HERO, Race.GOD , WeaponType.HAMMER) },
        { UnitKey.LOKI , new MyUnit(UnitKey.LOKI, UnitType.QUEEN, Race.GOD , WeaponType.DAGGER) },
        { UnitKey.ODIN , new MyUnit(UnitKey.ODIN, UnitType.KING, Race.GOD , WeaponType.SPEAR) },
        { UnitKey.GODWALL , new MyUnit(UnitKey.GODWALL, UnitType.WALL, Race.GOD , WeaponType.NOTHING) },
        { UnitKey.GODTOWER , new MyUnit(UnitKey.GODTOWER, UnitType.TOWER, Race.GOD , WeaponType.NOTHING) },
        { UnitKey.GODBARRACK , new MyUnit(UnitKey.GODBARRACK, UnitType.BARRACK, Race.GOD , WeaponType.SPAWNER) },
        { UnitKey.GODCATAPULT , new MyUnit(UnitKey.GODCATAPULT, UnitType.CATAPULT, Race.GOD , WeaponType.CANNON) },
        { UnitKey.GODSPIKETRAP , new MyUnit(UnitKey.GODSPIKETRAP, UnitType.TRAP, Race.GOD , WeaponType.NOTHING) },
        { UnitKey.GODSIEGE , new MyUnit(UnitKey.GODSIEGE, UnitType.SIEGE, Race.GOD , WeaponType.SIEGE) },
        { UnitKey.GODBEACON, new MyUnit(UnitKey.GODBEACON, UnitType.BEACON, Race.GOD , WeaponType.NOTHING) },

        { UnitKey.ELFRANGER , new MyUnit(UnitKey.ELFRANGER, UnitType.ARCHER, Race.ELF, WeaponType.BOW ) },
        { UnitKey.ELFGOLEM , new MyUnit(UnitKey.ELFGOLEM, UnitType.TANK, Race.ELF , WeaponType.PUNCH) },
        { UnitKey.ELFCAVALRY , new MyUnit(UnitKey.ELFCAVALRY, UnitType.CAVALRY, Race.ELF, WeaponType.LANCE ) },
        { UnitKey.ELFMAGE , new MyUnit(UnitKey.ELFMAGE, UnitType.MAGIC, Race.ELF , WeaponType.SPELL) },
        { UnitKey.ELFFOOTMAN , new MyUnit(UnitKey.ELFFOOTMAN, UnitType.FOOTMAN, Race.ELF , WeaponType.AXE) },
        { UnitKey.ELFQUEEN , new MyUnit(UnitKey.ELFQUEEN, UnitType.QUEEN, Race.ELF , WeaponType.SHSWORD) },
        { UnitKey.ELFDEMONHUNTER , new MyUnit(UnitKey.ELFDEMONHUNTER, UnitType.HERO, Race.ELF, WeaponType.AXE ) },
        { UnitKey.ELFTREEANT, new MyUnit(UnitKey.ELFTREEANT, UnitType.KING, Race.ELF, WeaponType.PUNCH ) },
        { UnitKey.ELFWALL , new MyUnit(UnitKey.ELFWALL, UnitType.WALL, Race.ELF , WeaponType.NOTHING) },
        { UnitKey.ELFTOWER , new MyUnit(UnitKey.ELFTOWER, UnitType.TOWER, Race.ELF , WeaponType.NOTHING) },
        { UnitKey.ELFBARRACK , new MyUnit(UnitKey.ELFBARRACK, UnitType.BARRACK, Race.ELF , WeaponType.SPAWNER) },
        { UnitKey.ELFCATAPULT , new MyUnit(UnitKey.ELFCATAPULT, UnitType.CATAPULT, Race.ELF , WeaponType.CANNON) },
        { UnitKey.ELFSPIKETRAP, new MyUnit(UnitKey.ELFSPIKETRAP, UnitType.TRAP, Race.ELF , WeaponType.NOTHING) },
        { UnitKey.ELFSIEGE , new MyUnit(UnitKey.ELFSIEGE, UnitType.SIEGE, Race.ELF , WeaponType.SIEGE) },
        { UnitKey.ELFBEACON , new MyUnit(UnitKey.ELFBEACON, UnitType.BEACON, Race.ELF , WeaponType.NOTHING) },

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
    public static Dictionary<UnitType, UnitSkill> UnitTypeSkillOne = new Dictionary<UnitType, UnitSkill>()
    {
        { UnitType.ARCHER, UnitSkill.NOTHING } ,
        { UnitType.MAGIC, UnitSkill.NOTHING} ,
        { UnitType.CAVALRY, UnitSkill.NOTHING } ,
        { UnitType.FOOTMAN, UnitSkill.NOTHING },
        { UnitType.TANK, UnitSkill.NOTHING },
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

    public static UnitKey GetUnitKeyByRaceType(Race race, UnitType type)
    {
        UnitKey result = UnitKey.ARCHER;
        foreach (var key in UnitStruct.Keys )
        {
            MyUnit unit = UnitStruct[key];
            if (unit.race == race && unit.type == type)
            {
                result = key ;
                break;
            }
        }
        return result;
    }
}
