using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitSkillArt : MonoBehaviour
{
    [Serializable]
    public struct UnitSkillImage
    {
        public UnitMeta.UnitSkill name;
        public Sprite image;
    }
    public UnitSkillImage[] UnitSkillImages;
    public Dictionary<string, UnitSkillImage> UnitSkillImageDictionary = new Dictionary<string, UnitSkillImage>();

    private void Start()
    {
        initDictionary();
    }
    public void initDictionary()
    {
        UnitSkillImageDictionary.Clear();
        foreach (UnitSkillImage image in UnitSkillImages)
        {
            UnitSkillImageDictionary.Add(image.name.ToString(), image);
        }
    }
}
