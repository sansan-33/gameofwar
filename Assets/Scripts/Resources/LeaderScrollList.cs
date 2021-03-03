using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Item
{
    public string name;
    public Sprite icon;
    public int id = 1;
    public string unitType;
}

public class LeaderScrollList : MonoBehaviour
{

    public List<Item> itemList;
    public Transform contentPanel;
    public Text myDisplay;
    public SimpleObjectPool buttonObjectPool;
    public List<Sprite> icons;
    public static event Action<int> LeaderSelected;
    public TacticalBehavior tb;
    private int selectedLeaderIndex = 0;
    private string selectedLeaderName;
    public ToggleGroup toggleGroup;

    // Use this for initialization
    void Start()
    {
        RefreshDisplay();
        TacticalBehavior.LeaderUpdated += TacticalStatusToItem;
    }
    private void OnDestroy()
    {
        TacticalBehavior.LeaderUpdated -= TacticalStatusToItem;
    }
    void RefreshDisplay()
    {
        RefreshHeader();
        RemoveButtons();
        AddButtons();
    }
    void RefreshHeader()
    {
        myDisplay.text = "Selected Leader: " + (selectedLeaderIndex + 1).ToString() + " " + selectedLeaderName;
    }
    private void RemoveButtons()
    {
        while (contentPanel.childCount > 0)
        {
            GameObject toRemove = transform.GetChild(0).gameObject;
            buttonObjectPool.ReturnObject(toRemove);
        }
    }

    private void AddButtons()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            Item item = itemList[i];
            GameObject newButton = buttonObjectPool.GetObject();
            newButton.transform.SetParent(contentPanel);

            UnitButton unitButton = newButton.GetComponent<UnitButton>();
            unitButton.Setup(item, this);
            if (i == selectedLeaderIndex) { newButton.GetComponent<Toggle>().isOn = true; selectedLeaderIndex = item.id; RefreshHeader(); }
            newButton.GetComponent<Toggle>().group = toggleGroup;
        }
    }
    public void TrySelectLeader(Item item)
    {
        selectedLeaderIndex = item.id;
        selectedLeaderName = item.unitType;
        RefreshHeader();
        LeaderSelected?.Invoke(selectedLeaderIndex);

    }
    public void TryTransferItemToOtherShop(Item item)
    {
        
    }
    void TacticalStatusToItem(Dictionary<int, GameObject> leaders)
    {
        itemList.Clear();
        Item item;
        foreach (var leader in leaders)
        {
            item = new Item();
            item.id = (int)leader.Value.GetComponent<Unit>().unitType;
            item.name = leader.Value.name;
            item.icon = icons[(int)leader.Value.GetComponent<Unit>().unitType];
            item.unitType = leader.Value.GetComponent<Unit>().unitType.ToString();
            itemList.Add(item);
        }
        RefreshDisplay();
    }
    void AddItem(Item itemToAdd, LeaderScrollList leaderList)
    {
        leaderList.itemList.Add(itemToAdd);
    }

    private void RemoveItem(Item itemToRemove, LeaderScrollList leaderList)
    {
        for (int i = leaderList.itemList.Count - 1; i >= 0; i--)
        {
            if (leaderList.itemList[i] == itemToRemove)
            {
                leaderList.itemList.RemoveAt(i);
            }
        }
    }
}
