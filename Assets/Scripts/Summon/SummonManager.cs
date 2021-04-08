using System;
using System.Collections;
using System.Text;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using static CharacterArt;
using static UnitTypeArt;
public class SummonManager : MonoBehaviour 
{
    [Header("Layout References")]
    [SerializeField] Transform summonResultPlacement;
    public GameObject summonResultDisplay;
    public GameObject summonStartDisplay;
    public GameObject summonPullButton;

    [Header("Settings")]
    [SerializeField] float cardXOffset = 0f; // NO Effect to change here, need to set it in inspector
    [SerializeField] float cardYOffset = 0f;  
    [SerializeField] int CardPerRow = 0;
    
    public GameObject cardPrefab;
    Vector3 v360 = new Vector3(0, 0, 90);
    private static int cardMoveSpeed = 5000;
    Array UnitKeyValues = Enum.GetValues(typeof(UnitMeta.UnitKey));
    [SerializeField] public CharacterArt Arts;
    [SerializeField] public UnitTypeArt unitTypeArt;
    private int SUMMON_COUNT=11;

    public void Start()
    {
        if (unitTypeArt.UnitTypeArtDictionary.Count < 1)
        {
            unitTypeArt.initDictionary();
        }
    }
    public void HandleSummon()
    {
        int summonCount = SUMMON_COUNT;
        summonResultDisplay.SetActive(true);
        summonStartDisplay.SetActive(false);
        summonPullButton.SetActive(false);
        StartCoroutine(ServerSummon(StaticClass.UserID, summonCount));
    }
    public void PullAgain()
    {
        summonResultDisplay.SetActive(false);
        summonStartDisplay.SetActive(true);
        summonPullButton.SetActive(true);
        foreach (Transform child in summonResultPlacement)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
    IEnumerator MoveCardTo(Transform cardTransform, Vector3 targetPosition, float local_cardMoveSpeed)
    {
        // break if card is merged
        while (cardTransform != null && (cardTransform.position - targetPosition).sqrMagnitude > 0.00000001f)
        {
            cardTransform.position = Vector3.MoveTowards(cardTransform.position, targetPosition, Time.deltaTime * local_cardMoveSpeed);
            cardTransform.localEulerAngles = Vector3.Lerp(cardTransform.localEulerAngles, v360, Time.deltaTime * 5);
            yield return null;
        }
        if (cardTransform != null)
        {
            cardTransform.position = targetPosition;
            cardTransform.localEulerAngles = Vector3.zero;
        }
        
    }
   
    IEnumerator ServerSummon(string userid, int summonCount)
    {
        GameObject userCard;
        float local_cardMoveSpeed = 0f;
        // resulting JSON from an API request
        JSONNode jsonResult;
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();
        webReq.url = string.Format("{0}/{1}/{2}/{3}", APIConfig.urladdress, APIConfig.summonService, userid, summonCount);

        // send the web request and wait for a returning result
        yield return webReq.SendWebRequest();

        // convert the byte array to a string
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);

        // parse the raw string into a json result we can easily read
        jsonResult = JSON.Parse(rawJson);
        Debug.Log($"ServerSummon {jsonResult}");
        for (int i = 0; i < jsonResult.Count; i++)
        {
            local_cardMoveSpeed = cardMoveSpeed;
            CharacterImage characterImage = Arts.CharacterArtDictionary[jsonResult[i]["cardkey"]];
            userCard = Instantiate(cardPrefab);
            userCard.transform.parent = summonResultPlacement;
            userCard.GetComponent<UserCardButton>().characterImage.sprite = characterImage.image;
            userCard.GetComponent<UserCardButton>().cardkey = characterImage.name.ToString();
            userCard.GetComponent<UserCardButton>().lockImage.SetActive(false);
            userCard.GetComponent<UserCardButton>().userLevelBar.SetActive(false);
            userCard.GetComponent<UserCardButton>().levelBadge.SetActive(false);
            
            userCard.transform.Find( (jsonResult[i]["rarity"] + "_background").ToLower()).gameObject.SetActive(true);
            userCard.GetComponent<UserCardButton>().rarity.transform.Find( (jsonResult[i]["rarity"] + "_rare_background").ToLower()).gameObject.SetActive(true);
            userCard.GetComponent<UserCardButton>().rarity.transform.Find( (jsonResult[i]["rarity"] + "_rare_background/text_rare_value").ToLower()).transform.GetComponent<TMP_Text>().text = jsonResult[i]["rarity"];
            userCard.GetComponent<UserCardButton>().unitTypeImage.sprite = unitTypeArt.UnitTypeArtDictionary[jsonResult[i]["unittype"]].image;
            if (Int32.TryParse(jsonResult[i]["star"], out int star))
            {
                for (int j = (userCard.GetComponent<UserCardButton>().star.transform.childCount -1 ) ; j > (star - 1); j--)
                {
                    userCard.GetComponent<UserCardButton>().star.transform.GetChild(j).Find("Active").gameObject.SetActive(false);
                }
            }
            if (jsonResult[i]["rarity"] == "SSR" || jsonResult[i]["rarity"] == "UR" || jsonResult[i]["rarity"] == "LR")
            {
                userCard.GetComponent<UserCardButton>().cardGlow.SetActive(true);
                local_cardMoveSpeed =  cardMoveSpeed / 3;
            }

            yield return MoveCardTo(userCard.transform, summonResultPlacement.transform.position + new Vector3((i % CardPerRow) * cardXOffset, (i / CardPerRow) * cardYOffset * -1, 0) , local_cardMoveSpeed);

        }
        
    }
}

