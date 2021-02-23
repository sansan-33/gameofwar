using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UnitButton : MonoBehaviour
{

    public Toggle buttonComponent;
    public Text nameLabel;
    public Image iconImage;
    public Text unitId;
    public Text unitTag;

    private Item item;
    private LeaderScrollList scrollList;

    // Use this for initialization
    void Start()
    {
        buttonComponent.onValueChanged.AddListener(HandleClick);
    }

    public void Setup(Item currentItem, LeaderScrollList currentScrollList)
    {
        item = currentItem;
        nameLabel.text = item.name;
        unitTag.text = item.unitType;
        iconImage.sprite = item.icon;
        unitId.text = item.id.ToString();
        scrollList = currentScrollList;
    }
    public void HandleClick(bool state)
    {
        scrollList.TrySelectLeader(item);
    }
}
