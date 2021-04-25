using System.Collections.Generic;
using UnityEngine;

public class CardStats : MonoBehaviour
{
    public int star;
    public int cardLevel;
    public int health;
    public int attack;
    public float repeatAttackDelay;
    public int speed;
    public int defense;
    public int special;
    public string specialkey;
    public string passivekey;
    [HideInInspector] public SpecialAttackDict.SpecialAttackType specialAttackType = SpecialAttackDict.SpecialAttackType.ICE;

    public CardStats() { }
    public CardStats(int star, int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special = 0, string specialkey ="", string passivekey = "")
    {
        SetCardStats(star, cardLevel, health, attack, repeatAttackDelay, speed, defense, special,  specialkey, passivekey);
    }
    public void SetCardStats(int star, int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special = 0, string specialkey = "", string passivekey = "")
    {
        this.star = star;
        this.cardLevel = cardLevel;
        this.health = health;
        this.attack = attack;
        this.repeatAttackDelay = repeatAttackDelay;
        this.speed = speed;
        this.defense = defense;
        this.specialkey = specialkey;
        this.passivekey = passivekey;
        this.special = special;
    }
    public void SetCardStats(CardStats _cardStats)
    {
        SetCardStats(_cardStats.star, _cardStats.cardLevel, _cardStats.health, _cardStats.attack, _cardStats.repeatAttackDelay, _cardStats.speed, _cardStats.defense, _cardStats.special, _cardStats.specialkey , _cardStats.passivekey);
    }
    public override string ToString()
    {
        return "health:" + health + "\t attack:" + attack + "\t repeatAttackDelay:" + repeatAttackDelay + "\t speed:" + speed + "\t defense:" + defense + "\t special:" + special + "\t specialkey:" + specialkey + "\t passivekey:" + passivekey;
    }
}