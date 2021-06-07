using System;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class MissionButton : MonoBehaviour, IPointerDownHandler 
{
    [SerializeField] public string mission = null;
    [SerializeField] public GameObject loadingPanel = null;
    [SerializeField] public GameObject stagePanel = null;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"MissionButton clicked, chapter {StaticClass.Chapter} - {mission}");
        StaticClass.Mission = mission;
        StaticClass.enemyRace = (UnitMeta.Race) (Int32.Parse(StaticClass.Chapter) - 1);
        stagePanel.SetActive(false);
        loadingPanel.SetActive(true);
        NetworkClient.connection.identity.GetComponent<RTSPlayer>().CmdStartMission(StaticClass.Chapter, mission);

    }
}

