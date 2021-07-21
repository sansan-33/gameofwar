using System;
using System.Collections.Generic;
using UnityEngine;

public class SpTypeArt : MonoBehaviour
{
    [Serializable]
    public struct SpTypeImage
    {
        public SpecialAttackDict.SpecialAttackType name;
        public Sprite image;
    }
    public SpTypeImage[] SpTypeArtImages;
    public Dictionary<string, SpTypeImage> SpTypeArtDictionary = new Dictionary<string, SpTypeImage>();

    private void Start()
    {
        initDictionary();
    }
    public void initDictionary()
    {
        SpTypeArtDictionary.Clear();
        foreach (SpTypeImage image in SpTypeArtImages)
        {
            SpTypeArtDictionary.Add(image.name.ToString(), image);
            //Debug.Log($"SpTypeArtDictionary name added {image.name.ToString()} ");
        }
    }
}
