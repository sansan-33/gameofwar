using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using static CharacterArt;
using static SpecialAttackDict;

public class SpButtonManager : MonoBehaviour
{
    [SerializeField] public LayerMask layerMask;
    [SerializeField] public CharacterArt Arts;
    [SerializeField] GameObject buttonPrefab;

    [SerializeField] public GameObject lightningPrefab;
    [SerializeField] public GameObject icePrefab;
    [SerializeField] public GameObject stunPrefab;
    [SerializeField] public GameObject shieldPrefab;
    [SerializeField] public GameObject slashPrefab;
    [SerializeField] public GameObject impectSmashPrefab;
    [SerializeField] GameObject zapPrefab;
    [SerializeField] GameObject freezePrefab;
    [SerializeField] GameObject meteorPrefab;
    [SerializeField] GameObject fireArrowPrefab;
    [SerializeField] GameObject tornadoPrefab;
    [SerializeField] private Transform spPrefabParent;
    public Dictionary<SpecialAttackType, GameObject> SpecialAttackPrefab = new Dictionary<SpecialAttackType, GameObject>();

    public int EnemyButtonOffSet;
    public int buttonOffSet;
    public RectTransform FirstCardPos;
    public RectTransform enemyFirstCardPos;
    private GameObject button;
    private int buttonCount;
    private int enemybuttonCount;
    private bool enemyDied = false;
    private Unit diedEnemy;
    private List<SpecialAttackDict.SpecialAttackType> spawnedButtonSpType = new List<SpecialAttackDict.SpecialAttackType>();
    public List<UnitMeta.UnitKey> spawnedSpButtonUnit = new List<UnitMeta.UnitKey>();
    public static List<Button> buttons = new List<Button>();
    public static List<GameObject> enemySp = new List<GameObject>();
    private Sprite sprite;
    private RTSPlayer player;
    private GameObject buttonChild;
    //private GameObject specialAttack;

    void Awake()
    {
        //Debug.Log("SpButtonManager Awake()");
        Arts.initDictionary();

        SpecialAttackPrefab.Add(SpecialAttackType.LIGHTNING, lightningPrefab);
        SpecialAttackPrefab.Add(SpecialAttackType.STUNO, stunPrefab);
        SpecialAttackPrefab.Add(SpecialAttackType.ICE, icePrefab);
        SpecialAttackPrefab.Add(SpecialAttackType.SHIELD, shieldPrefab);
        SpecialAttackPrefab.Add(SpecialAttackType.SLASH, slashPrefab);
        SpecialAttackPrefab.Add(SpecialAttackType.FIREARROW, fireArrowPrefab);
        SpecialAttackPrefab.Add(SpecialAttackType.METEOR, meteorPrefab);
        SpecialAttackPrefab.Add(SpecialAttackType.TORNADO, tornadoPrefab);
        SpecialAttackPrefab.Add(SpecialAttackType.ZAP, zapPrefab);
        SpecialAttackPrefab.Add(SpecialAttackType.FREEZE, freezePrefab);
        SpecialAttackPrefab.Add(SpecialAttackType.STUN, stunPrefab);
    }

    private void Start()
    {
        //Debug.Log("SpButtonManager void Start()");
        unitBtn.Clear();
        enemyUnitObj.Clear();
        enemyUnitBtns.Clear();
        //StartCoroutine(start());
        SpawnTeam.UserCardLoaded += HandleButtonSetup;
        
    }
    private void OnDestroy()
    {
        SpawnTeam.UserCardLoaded -= HandleButtonSetup;
        Unit.ClientOnUnitDespawned -= OnEnemyDied;
        Unit.ClientOnUnitSpawned -= OnEnemySpawn;
    }
    private void HandleButtonSetup()
    {
        StartCoroutine(SpecialButtonSetup());
    }
    private IEnumerator SpecialButtonSetup()
    {
        //Debug.Log("SpButtonManager IEnumerator SpecialButtonSetup");
        yield return new WaitForSeconds(1);
        Unit.ClientOnUnitDespawned += OnEnemyDied;
        Unit.ClientOnUnitSpawned += OnEnemySpawn;
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        CardStats[] units;
        //find all unit
        units = FindObjectsOfType<CardStats>();
        //senemyList = GameObject.FindGameObjectsWithTag("Player" + player.GetEnemyID()).ToList();
         //Debug.Log($"{units.Length}");
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)//1 player mode
        {

            foreach (CardStats unit in units)
            {  // Only Set on our side
               //Debug.Log($"one player mode {unit.tag}");
                if (unit.CompareTag("Player0") || unit.CompareTag("King0"))
                {
                    if (unit.specialkey != "")
                    {
                        //SpecialAttackType specialAttackType = (SpecialAttackType)Enum.Parse(typeof(SpecialAttackType), unit.specialkey.ToUpper());

                        // Debug.Log($"1 player mode specialAttackType: {specialAttackType}, SpecialAttackPrefab[specialAttackType]: {SpecialAttackPrefab[specialAttackType]}");

                        //GameObject specialAttack = SpecialAttackPrefab[specialAttackType];

                       // Debug.Log($"1 player mode unit.specialAttackType: {unit.specialAttackType} {unit.name}");
                        //InstantiateSpButton(unit.specialAttackType, unit.GetComponent<Unit>(), specialAttack, false);
                        InstantiateSpButton(unit.specialAttackType, unit.GetComponent<Unit>(), false);
                    }
                    
                }
                else
                {
                    if (unit.GetComponent<Unit>().unitType == UnitMeta.UnitType.HERO || unit.GetComponent<Unit>().unitType == UnitMeta.UnitType.KING)
                    {
                        //Debug.Log($"enemy unit.specialkey{unit.specialkey} {unit.name}");
                        if(unit.specialkey == "") { Debug.Log("unit specail key is null"); }
                        if(unit.specialkey != "")
                        {
                            //SpecialAttackType specialAttackType = (SpecialAttackType)Enum.Parse(typeof(SpecialAttackType), unit.specialkey.ToUpper());
                             //Debug.Log($"1 player mode specialAttackType: {specialAttackType}, SpecialAttackPrefab[specialAttackType]: {SpecialAttackPrefab[specialAttackType]}");
                            //GameObject specialAttack = SpecialAttackPrefab[specialAttackType];

                            //InstantiateSpButton(unit.specialAttackType, unit.GetComponent<Unit>(), specialAttack, true);
                            InstantiateSpButton(unit.specialAttackType, unit.GetComponent<Unit>(), true);
                        }
                    }
                }
            }
            }
            else // Multi player seneriao
            {
                //Debug.Log($"OnPointerDown Defend SP Multi shieldList {shieldList.Length}");
                foreach (CardStats unit in units)
                {  // Only Set on our side
                    if (unit.CompareTag("Player" + player.GetPlayerID()) || unit.CompareTag("King" + player.GetPlayerID()))
                    {
                    //Debug.Log($"multi player mode unit.specialkey: {unit.specialkey} / {unit.name} /  {unit.tag}");

                    // Anthea 2021-04-22 need to change
                   // SpecialAttackType specialAttackType = (SpecialAttackType)Enum.Parse(typeof(SpecialAttackType), unit.specialkey.ToUpper());
                    //Debug.Log($"1 player mode specialAttackType: {specialAttackType}, SpecialAttackPrefab[specialAttackType]:");// {SpecialAttackPrefab[specialAttackType]}");
                    //GameObject specialAttack = SpecialAttackPrefab[specialAttackType];
                    //Debug.Log($"1 player mode specialAttack: {specialAttack}");
                    //InstantiateSpButton(unit.specialAttackType, unit.GetComponent<Unit>(), specialAttack, false);
                    InstantiateSpButton(unit.specialAttackType, unit.GetComponent<Unit>(), false);
                }
                }
            }
        yield return null;
    }
    private void OnEnemyDied(Unit unit)
    {
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)//1 player mode
        {
            //Debug.Log($"{unit.tag} {unit.CompareTag("Player1")}]");
            if (unit.CompareTag("Player1"))
            {
                diedEnemy = unit;
            }
        }
        //enemyDied = false;
    }
    private void OnEnemySpawn(Unit unit)
    {
        //StartCoroutine(SetupSpecialButtonMidMatch(unit));
    }

    private IEnumerator SetupSpecialButtonMidMatch(Unit unit)
    {
        yield return new WaitForSeconds(1);
        //Debug.Log($"SetupSpecialButtonMidMatch {enemyDied}");

        if (diedEnemy != null)
        {
            if (unit.unitType == UnitMeta.UnitType.HERO || unit.unitType == UnitMeta.UnitType.KING)
            {
                if (diedEnemy.unitKey == unit.unitKey)
                {
                    // Debug.Log($"SetupSpecialButtonMidMatch make obj");
                    SpecialAttackType specialAttackType = (SpecialAttackType)Enum.Parse(typeof(SpecialAttackType), unit.GetComponent<CardStats>().specialkey.ToUpper());
                    // Debug.Log($"1 player mode specialAttackType: {specialAttackType}, SpecialAttackPrefab[specialAttackType]: {SpecialAttackPrefab[specialAttackType]}");
                    GameObject specialAttack = SpecialAttackPrefab[specialAttackType];
                    Instantiate(specialAttack, unit.transform);
                }
            }
        }

    }
    /*public void InstantiateSpButton(SpecialAttackDict.SpecialAttackType spType, Unit unit, GameObject specialAttack, bool enemySpawn)
    {
        //Debug.Log("SpButtonManager InstantiateSpButton()");

        //only spawn one button for each type of Sp
        //Debug.Log($"spawn {spType}");
        if (enemySpawn == true)
        {
            enemybuttonCount++;
        }
        else
        {
            buttonCount++;
        }

        
        // if(spType == SpecialAttackDict.SpecialAttackType.Shield) { Debug.Log(buttonCount); }
        // spawn the button
        CharacterImage characterImage = Arts.CharacterArtDictionary[unit.unitKey.ToString()];
        button = Instantiate(buttonPrefab, spPrefabParent);
        //Set button pos
        if(enemySpawn == true)
        {
            button.GetComponent<RectTransform>().SetAnchor(AnchorPresets.TopLeft);
            button.GetComponent<RectTransform>().localScale = new Vector3(0.25f, 0.25f, 0.25f);
            Debug.Log($"size{button.GetComponent<RectTransform>().localScale}");
        }
        button.GetComponent<RectTransform>().anchoredPosition = !enemySpawn ? new Vector3(FirstCardPos.anchoredPosition.x + buttonOffSet * buttonCount, FirstCardPos.anchoredPosition.y, 0) :
        new Vector3(enemyFirstCardPos.anchoredPosition.x + EnemyButtonOffSet * enemybuttonCount, enemyFirstCardPos.anchoredPosition.y, 0);
        
        buttonChild = button.transform.Find("mask").gameObject;
        buttonChild.transform.GetChild(0).GetComponent<Image>().sprite = characterImage.image;
        SpecialAttackDict.ChildSpSprite.TryGetValue(spType, out sprite);
        buttonChild.transform.GetChild(1).GetComponent<Image>().sprite = sprite;
        spawnedSpButtonUnit.Add(unit.unitKey);
        buttons.Add(button.GetComponent<Button>());

        //hard code sp type is TORNADO

        if(spType == SpecialAttackType.FIREARROW)
        {
           var impectSmash = Instantiate(impectSmashPrefab, button.transform);
            impectSmash.GetComponent<ImpectSmash>().SetImpectType(fireArrowPrefab);
        }
        if (spType == SpecialAttackType.METEOR)
        {
            var impectSmash = Instantiate(impectSmashPrefab, button.transform);
            impectSmash.GetComponent<ImpectSmash>().SetImpectType(meteorPrefab);
        }
        if (spType == SpecialAttackType.TORNADO)
        {
            var impectSmash = Instantiate(impectSmashPrefab, button.transform);
            impectSmash.GetComponent<ImpectSmash>().SetImpectType(tornadoPrefab);
            impectSmash.GetComponent<ImpectSmash>().SetSpecialAttackType(SpecialAttackType.TORNADO);
        }
        // Instantiate specialAttack
        GameObject specialAttackObj = Instantiate(specialAttack, spPrefabParent);

        ISpecialAttack iSpecialAttack = specialAttackObj.GetComponent(typeof(ISpecialAttack)) as ISpecialAttack;
        specialAttackObj.transform.SetParent(unit.transform);
        if (enemySpawn == true)
        {
            //Debug.Log("Instantiate SP");
            enemySp.Add(specialAttackObj);
            enemyUnitObj.Add(unit.GetComponent<Unit>().unitKey, specialAttackObj);
        }

        
        
       // Debug.Log($"SpButtonManager InstantiateSpButton() specialAttackObj:{specialAttackObj}, iSpecialAttack:{iSpecialAttack}");


        button.GetComponent<Button>().onClick.AddListener(iSpecialAttack.OnPointerDown);
        button.GetComponent<SpCostDisplay>().SetUnit(unit);
        button.GetComponent<SpCostDisplay>().SetSpPrefab(specialAttackObj);
        // tell unit where is the button in the list
        if(enemySpawn == false)
        {
            unitBtn.Add(unit.unitKey, button.GetComponent<Button>());
        }
        else
        {
            enemyUnitBtns.Add(unit.unitKey, button.GetComponent<Button>());
        }

        

    }*/
    public void InstantiateSpButton(SpecialAttackDict.SpecialAttackType spType, Unit unit, bool enemySpawn)
    {
        //Debug.Log("SpButtonManager InstantiateSpButton()");
        //Debug.Log($"spType{spType}");
        //only spawn one button for each type of Sp
        //Debug.Log($"spawn {spType}");
        if (enemySpawn == true)
        {
            enemybuttonCount++;
        }
        else
        {
            buttonCount++;
        }


        // if(spType == SpecialAttackDict.SpecialAttackType.Shield) { Debug.Log(buttonCount); }
        // spawn the button
        CharacterImage characterImage = Arts.CharacterArtDictionary[unit.unitKey.ToString()];
        button = Instantiate(buttonPrefab, spPrefabParent);
        //Set button pos
        if (enemySpawn == true)
        {
            button.GetComponent<RectTransform>().SetAnchor(AnchorPresets.TopLeft);
            button.GetComponent<RectTransform>().localScale = new Vector3(0.25f, 0.25f, 0.25f);
            //Debug.Log($"size{button.GetComponent<RectTransform>().localScale}");
        }
        button.GetComponent<RectTransform>().anchoredPosition = !enemySpawn ? new Vector3(FirstCardPos.anchoredPosition.x + buttonOffSet * buttonCount, FirstCardPos.anchoredPosition.y, 0) :
        new Vector3(enemyFirstCardPos.anchoredPosition.x + EnemyButtonOffSet * enemybuttonCount, enemyFirstCardPos.anchoredPosition.y, 0);

        buttonChild = button.transform.Find("mask").gameObject;
        buttonChild.transform.GetChild(0).GetComponent<Image>().sprite = characterImage.image;
        if(SpecialAttackDict.ChildSpSprite.TryGetValue(spType, out sprite))
        {
            buttonChild.transform.GetChild(1).GetComponent<Image>().sprite = sprite;
        }
       
        spawnedSpButtonUnit.Add(unit.unitKey);
        buttons.Add(button.GetComponent<Button>());

        //hard code sp type is Freeze
        //spType = SpecialAttackType.ZAP;

        var impectSmash = Instantiate(impectSmashPrefab, button.transform);
        switch (spType)
        {
            case SpecialAttackType.FIREARROW:
                impectSmash.GetComponent<ImpactSmash>().SetImpectType(fireArrowPrefab);
                impectSmash.GetComponent<ImpactSmash>().SetSpecialAttackType(SpecialAttackType.FIREARROW);
                break;
            case SpecialAttackType.METEOR:
                impectSmash.GetComponent<ImpactSmash>().SetImpectType(meteorPrefab);
                impectSmash.GetComponent<ImpactSmash>().SetSpecialAttackType(SpecialAttackType.METEOR);
                break;
            case SpecialAttackType.TORNADO:
                impectSmash.GetComponent<ImpactSmash>().SetImpectType(tornadoPrefab);
                impectSmash.GetComponent<ImpactSmash>().SetSpecialAttackType(SpecialAttackType.TORNADO);
                break;
            case SpecialAttackType.ZAP:
                impectSmash.GetComponent<ImpactSmash>().SetImpectType(zapPrefab);
                impectSmash.GetComponent<ImpactSmash>().SetSpecialAttackType(SpecialAttackType.ZAP);
                break;
            case SpecialAttackType.FREEZE:
                impectSmash.GetComponent<ImpactSmash>().SetImpectType(null);
                impectSmash.GetComponent<ImpactSmash>().SetSpecialAttackType(SpecialAttackType.FREEZE);
                break;
            case SpecialAttackType.STUN:
                impectSmash.GetComponent<ImpactSmash>().SetImpectType(stunPrefab);
                impectSmash.GetComponent<ImpactSmash>().SetSpecialAttackType(SpecialAttackType.STUN);
                break;
        }

        // Instantiate specialAttack



        // Debug.Log($"SpButtonManager InstantiateSpButton() specialAttackObj:{specialAttackObj}, iSpecialAttack:{iSpecialAttack}");


        button.GetComponent<SpCostDisplay>().SetUnit(unit);
        // tell unit where is the button in the list
        if (enemySpawn == false)
        {
            unitBtn.Add(unit.unitKey, button.GetComponent<Button>());
        }
        else
        {
            enemyUnitBtns.Add(unit.unitKey, button.GetComponent<Button>());
        }



    }
    public static Dictionary<UnitMeta.UnitKey, Button> unitBtn = new Dictionary<UnitMeta.UnitKey, Button>()
    {

    };
    public static Dictionary<UnitMeta.UnitKey, GameObject> enemyUnitObj = new Dictionary<UnitMeta.UnitKey, GameObject>()
    {

    };
    public static Dictionary<UnitMeta.UnitKey, Button> enemyUnitBtns = new Dictionary<UnitMeta.UnitKey, Button>()
    {

    };

    // Update is called once per frame
    void Update()
    {

    }

}


