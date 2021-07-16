using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class TeamCardButton : MonoBehaviour
{

    public Button buttonComponent;
    public GameObject cardSlotFocus;
    public int cardSlotID;
    public GameObject cardSlotEmpty;
    [SerializeField] public TMP_Text cardSlotKey;
    [SerializeField] public TMP_Text cardSlotType;
    [SerializeField] public TMP_Text cardSlotLevel;
    [SerializeField] public Image unitTypeImage;
    [SerializeField] public UserCard userCard ;
    [SerializeField] public Image characterImage;

    // Use this for initialization
    void Start()
    {
        // focus team card by user card clicked, no need to click team card
        //buttonComponent.onClick.AddListener(HandleClick);
    }
    public void HandleClick()
    {
        cardSlotFocus.transform.parent = transform;
        cardSlotFocus.transform.position = new Vector3(transform.position.x, transform.position.y + 50, transform.position.z); // Because Card Slot Button in Horiztional Layout with padding bottom 50
        cardSlotFocus.SetActive(true);
        StaticClass.SelectedCardSlot = cardSlotID;
    }
}
