using System;
using System.Collections.Generic;

public class ChapterMissionMeta
{
    public enum Chapter { HUMAN,UNDEAD,ELF, GOD };
    public enum Mission { EASY,NORMAL, HARD, EXTREME };
    public static Dictionary<string, string> ChapterMissionTeam = new Dictionary<string, string>()
    {
        { "1-1","KING,HERO,MULAN" },
        { "1-2","KING,HERO,MULAN" },
        { "1-3","KING,HERO,MULAN" },
        { "1-4","KING,HERO,MULAN" },
        { "2-1","UNDEADKING,UNDEADHERO,UNDEADQUEEN" },
        { "2-2","UNDEADKING,UNDEADHERO,UNDEADQUEEN"  },
        { "2-3","UNDEADKING,UNDEADHERO,UNDEADQUEEN"  },
        { "2-4","UNDEADKING,UNDEADHERO,UNDEADQUEEN"  },
        { "3-1","ELFTREEANT,ELFDEMONHUNTER,ELFQUEEN" },
        { "3-2","ELFTREEANT,ELFDEMONHUNTER,ELFQUEEN" },
        { "3-3","ELFTREEANT,ELFDEMONHUNTER,ELFQUEEN" },
        { "3-4","ELFTREEANT,ELFDEMONHUNTER,ELFQUEEN" },
        { "4-1","ODIN,LOKI,THOR" },
        { "4-2","ODIN,LOKI,THOR" },
        { "4-3","ODIN,LOKI,THOR" },
        { "4-4","ODIN,LOKI,THOR" }
    };
}
