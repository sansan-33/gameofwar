using System;
public class UserCard
{
    public string cardkey;
    public string level;
    public string exp;
    public string special;
    public string rarity;
    public string leveluprequirement;
    public string star;
    public string unittype;

    public UserCard(string local_cardkey, string local_level, string local_exp, string local_special, string local_rarity, string local_leveluprequirement, string local_star, string local_unittype)
    {
        cardkey = local_cardkey;
        level = local_level;
        exp = local_exp;
        special = local_special;
        rarity = local_rarity;
        leveluprequirement = local_leveluprequirement;
        star = local_star;
        unittype = local_unittype;
    }
    public override string ToString()
    {
        return "cardkey: " + cardkey + ",level: " + level + ",exp: " + exp + ",special: " + special + ",rarity: " + rarity + ",leveluprequirement: " + leveluprequirement + ",star: " + star + ",unittype: " + unittype;
    }
}

