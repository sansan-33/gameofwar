using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialAttackDict : MonoBehaviour
{
    //[SerializeField] public Sprite[] sprite ;
    [SerializeField] public Sprite[] childSprite;
    public enum SpecialAttackType { SLASH, SHIELD, STUN, LIGHTNING, ICE, METEOR, FIREARROW, TORNADO, ZAP, FREEZE};
  

    public static Dictionary<UnitMeta.UnitKey, SpecialAttackType[]> unitSp = new Dictionary<UnitMeta.UnitKey, SpecialAttackType[]>()
    {
        {UnitMeta.UnitKey.ARCHER, new[]{SpecialAttackType.ICE, SpecialAttackType.LIGHTNING } }
    } ;
    public static Dictionary<string, Dictionary<UnitMeta.UnitKey, SpecialAttackType[]>> userSp = new Dictionary<string, Dictionary<UnitMeta.UnitKey, SpecialAttackType[]>>()
    {
        {"a",unitSp}

    };
    [SerializeField] public static Dictionary<SpecialAttackType, Sprite> SpSprite = new Dictionary<SpecialAttackType, Sprite>()
    {
        
    };
 
    public static Dictionary<SpecialAttackType, Sprite> ChildSpSprite = new Dictionary<SpecialAttackType, Sprite>()
    {

    };
    public static Dictionary<SpecialAttackType, bool> NeedCameraShake = new Dictionary<SpecialAttackType, bool>()
    {
        {SpecialAttackType.FIREARROW, false },
        {SpecialAttackType.TORNADO, false },
        {SpecialAttackType.METEOR, true },
        {SpecialAttackType.ZAP, true },
    };
    public static Dictionary<SpecialAttackType, float> RangeScale = new Dictionary<SpecialAttackType, float>()
    {
        {SpecialAttackType.ZAP, 0.5f },
        {SpecialAttackType.FIREARROW, 1 },
        {SpecialAttackType.METEOR, 1.5f },
        {SpecialAttackType.TORNADO, 2 },
    };
    private void Start()
    {
        /*SpSprite.Clear();
        SpSprite.Add(SpecialAttackType.Slash, sprite[0]);
        SpSprite.Add(SpecialAttackType.Shield, sprite[1]);
        SpSprite.Add(SpecialAttackType.Stun, sprite[2]);
        SpSprite.Add(SpecialAttackType.Lightling, sprite[3]);
        SpSprite.Add(SpecialAttackType.Ice, sprite[4]);*/

        ChildSpSprite.Clear();
        ChildSpSprite.Add(SpecialAttackType.SLASH, childSprite[0]);
        ChildSpSprite.Add(SpecialAttackType.SHIELD, childSprite[1]);
        ChildSpSprite.Add(SpecialAttackType.STUN, childSprite[2]);
        ChildSpSprite.Add(SpecialAttackType.LIGHTNING, childSprite[3]);
        ChildSpSprite.Add(SpecialAttackType.ICE, childSprite[4]);
    }
    public void SetUnitSp(string Id, UnitMeta.UnitKey unitKey, SpecialAttackType[] specialAttackTypes)
    {
         userSp.Add(Id, new Dictionary<UnitMeta.UnitKey, SpecialAttackType[]>() { { unitKey, specialAttackTypes } });
    }
}
