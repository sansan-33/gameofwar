using System;
using System.Collections;
using System.Text;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Localization.Settings;
using static SpTypeArt;

public class HeroMenu : MonoBehaviour
{
    [SerializeField] public TMP_Text lvlText;
    [SerializeField] public TMP_Text expText;
    [SerializeField] public TMP_Text raceText;
    [SerializeField] public TMP_Text leveluprequirementText;
    [SerializeField] public TMP_Text nameText;
    [SerializeField] public TMP_Text rarityText;
    [SerializeField] public TMP_Text upgradeGoldValue;

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
    [SerializeField] public Image skillImage;
    [SerializeField] public Image passiveImage;
    [SerializeField] public Button levelUpButton;
    [SerializeField] public Slider levelSlider;
    [SerializeField] public GameObject stars;
    [SerializeField] public UnitTypeArt unitTypeArt;
    [SerializeField] public CharacterFullArt characterFullArt;
    [SerializeField] public SpTypeArt spTypeArt;
    [SerializeField] public UnitFactory localFactory;
    [SerializeField] public Transform unitBodyParent;

    private bool CANLEVELUP = false;
    private static float LEVELUP_POWER = 1.1f;
    private Transform unitBody;
   
    private void Start()
    {
        Debug.Log($"Hero Menu Scense Loaded, cardkey is {StaticClass.CrossSceneInformation} ");
        if (StaticClass.CrossSceneInformation == null) { return; }
        StartCoroutine( GetUserCardDetail(StaticClass.UserID, StaticClass.CrossSceneInformation));
    }
    public void levelUpCard()
    {
        if (!CANLEVELUP) { return; }
        StartCoroutine(handleLevelUpCard(StaticClass.CrossSceneInformation, StaticClass.UserID, "1"));
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
        SpTypeImage spTypeImage;

        // parse the raw string into a json result we can easily read
        jsonResult = JSON.Parse(rawJson);
        Debug.Log($"Hero Menu GetUserCardDetail jsonResult:{jsonResult} ");
        if (jsonResult.Count> 0)
        {
            double levelupfactor = Math.Pow(LEVELUP_POWER, Int32.Parse(jsonResult[0]["level"]));
            lvlText.text = jsonResult[0]["level"];
            expText.text = jsonResult[0]["exp"];
            leveluprequirementText.text = jsonResult[0]["leveluprequirement"];
            nameText.text = jsonResult[0]["cardkey"];
            rarityText.text = jsonResult[0]["rarity"];
            raceText.text = jsonResult[0]["race"];
            upgradeGoldValue.text = jsonResult[0]["goldrequirement"];
            if (float.TryParse(jsonResult[0]["exp"], out float cardexp) && float.TryParse(jsonResult[0]["leveluprequirement"], out float cardleveluprequirement))
            {
                if (cardexp >= cardleveluprequirement) {
                    CANLEVELUP = true;
                    //Debug.Log($"Hero Menu levelUpButton.interactable = true ");
                    levelUpButton.interactable = true;
                    //levelUpButton.image.color = Color.white;
                } else
                {
                    //Debug.Log($"Hero Menu levelUpButton.interactable = false ");
                    levelUpButton.interactable = false;
                }
                levelSlider.value = cardexp / cardleveluprequirement;
            }
            //characterImage.sprite = characterFullArt.CharacterFullArtDictionary[jsonResult[0]["cardkey"]].image;
            unitTypeImage.sprite = unitTypeArt.UnitTypeArtDictionary[jsonResult[0]["unittype"]].image;

            Vector3 unitPos = unitBodyParent.gameObject.transform.position;
            UnitMeta.UnitKey unitKey = (UnitMeta.UnitKey)Enum.Parse(typeof(UnitMeta.UnitKey), jsonResult[0]["cardkey"]);
            Debug.Log($"Hero Menu unitKey:{unitKey} / cardkey: {jsonResult[0]["cardkey"]} ");
            GameObject unitPrefab = localFactory.GetUnitPrefab(unitKey);
            if (unitBody == null) {
                unitBody = Instantiate(unitPrefab.transform.Find("Body"));
                unitBody.gameObject.GetComponentInChildren<Animator>().enabled = true;
                unitBody.position = new Vector3(unitPos.x, unitPos.y - 2, unitPos.z);
                unitBody.transform.Rotate(0, 180, 0);
                unitBody.localScale = new Vector3(7, 7, 7);
                unitBody.transform.SetParent(unitBodyParent.transform);
            }

            if (Int32.TryParse(jsonResult[0]["star"], out int star))
            {
                for (int j = (stars.transform.childCount - 1); j > (star - 1); j--)
                {
                     stars.transform.GetChild(j).Find("Active").gameObject.SetActive(false);
                }
            }
            healthValue.text = "" + Math.Round(Double.Parse(jsonResult[0]["health"]) * levelupfactor);
            attackValue.text = "" + Math.Round(jsonResult[0]["attack"] * levelupfactor);
            attackDelayValue.text = jsonResult[0]["repeatattackdelay"];
            defenseValue.text = "" + Math.Round(jsonResult[0]["defense"] * levelupfactor);
            speedValue.text = jsonResult[0]["speed"];
            specialValue.text = jsonResult[0]["special"];
            powerValue.text = jsonResult[0]["power"];

            //Debug.Log($"HeroMenu.GetUserCardDetail() nameText.text:{nameText.text}");
            AsyncOperationHandle<string> op = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(LanguageSelectionManager.STRING_TEXT_REF, nameText.text.ToLower(), null);
            if (op.IsDone)
            {
                nameText.text = op.Result;
            }
            else
            {
                op.Completed += (o) => nameText.text = o.Result;
            }
            //Debug.Log($"HeroMenu.GetUserCardDetail() after locale nameText.text:{nameText.text}");
            
            //Debug.Log($"HeroMenu.GetUserCardDetail() after jsonResult[0][specialkey]:{jsonResult[0]["specialkey"]} jsonResult[0][specialkey].ToString().ToLower():{jsonResult[0]["specialkey"].ToString().ToLower()}");
            if (spTypeArt.SpTypeArtDictionary.TryGetValue(jsonResult[0]["specialkey"], out spTypeImage))
            {
                skillImage.sprite = spTypeImage.image;
                //skillImage.GetComponentInChildren<TMP_Text>().text = jsonResult[0]["specialkey"];

                op = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(LanguageSelectionManager.STRING_TEXT_REF, jsonResult[0]["specialkey"].ToString().Replace("\"", "").ToLower(), null);
                if (op.IsDone)
                {
                    skillImage.GetComponentInChildren<TMP_Text>().text = op.Result;
                }
                else
                {
                    op.Completed += (o) => skillImage.GetComponentInChildren<TMP_Text>().text = o.Result;
                }
            }
            else
            {
                skillImage.gameObject.SetActive(false);
                skillImage.transform.parent.gameObject.SetActive(false);
                //passiveImage.sprite = skillArt.skillImages[jsonResult[0]["passivekey"].ToString().Length == 0 ? "99" : jsonResult[0]["passivekey"]].image;
            }
        }
    }
}

