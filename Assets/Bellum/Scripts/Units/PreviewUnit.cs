using System.Collections;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using static SpTypeArt;

public class PreviewUnit : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    Vector3 mPrevPos = Vector3.zero;
    Vector3 mPosDelta = Vector3.zero;
    Vector3 mPos;

    [SerializeField] GameObject zapPrefab;
    [SerializeField] GameObject freezePrefab;
    [SerializeField] GameObject meteorPrefab;
    [SerializeField] GameObject fireArrowPrefab;
    [SerializeField] GameObject tornadoPrefab;
    [SerializeField] GameObject EarthquakePrefab;
    [SerializeField] GameObject removeGaugePrefab;
    [SerializeField] GameObject grabPrefab;
    [SerializeField] GameObject cardRankUpPrefab;
    [SerializeField] GameObject firePrefab;
    [SerializeField] GameObject bombPrefab;
    [SerializeField] GameObject shieldPrefab;
    [SerializeField] bool showEffect = false;
    [SerializeField] Vector3 spawnPos = new Vector3(66, 222, -1945);

    private Dictionary<string, GameObject> SPEffect = new Dictionary<string, GameObject>()
    {
        
    };
    private Dictionary<string, Vector3> SpPosition = new Dictionary<string, Vector3>()
    {
        { "FIREARROW", new Vector3(92,430,-1945) },
           { "METEOR", new Vector3(92,270,-2010) },
           { "TORNADO", new Vector3(95.75f,215,-1940) },
            {"ZAP", new Vector3(92,222,-1945) },
            //SPEffect.Add("FREEZE", new Vector3(92,222,-1945));
            {"STUN", new Vector3(92,222,-1945) },
            {"REMOVEGAUGE", new Vector3(92,222,-1945) },
            {"GRAB", new Vector3(73,222,-1945) },
            {"CARDRANKUP", new Vector3(92,222,-1945) },
            {"FIRE", new Vector3(72,222,-1945) },
            {"BOMB", new Vector3(92,222,-1945) },
            {"SHIELD", new Vector3(95.4f,266,-1945) },
    };
    private Dictionary<string, Vector3> SpScale = new Dictionary<string, Vector3>()
    {
           { "TORNADO", new Vector3(3, 3, 3) },
            {"GRAB", new Vector3(3, 3, 3) },
            {"SHIELD", new Vector3(7,5,12) },
    };
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"OnBeginDrag ");
        mPos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log($"On Drag ");
        mPosDelta = mPos - mPrevPos;
        transform.Rotate(transform.up, -Vector3.Dot(mPosDelta, Camera.main.transform.right), Space.World);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        mPrevPos = mPos;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(showEffect  == true)
        {
            SPEffect.Clear();
            SPEffect.Add("FIREARROW", fireArrowPrefab);
            SPEffect.Add("METEOR", meteorPrefab);
            SPEffect.Add("TORNADO", tornadoPrefab);
            SPEffect.Add("ZAP", zapPrefab);
            //SPEffect.Add("FREEZE", freezePrefab);
            SPEffect.Add("STUN", EarthquakePrefab);
            SPEffect.Add("REMOVEGAUGE", removeGaugePrefab);
            SPEffect.Add("GRAB", grabPrefab);
            SPEffect.Add("CARDRANKUP", cardRankUpPrefab);
            SPEffect.Add("FIRE", firePrefab);
            SPEffect.Add("BOMB", bombPrefab);
            SPEffect.Add("SHIELD", shieldPrefab);
            StartCoroutine(HandlePlaySP());
        }
        
    }
    private IEnumerator HandlePlaySP()
    {
        JSONNode jsonResult;
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();
        webReq.url = string.Format("{0}/{1}/{2}/{3}", APIConfig.urladdress, APIConfig.cardService,  StaticClass.UserID, StaticClass.CrossSceneInformation);
        yield return webReq.SendWebRequest();
        
        // convert the byte array to a string
        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);
        SpTypeImage spTypeImage;

        // parse the raw string into a json result we can easily read
        jsonResult = JSON.Parse(rawJson);
        string key = jsonResult[0]["specialkey"];
        Debug.Log($"key = {key} json = {jsonResult} json0 = {jsonResult[0]}");
        if (SPEffect.TryGetValue(key, out GameObject _effect))
        {
            Debug.Log($"effect = {_effect}");
            if(_effect != null)
            {
                Debug.Log("spawn prefab");
                GameObject effect = Instantiate(_effect);
                Vector3 spawnPos = SpPosition[key];
                effect.transform.position = spawnPos;
                var rotationVector = effect.transform.rotation.eulerAngles;
                rotationVector.x = 90;
                effect.transform.rotation = Quaternion.Euler(rotationVector);
                if(SpScale.TryGetValue(key, out Vector3 scale))
                {
                    effect.transform.localScale = scale;
                }
                switch (key)
                {
                    case "FIRE":
                        Debug.Log("change rotation of fire");
                        var fireRotationVector = effect.transform.rotation.eulerAngles;
                        fireRotationVector.x = -90;
                        effect.transform.rotation = Quaternion.Euler(fireRotationVector);
                        break;
                }
            }
          
        }
        
    }
    private IEnumerator HandleMoveEffect(Vector3 pos, GameObject effect)
    {
        float timer = 10;
        Vector3 currentVelocity = Vector3.zero;
        while (effect != null && effect.transform.position != pos)
        {
            //Debug.Log($"unit prefab back {unit.transform.position}");
            effect.transform.position = Vector3.SmoothDamp(effect.transform.position, pos, ref currentVelocity, 5f);
            yield return new WaitForSeconds(0.01f);
            if (timer <= 0) { Debug.Log("Time break"); break; }
        }
        Destroy(effect);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0)) {
            mPos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();
            mPosDelta = mPos - mPrevPos;
            transform.Rotate(transform.up, -Vector3.Dot(mPosDelta, Camera.main.transform.right), Space.World);
        }
        mPrevPos = mPos;
    }
}
