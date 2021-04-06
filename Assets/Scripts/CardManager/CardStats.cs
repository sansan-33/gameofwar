using UnityEngine;

public class CardStats : MonoBehaviour
{
    public int cardLevel;
    public int health;
    public int attack;
    public float repeatAttackDelay;
    public int speed;
    public int defense;
    public int special;
    public CardStats() { }
    public CardStats(int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special)
    {
        SetCardStats(cardLevel,health,attack,repeatAttackDelay,speed,defense,special);
    }
    public void SetCardStats(int cardLevel, int health, int attack, float repeatAttackDelay, int speed, int defense, int special)
    {
        this.cardLevel = cardLevel;
        this.health = health;
        this.attack = attack;
        this.repeatAttackDelay = repeatAttackDelay;
        this.speed = speed;
        this.defense = defense;
        this.special = special;
    }
    public override string ToString()
    {
        return "health:" + health + "\t attack:" + attack + "\t repeatAttackDelay:" + repeatAttackDelay + "\t speed:" + speed + "\t defense:" + defense + "\t special:" + special;
    }
}