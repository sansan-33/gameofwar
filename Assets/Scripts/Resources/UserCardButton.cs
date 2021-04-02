using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UserCardButton : MonoBehaviour
{

    public Button buttonComponent;
    public Image characterImage;
    public GameObject lockImage;
    public GameObject cardGlow;
    public GameObject userLevelBar;
    public GameObject levelBadge;
    public GameObject star;
    public GameObject userCardFocus;
    public Image unitTypeImage;

    public string cardkey;
    public string cardtype;
    public Slider levelSlider;
    public TMP_Text level;
    public TMP_Text exp;
    public TMP_Text rarity;
    public TMP_Text leveluprequirement;
    public bool IS_TEAM_MEMBER_SELECTION = false;
    public GameObject cardSlotParent;


    // Use this for initialization
    void Start()
    {
        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(HandleClick);
    }
    public void HandleClick()
    {
        StaticClass.CrossSceneInformation = cardkey;
        if (IS_TEAM_MEMBER_SELECTION && userCardFocus != null) {
            userCardFocus.transform.parent = transform;
            userCardFocus.transform.position = transform.position; // Because Card Slot Button in Horiztional Layout with padding bottom 50
            userCardFocus.SetActive(true);
            TeamCardButton teamCard = cardSlotParent.transform.GetChild(StaticClass.SelectedCardSlot).GetComponent<TeamCardButton>();
            teamCard.cardSlotEmpty.SetActive(false);
            teamCard.cardSlotKey.text = cardkey;
            teamCard.cardSlotLevel.text = level.text;
            teamCard.unitTypeImage.sprite = unitTypeImage.sprite;
            teamCard.cardSlotType.text = cardtype;
            teamCard.characterImage.sprite = characterImage.sprite;
        }
        else
            SceneManager.LoadScene("Scene_Hero_Menu");
    }
}
