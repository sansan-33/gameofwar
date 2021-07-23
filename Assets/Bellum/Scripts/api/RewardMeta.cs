
using System.Collections.Generic;

public class RewardMeta
{
    public static Dictionary<string, int> missionExp = new Dictionary<string, int>() {
        { "1-1", 4 }, { "1-2", 8 }, { "1-3", 12 }, { "1-4", 16 }, { "1-5", 20 },
        { "2-1", 4 }, { "2-2", 8 }, { "2-3", 12 }, { "2-4", 16 }, { "2-5", 20 },
        { "3-1", 4 }, { "3-2", 8 }, { "3-3", 12 }, { "3-4", 16 }, { "3-5", 20 },
        { "4-1", 4 }, { "4-2", 8 }, { "4-3", 12 }, { "4-4", 16 }, { "4-5", 20 }
    };

    public static Dictionary<string, int> missionGold = new Dictionary<string, int>() {
        { "1-1", 4 }, { "1-2", 8 }, { "1-3", 12 }, { "1-4", 16 }, { "1-5", 100 },
        { "2-1", 4 }, { "2-2", 8 }, { "2-3", 12 }, { "2-4", 16 }, { "2-5", 200 },
        { "3-1", 4 }, { "3-2", 8 }, { "3-3", 12 }, { "3-4", 16 }, { "3-5", 300 },
        { "4-1", 4 }, { "4-2", 8 }, { "4-3", 12 }, { "4-4", 16 }, { "4-5", 400 }
    };
    public static Dictionary<string, string> missionTreasure = new Dictionary<string, string>() {
        { "1-1", "ruby-100" }, { "1-2", "opal-100" }, { "1-3", "emerald-50" }, { "1-4", "sapphire-40" }, { "1-5", "topaz-10" },
        { "2-1", "ruby-100" }, { "2-2", "opal-100" }, { "2-3", "emerald-50" }, { "2-4", "sapphire-40" }, { "2-5", "topaz-10" },
        { "3-1", "ruby-100" }, { "3-2", "opal-100" }, { "3-3", "emerald-50" }, { "3-4", "sapphire-40" }, { "3-5", "topaz-10" },
        { "4-1", "ruby-100" }, { "4-2", "opal-100" }, { "4-3", "emerald-50" }, { "4-4", "sapphire-40" }, { "4-5", "topaz-10" }
    };
}
