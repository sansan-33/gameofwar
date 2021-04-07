using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitTypeArt : MonoBehaviour
{
    [Serializable]
    public struct UnitTypeImage
    {
        public UnitMeta.UnitType type;
        public Sprite image;
    }
    public UnitTypeImage[] unitTypeImages;
    public Dictionary<string, UnitTypeImage> UnitTypeArtDictionary = new Dictionary<string, UnitTypeImage>();

    public void Start()
    {
        initDictionary();
    }
    public void initDictionary()
    {
        UnitTypeArtDictionary.Clear();
        foreach (UnitTypeImage image in unitTypeImages)
        {
            UnitTypeArtDictionary.Add(image.type.ToString(), image);
        }
    }
}
