using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static CharacterArt;
using static UnitTypeArt;
using static SpTypeArt;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Localization.Settings;

public class UserCardManager : MonoBehaviour
{
    [SerializeField] GameObject userCardPrefab;
    [SerializeField] public CharacterArt Arts;
    [SerializeField] public UnitTypeArt unitTypeArt;
    [SerializeField] public SpTypeArt spTypeArt;
    [SerializeField] bool IS_TEAM_MEMBER_SELECTION;
    [SerializeField] public LocalizationResponder localizationResponder;
    public GameObject userCardFocus;
    public GameObject cardSlotParent;
    public GameObject CharacterTabSlot;
    private string characterNumber = "0";

    public Dictionary<string, UserCard> userCardDict = new Dictionary<string, UserCard>();
    private Dictionary<string, UserCard> allCardDict = new Dictionary<string, UserCard>();
    public static event Action<string> UserCardLoaded;

    public void Start()
    {
        CharacterTabButton.CharacterTabChanged += CharacterChanged;
        StartCoroutine(Populate(StaticClass.UserID));
    }
    
    public void OnDestroy()
    {
        CharacterTabButton.CharacterTabChanged -= CharacterChanged;
    }

    public void CharacterChanged(string local_characternumber)
    {
        
        characterNumber = local_characternumber;
        StartCoroutine(TabChanged(Int32.Parse(characterNumber)));

    }

    IEnumerator TabChanged(int tabID)
    {
        UserCardButton userCard;
        for (int i = 0; i < transform.childCount; i++)
        {
            userCard = transform.GetChild(i).GetComponent<UserCardButton>();

            // get selected character tab id and UnitType, don't show if not same unittype
            if (tabID == 1) // first tab show team characters only
            {
                if (UnitMeta.TeamUnitType.ContainsKey((UnitMeta.UnitType)Enum.Parse(typeof(UnitMeta.UnitType), userCard.cardtype)))
                {
                    userCard.gameObject.SetActive(true);
                }
                else
                {
                    userCard.gameObject.SetActive(false);
                }
            }
            else if (tabID > 1 && userCard.cardtype != UnitMeta.CharacterUnitType[tabID].ToString())
            {
                userCard.gameObject.SetActive(false);
            }
            else
            {
                userCard.gameObject.SetActive(true);
            }

        }
        yield return null;
    }


    IEnumerator Populate(string userid)
    {
        yield return GetAllCard("id");
        yield return GetUserCard(userid);

        GameObject userCard;
        CharacterImage characterImage;
        UserCard card;
        SpTypeImage spTypeImage;

        foreach (var allCard in allCardDict)
        {
            //Debug.Log($"Character Art Card : {allCard}  ");
            if (IS_TEAM_MEMBER_SELECTION)
            {
                // load KING, QUEEN and HERO cards only
                //Debug.Log($"IS_TEAM_MEMBER_SELECTION allCard.Value.unittype: {allCard.Value.unittype}"); 
                if (Enum.TryParse(allCard.Value.unittype, out UnitMeta.UnitType unitType))
                {
                    if (!UnitMeta.TeamUnitType.ContainsKey(unitType))
                    {
                        continue;
                    }
                }
            }
            
            characterImage = Arts.CharacterArtDictionary[allCard.Key];
            userCard = Instantiate(userCardPrefab);
            userCard.GetComponent<UserCardButton>().characterImage.sprite = characterImage.image ;
            userCard.GetComponent<UserCardButton>().cardkey = allCard.Key;

            if (spTypeArt.SpTypeArtDictionary.TryGetValue(allCard.Value.spType, out spTypeImage))
            {
                userCard.GetComponent<UserCardButton>().spTypeImage.sprite = spTypeImage.image;
            }
            else
            {
                userCard.GetComponent<UserCardButton>().spTypeImage.gameObject.SetActive(false);
                userCard.GetComponent<UserCardButton>().spTypeImage.transform.parent.gameObject.SetActive(false);
            }
            

            if (userCardDict.TryGetValue(allCard.Key, out card))
            {
                userCard.GetComponent<UserCardButton>().lockImage.SetActive(false);
                userCard.GetComponent<UserCardButton>().isLock = false;
                if (IS_TEAM_MEMBER_SELECTION)
                    userCard.GetComponent<UserCardButton>().userLevelBar.SetActive(false);

            }
            else
            {
                userCard.GetComponent<UserCardButton>().lockImage.SetActive(true);
                userCard.GetComponent<UserCardButton>().userLevelBar.SetActive(false);
                userCard.GetComponent<UserCardButton>().isLock = true;
                card = allCard.Value;
            }

            // Localization
            //userCard.GetComponent<UserCardButton>().cardname.text = card.cardkey;
            //Debug.Log($"UserCardManager.Populate() card.cardkey:{card.cardkey}");
            AsyncOperationHandle<string> op = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(LanguageSelectionManager.STRING_TEXT_REF, card.cardkey.ToLower(), null);
            if (op.IsDone)
            {
                userCard.GetComponent<UserCardButton>().cardname.text = op.Result;
            }
            else
            {
                op.Completed += (o) => userCard.GetComponent<UserCardButton>().cardname.text = o.Result;
            }
            //Debug.Log($"UserCardManager.Populate() after locale userCard.GetComponent<UserCardButton>().cardname.text:{userCard.GetComponent<UserCardButton>().cardname.text}");
            //Debug.Log($"UserCardManager.Populate() after locale localizationResponder:{localizationResponder}");

            userCard.GetComponent<UserCardButton>().cardname.font = localizationResponder.getCurrentFont();
            //Debug.Log($"UserCardManager.Populate() after locale userCard.GetComponent<UserCardButton>().cardname.font:{userCard.GetComponent<UserCardButton>().cardname.font}");

            userCard.GetComponent<UserCardButton>().level.text = card.level;
            userCard.GetComponent<UserCardButton>().exp.text = card.exp;
            userCard.GetComponent<UserCardButton>().cardtype = card.unittype;
            //Debug.Log($"unitTypeArt unittype: {card.unittype}  ");
            userCard.GetComponent<UserCardButton>().unitTypeImage.sprite = unitTypeArt.UnitTypeArtDictionary[card.unittype].image;

            userCard.transform.Find(card.rarity.ToLower() + "_background").gameObject.SetActive(true);
            userCard.GetComponent<UserCardButton>().rarity.transform.Find(card.rarity.ToLower() + "_rare_background").gameObject.SetActive(true);
            userCard.GetComponent<UserCardButton>().rarity.transform.Find(card.rarity.ToLower() + "_rare_background/text_rare_value").transform.GetComponent<TMP_Text>().text = card.rarity;

            userCard.GetComponent<UserCardButton>().leveluprequirement.text = card.leveluprequirement;
            if (IS_TEAM_MEMBER_SELECTION)
            {
                userCard.GetComponent<UserCardButton>().userCardFocus = userCardFocus;
                userCard.GetComponent<UserCardButton>().cardSlotParent = cardSlotParent;
            }
            userCard.GetComponent<UserCardButton>().IS_TEAM_MEMBER_SELECTION = IS_TEAM_MEMBER_SELECTION;
            if (Int32.TryParse(card.star, out int star))
            {
                for (int j = (userCard.GetComponent<UserCardButton>().star.transform.childCount - 1); j > (star-1); j--)
                {
                    userCard.GetComponent<UserCardButton>().star.transform.GetChild(j).Find("Active").gameObject.SetActive(false);
                }
            }
            if (float.TryParse(card.exp, out float cardexp) && float.TryParse(card.leveluprequirement, out float cardleveluprequirement))
            {
                userCard.GetComponent<UserCardButton>().levelSlider.value = cardexp / cardleveluprequirement;
            }
            userCard.transform.SetParent(transform);
        }

        if (!IS_TEAM_MEMBER_SELECTION && CharacterTabSlot.transform.childCount > 0)
        {
            CharacterTabButton characterTabButton = CharacterTabSlot.transform.GetChild(StaticClass.SelectedCharacterTab).GetComponent<CharacterTabButton>();
            characterTabButton.HandleClick();
        }

        yield return null;
    }

    // sends an API request - returns a JSON file
    IEnumerator GetUserCard(string userid)
    {
        userCardDict.Clear();
        // resulting JSON from an API request
        JSONNode jsonResult;
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();

        // build the url and query

        webReq.url = string.Format("{0}/{1}/{2}", APIConfig.urladdress, APIConfig.cardService, userid );

        // send the web request and wait for a returning result
        yield return webReq.SendWebRequest();

        // convert the byte array to a string
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);

        // parse the raw string into a json result we can easily read
        jsonResult = JSON.Parse(rawJson);
        for (int i = 0; i < jsonResult.Count; i++)
        {
            if(jsonResult[i]["cardkey"] !=null && jsonResult[i]["cardkey"].ToString().Length > 0  )
                userCardDict.Add(jsonResult[i]["cardkey"] , new UserCard(jsonResult[i]["cardkey"], jsonResult[i]["level"], jsonResult[i]["exp"], jsonResult[i]["specail"], jsonResult[i]["rarity"], jsonResult[i]["leveluprequirement"], jsonResult[i]["star"], jsonResult[i]["unittype"], jsonResult[i]["specialkey"]) );
        }
        UserCardLoaded?. Invoke(userid);
        //Debug.Log($"GetUserCard() jsonResult {webReq.url } {jsonResult}");
        
    }
    // sends an API request - returns a JSON file
    IEnumerator GetAllCard(string sortby)
    {
        allCardDict.Clear();
        // resulting JSON from an API request
        JSONNode jsonResult;
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();

        // build the url and query

        webReq.url = string.Format("{0}/{1}?sortby={2}", APIConfig.urladdress, APIConfig.cardService, sortby);

        // send the web request and wait for a returning result
        yield return webReq.SendWebRequest();

        // convert the byte array to a string
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);

        // parse the raw string into a json result we can easily read
        jsonResult = JSON.Parse(rawJson);
        for (int i = 0; i < jsonResult.Count; i++)
        {
            if (jsonResult[i]["cardkey"] != null && jsonResult[i]["cardkey"].ToString().Length > 0)
                allCardDict.Add(jsonResult[i]["cardkey"], new UserCard(jsonResult[i]["cardkey"], "?", "?", "?", jsonResult[i]["rarity"], "?", jsonResult[i]["star"], jsonResult[i]["unittype"], jsonResult[i]["specialkey"]));
        }
        //Debug.Log($"GetAllCard() jsonResult {webReq.url } {jsonResult}");
    }

}
 
