using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Localization.Settings;
using System;


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
    public GameObject rarity;
    public Image unitTypeImage;

    public string cardkey;
    public string cardtype;
    public Slider levelSlider;
    public TMP_Text cardname;
    public TMP_Text level;
    public TMP_Text exp;
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

            // Click user card, focus team card by team position
            int teamCardSlot = UnitMeta.TeamUnitType[(UnitMeta.UnitType)Enum.Parse(typeof(UnitMeta.UnitType), cardtype)];
            StaticClass.SelectedCardSlot = teamCardSlot;
            //Debug.Log($"UsercardButton.HandleClick() cardtype:{cardtype} teamCardSlot:{teamCardSlot}");

            TeamCardButton teamCard = cardSlotParent.transform.GetChild(StaticClass.SelectedCardSlot).GetComponent<TeamCardButton>();
            teamCard.cardSlotEmpty.SetActive(false);
            
            teamCard.cardSlotLevel.text = level.text;
            teamCard.unitTypeImage.sprite = unitTypeImage.sprite;
            teamCard.characterImage.sprite = characterImage.sprite;

            teamCard.cardSlotFocus.transform.parent = teamCard.transform;
            teamCard.cardSlotFocus.transform.position = new Vector3(teamCard.transform.position.x, teamCard.transform.position.y + 50, teamCard.transform.position.z); // Because Card Slot Button in Horiztional Layout with padding bottom 50
            teamCard.cardSlotFocus.SetActive(true);

            // Localization
            //teamCard.cardSlotKey.text = cardkey;
            AsyncOperationHandle<string> op = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(LanguageSelectionManager.STRING_TEXT_REF, cardkey.ToLower(), null);
            if (op.IsDone)
            {
                teamCard.cardSlotKey.text = op.Result;
            }
            else
            {
                op.Completed += (o) => teamCard.cardSlotKey.text = o.Result;
            }
            // no need to set font because the card alredy set font in TeamManager

            //teamCard.cardSlotType.text = cardtype;
            op = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(LanguageSelectionManager.STRING_TEXT_REF, cardtype.ToLower(), null);
            if (op.IsDone)
            {
                teamCard.cardSlotType.text = op.Result;
            }
            else
            {
                op.Completed += (o) => teamCard.cardSlotType.text = o.Result;
            }
            // no need to set font because the card alredy set font in TeamManager
            //Debug.Log($"UsercardButton.HandleClick() cardtype:{cardtype} teamCard.cardSlotType.text:{teamCard.cardSlotType.text}");
        }
        else
            SceneManager.LoadScene("Scene_Hero_Menu");
    }
}
