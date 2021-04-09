using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialAttackDict : MonoBehaviour
{
    [SerializeField] public Sprite[] sprite ;
    [SerializeField] public Sprite[] childSprite;
    public enum SpecialAttackType { Slash, Shield, Stun, Lightling };
    public static Dictionary<SpecialAttackType, Sprite> SpSprite = new Dictionary<SpecialAttackType, Sprite>()
    {

    };
    public static Dictionary<SpecialAttackType, Sprite> ChildSpSprite = new Dictionary<SpecialAttackType, Sprite>()
    {

    };
    private void Start()
    {
        SpSprite.Clear();
        SpSprite.Add(SpecialAttackType.Slash, sprite[0]);
        SpSprite.Add(SpecialAttackType.Shield, sprite[1]);
        SpSprite.Add(SpecialAttackType.Stun, sprite[2]);
        SpSprite.Add(SpecialAttackType.Lightling, sprite[3]);


        ChildSpSprite.Add(SpecialAttackType.Slash, childSprite[0]);
        ChildSpSprite.Add(SpecialAttackType.Shield, childSprite[1]);
        ChildSpSprite.Add(SpecialAttackType.Stun, childSprite[2]);
        ChildSpSprite.Add(SpecialAttackType.Lightling, childSprite[3]);

    }
}
