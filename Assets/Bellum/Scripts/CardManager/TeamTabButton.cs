using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System;

public class TeamTabButton : MonoBehaviour
{

    public Button buttonComponent;
    public GameObject tabFocus;
    public int tabID;
    public static event Action<string> TeamTabChanged;

    // Use this for initialization
    void Start()
    {
        buttonComponent.onClick.AddListener(HandleClick);
    }

    public void HandleClick()
    {
        FocusTab();
        StaticClass.SelectedTeamSlot = tabID;
        TeamTabChanged?.Invoke(tabID.ToString());
    }

    public void FocusTab()
    {
        tabFocus.transform.parent = transform;
        tabFocus.transform.position = new Vector3(transform.position.x, transform.position.y - 50, transform.position.z); // Because Card Slot Button in Horiztional Layout with padding bottom 50
        tabFocus.SetActive(true);
    }
}
