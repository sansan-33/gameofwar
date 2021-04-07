using System;
using System.Collections;
using System.Text;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HeroMenu : MonoBehaviour
{
    [SerializeField] public TMP_Text lvlText;
    [SerializeField] public TMP_Text expText;
    [SerializeField] public TMP_Text raceText;
    [SerializeField] public TMP_Text leveluprequirementText;
    [SerializeField] public TMP_Text nameText;
    [Header("Card Statistic")]
    [SerializeField] public TMP_Text healthValue;
    [SerializeField] public TMP_Text attackValue;
    [SerializeField] public TMP_Text attackDelayValue;
    [SerializeField] public TMP_Text defenseValue;
    [SerializeField] public TMP_Text speedValue;
    [SerializeField] public TMP_Text specialValue;
    [SerializeField] public TMP_Text powerValue;

    [SerializeField] public Image characterImage;
    [SerializeField] public Image unitTypeImage;
    [SerializeField] public Button levelUpButton;
    [SerializeField] public Slider levelSlider;
    [SerializeField] public GameObject stars;
    [SerializeField] public UnitTypeArt unitTypeArt;
    [SerializeField] public CharacterFullArt characterFullArt;

    private bool CANLEVELUP = false;
    private static float LEVELUP_POWER = 1.1f;

    private void Start()
    {
        Debug.Log($"Hero Menu Scense Loaded, cardkey is {StaticClass.CrossSceneInformation} ");
        if (StaticClass.CrossSceneInformation == null) { return; }
        StartCoroutine( GetUserCardDetail(StaticClass.UserID, StaticClass.CrossSceneInformation));
    }
    public void levelUpCard()
    {
        if (!CANLEVELUP) { return; }
        StartCoroutine(handleLevelUpCard(StaticClass.CrossSceneInformation, "1", "1"));
        CANLEVELUP = false;
        levelUpButton.image.color = Color.black;
    }
    IEnumerator handleLevelUpCard(string cardkey, string userid, string level)
    {
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();

        // build the url and query
        webReq.url = string.Format("{0}/{1}/{2}/{3}/{4}", APIConfig.urladdress, APIConfig.levelUpCardService, cardkey, userid, level);
        webReq.method = "put";
        Debug.Log($"levelup card {webReq.url }");
        // send the web request and wait for a returning result
        yield return webReq.SendWebRequest();
        yield return GetUserCardDetail(userid, cardkey);
    }
    // sends an API request - returns a JSON file
    IEnumerator GetUserCardDetail(string userid, string cardkey)
    {
        JSONNode jsonResult;
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();
        webReq.url = string.Format("{0}/{1}/{2}/{3}", APIConfig.urladdress, APIConfig.cardService, userid, cardkey);
        yield return webReq.SendWebRequest();

        // convert the byte array to a string
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);

        // parse the raw string into a json result we can easily read
        jsonResult = JSON.Parse(rawJson);
        if(jsonResult.Count> 0)
        {
            double levelupfactor = Math.Pow(LEVELUP_POWER, Int32.Parse(jsonResult[0]["level"]));
            lvlText.text = jsonResult[0]["level"];
            expText.text = jsonResult[0]["exp"];
            leveluprequirementText.text = jsonResult[0]["leveluprequirement"];
            nameText.text = jsonResult[0]["cardkey"];
            raceText.text = jsonResult[0]["race"];
            if (float.TryParse(jsonResult[0]["exp"], out float cardexp) && float.TryParse(jsonResult[0]["leveluprequirement"], out float cardleveluprequirement))
            {
                if (cardexp >= cardleveluprequirement) { CANLEVELUP = true; levelUpButton.image.color = Color.white; }
                levelSlider.value = cardexp / cardleveluprequirement;
            }
            characterImage.sprite = characterFullArt.CharacterFullArtDictionary[jsonResult[0]["cardkey"]].image;
            unitTypeImage.sprite = unitTypeArt.UnitTypeArtDictionary[jsonResult[0]["unittype"]].image;
            if (Int32.TryParse(jsonResult[0]["star"], out int star))
            {
                for (int j = (stars.transform.childCount - 1); j > (star - 1); j--)
                {
                     stars.transform.GetChild(j).gameObject.SetActive(false);
                }
            }
            healthValue.text = "" + Math.Round(Double.Parse(jsonResult[0]["health"]) * levelupfactor);
            attackValue.text = "" + Math.Round(jsonResult[0]["attack"] * levelupfactor);
            attackDelayValue.text = jsonResult[0]["repeatattackdelay"];
            defenseValue.text = "" + Math.Round(jsonResult[0]["defense"] * levelupfactor);
            speedValue.text = jsonResult[0]["speed"];
            specialValue.text = jsonResult[0]["special"];
            powerValue.text = jsonResult[0]["power"];

        }
    }
}

