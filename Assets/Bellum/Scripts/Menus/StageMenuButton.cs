using System;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Localization.Settings;

public class StageMenuButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] public string chapter = null;
    [SerializeField] public GameObject missionContentParent = null;
    public static event Action<int> TabClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"StageMenuButton OnPointerClick chapter {chapter}");
        StaticClass.Chapter = chapter;
        Debug.Log($"StageMenuButton OnPointerClick StaticClass.Chapter: {StaticClass.Chapter}");
        StaticClass.EventRankingID = chapter;
        int chapterIndex = int.Parse(chapter) - 1;
        Debug.Log($"StageMenuButton OnPointerClick chapterIndex {chapterIndex}");
        var buttons = FindObjectsOfType<StageMenuButton>();
        foreach(StageMenuButton button in buttons)
        {
            button.transform.GetChild(0).gameObject.SetActive(false);
        }
        this.transform.GetChild(0).gameObject.SetActive(true);
        TabClicked?.Invoke(chapterIndex);
        MissionButton[] missions = missionContentParent.GetComponentsInChildren<MissionButton>();

        // change mission 1-5 description
        //Debug.Log($"StageMenuButton OnPointerClick missions.Length: {missions.Length}");
        for (int i=1; i <= missions.Length; i++ )
        {
            //Debug.Log(i);
            //Debug.Log($"{i} StageMenuButton.OnPointerClick() missions[i-1].mission:{missions[i - 1].mission}");
            if (missions[i - 1].desc != null)
            {
                String key ="mission_" + StaticClass.Chapter + "-" + missions[i - 1].mission;
                AsyncOperationHandle<string> op = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(LanguageSelectionManager.STRING_TEXT_REF, key, null);
                if (op.IsDone)
                {
                    missions[i - 1].desc.text = op.Result;
                }
                else
                {
                    op.Completed += (o) => missions[i - 1].desc.text = o.Result;
                }

                //missions[i - 1].desc.text = GamePlayMeta.ArenaLevelTextDict[key];
                //Debug.Log($"{StaticClass.Chapter} - {missions[i - 1].mission} StageMenuButton.OnPointerClick() missions[i-1].desc.text:{missions[i - 1].desc.text}");
            }
        }
    }
}

