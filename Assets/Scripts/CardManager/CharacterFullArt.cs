using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFullArt : MonoBehaviour
{
    [Serializable]
    public struct CharacterFullImage
    {
        public UnitMeta.UnitKey name;
        public Sprite image;
    }
    public CharacterFullImage[] CharacterFullArtImages;
    public Dictionary<string, CharacterFullImage> CharacterFullArtDictionary = new Dictionary<string, CharacterFullImage>();

    private void Start()
    {
        initDictionary();
    }
    private void initDictionary()
    {
        CharacterFullArtDictionary.Clear();
        foreach (CharacterFullImage image in CharacterFullArtImages)
        {
            CharacterFullArtDictionary.Add(image.name.ToString(), image);
            //Debug.Log($"CharacterArtDictionary name added {image.name.ToString()} ");
        }
    }
}
