using System;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class StageMenuButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] public string chapter = null;
    [SerializeField] public GameObject missionContentParent = null;
    public static event Action<int> TabClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log($"StageMenuButton OnPointerClick chapter {chapter}");
        StaticClass.Chapter = chapter;
        StaticClass.EventRankingID = chapter;
        int chapterIndex = int.Parse(chapter) - 1;
        var buttons = FindObjectsOfType<StageMenuButton>();
        foreach(StageMenuButton button in buttons)
        {
            button.transform.GetChild(0).gameObject.SetActive(false);
        }
        this.transform.GetChild(0).gameObject.SetActive(true);
        TabClicked?.Invoke(chapterIndex);
        MissionButton[] missions = missionContentParent.GetComponentsInChildren<MissionButton>();
        for(int i=1; i < missions.Length; i++ )
        {
            //Debug.Log(i);
            missions[i-1].desc.text = GamePlayMeta.ArenaLevelTextDict[StaticClass.Chapter + "-" + i];
        }
    }
}

