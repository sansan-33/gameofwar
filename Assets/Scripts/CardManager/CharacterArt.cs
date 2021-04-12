using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterArt : MonoBehaviour
{
    [Serializable]
    public struct CharacterImage
    {
        public UnitMeta.UnitKey name;
        public Sprite image;
    }
    public CharacterImage[] CharacterArtImages;
    public Dictionary<string, CharacterImage> CharacterArtDictionary = new Dictionary<string, CharacterImage>();

    private void Start()
    {
        initDictionary();
    }
    public void initDictionary()
    {
        CharacterArtDictionary.Clear();
        foreach (CharacterImage image in CharacterArtImages)
        {
            CharacterArtDictionary.Add(image.name.ToString(), image);
            //Debug.Log($"CharacterArtDictionary name added {image.name.ToString()} ");
        }
    }
}
