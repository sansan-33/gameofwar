using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static CharacterArt;
using static UnitTypeArt;
public class UserCardManager : MonoBehaviour
{
    [SerializeField] GameObject userCardPrefab;
    [SerializeField] public CharacterArt Arts;
    [SerializeField] public UnitTypeArt unitTypeArt;
    [SerializeField] bool IS_TEAM_MEMBER_SELECTION;
    public GameObject userCardFocus;
    public GameObject cardSlotParent;

    public Dictionary<string, UserCard> userCardDict = new Dictionary<string, UserCard>();
    private Dictionary<string, UserCard> allCardDict = new Dictionary<string, UserCard>();
    public static event Action<string> UserCardLoaded;
    public void Start()
    {
        StartCoroutine(Populate(StaticClass.UserID));
    }
    IEnumerator Populate(string userid)
    {
        yield return GetAllCard("id");
        yield return GetUserCard(userid);

        GameObject userCard;
        CharacterImage characterImage;
        UserCard card;
        foreach (var allCard in allCardDict)
        {
            characterImage = Arts.CharacterArtDictionary[allCard.Key];
            userCard = Instantiate(userCardPrefab);
            userCard.GetComponent<UserCardButton>().characterImage.sprite = characterImage.image ;
            userCard.GetComponent<UserCardButton>().cardkey = allCard.Key;
            
            if (userCardDict.TryGetValue(allCard.Key, out card))
            {
                userCard.GetComponent<UserCardButton>().lockImage.SetActive(false);
                if (IS_TEAM_MEMBER_SELECTION)
                    userCard.GetComponent<UserCardButton>().userLevelBar.SetActive(false);

            }
            else
            {
                userCard.GetComponent<UserCardButton>().userLevelBar.SetActive(false);
                card = allCard.Value;
            }
          
            userCard.GetComponent<UserCardButton>().level.text = card.level;
            userCard.GetComponent<UserCardButton>().exp.text = card.exp;
            userCard.GetComponent<UserCardButton>().cardtype = card.unittype;
            Debug.Log($"unitTypeArt {unitTypeArt.UnitTypeArtDictionary[card.unittype].type }");
            userCard.GetComponent<UserCardButton>().unitTypeImage.sprite = unitTypeArt.UnitTypeArtDictionary[card.unittype].image;
            userCard.GetComponent<UserCardButton>().rarity.text = card.rarity;
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
                    userCard.GetComponent<UserCardButton>().star.transform.GetChild(j).gameObject.SetActive(false);
                }
            }
            if (float.TryParse(card.exp, out float cardexp) && float.TryParse(card.leveluprequirement, out float cardleveluprequirement))
            {
                userCard.GetComponent<UserCardButton>().levelSlider.value = cardexp / cardleveluprequirement;
            }
            userCard.transform.SetParent(transform);
        }
        yield return null;
    }
    // sends an API request - returns a JSON file
    IEnumerator GetUserCard(string userid)
    {
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
            userCardDict.Add(jsonResult[i]["cardkey"] , new UserCard(jsonResult[i]["cardkey"], jsonResult[i]["level"], jsonResult[i]["exp"], jsonResult[i]["specail"], jsonResult[i]["rarity"], jsonResult[i]["leveluprequirement"], jsonResult[i]["star"], jsonResult[i]["unittype"]) );
        }
        UserCardLoaded?. Invoke(userid);
        Debug.Log($"jsonResult {webReq.url } {jsonResult}");
        
    }
    // sends an API request - returns a JSON file
    IEnumerator GetAllCard(string sortby)
    {
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
            allCardDict.Add(jsonResult[i]["cardkey"], new UserCard(jsonResult[i]["cardkey"], "?", "?", "?", jsonResult[i]["rarity"], "?", jsonResult[i]["star"], jsonResult[i]["unittype"]));
        }
        Debug.Log($"jsonResult {webReq.url } {jsonResult}");

    }

}
 
