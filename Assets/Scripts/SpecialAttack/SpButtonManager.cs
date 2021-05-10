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
    [SerializeField] private Transform spPrefabParent;
    public Dictionary<SpecialAttackType, GameObject> SpecialAttackPrefab = new Dictionary<SpecialAttackType, GameObject>();
   

    public int buttonOffSet;
    public RectTransform FirstCardPos;
    private GameObject button;
    private int buttonCount;
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
        SpecialAttackPrefab.Add(SpecialAttackType.STUN, stunPrefab);
        SpecialAttackPrefab.Add(SpecialAttackType.ICE, icePrefab);
        SpecialAttackPrefab.Add(SpecialAttackType.SHIELD, shieldPrefab);
        SpecialAttackPrefab.Add(SpecialAttackType.SLASH, slashPrefab);
    }

    private void Start()
    {
        //Debug.Log("SpButtonManager void Start()");
        unitBtn.Clear();
        //StartCoroutine(start());
        CardDealer.UserCardLoaded += HandleButtonSetup;
        
    }
    private void OnDestroy()
    {
        CardDealer.UserCardLoaded -= HandleButtonSetup;
        Unit.ClientOnUnitDespawned -= OnEnemyDied;
        Unit.ClientOnUnitSpawned += OnEnemySpawn;
    }
    private void HandleButtonSetup()
    {
        StartCoroutine(SpecialButtonSetup());
    }
    private IEnumerator SpecialButtonSetup()
    {
        //Debug.Log("SpButtonManager IEnumerator SpecialButtonSetup");
        yield return new WaitForSeconds(2);
        Unit.ClientOnUnitDespawned += OnEnemyDied;
        Unit.ClientOnUnitSpawned += OnEnemySpawn;
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        CardStats[] units;
        //find all unit
        units = FindObjectsOfType<CardStats>();
        //senemyList = GameObject.FindGameObjectsWithTag("Player" + player.GetEnemyID()).ToList();
        // Debug.Log($"{units.Length}");
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)//1 player mode
        {

            foreach (CardStats unit in units)
            {  // Only Set on our side
               //Debug.Log($"one player mode {unit.tag}");
                if (unit.CompareTag("Player0") || unit.CompareTag("King0"))
                {
                    
                        SpecialAttackType specialAttackType = (SpecialAttackType)Enum.Parse(typeof(SpecialAttackType), unit.specialkey.ToUpper());
                        // Debug.Log($"1 player mode specialAttackType: {specialAttackType}, SpecialAttackPrefab[specialAttackType]: {SpecialAttackPrefab[specialAttackType]}");
                        GameObject specialAttack = SpecialAttackPrefab[specialAttackType];
                        //Debug.Log($"1 player mode specialAttack: {specialAttack}");
                        InstantiateSpButton(unit.specialAttackType, unit.GetComponent<Unit>(), specialAttack);
                    
                }
                else
                {
                    if (unit.GetComponent<Unit>().unitType == UnitMeta.UnitType.HERO || unit.GetComponent<Unit>().unitType == UnitMeta.UnitType.KING)
                    {
                        SpecialAttackType specialAttackType = (SpecialAttackType)Enum.Parse(typeof(SpecialAttackType), unit.specialkey.ToUpper());
                        // Debug.Log($"1 player mode specialAttackType: {specialAttackType}, SpecialAttackPrefab[specialAttackType]: {SpecialAttackPrefab[specialAttackType]}");
                        GameObject specialAttack = SpecialAttackPrefab[specialAttackType];
                        GameObject specialAttackObj = Instantiate(specialAttack, unit.transform);
                        enemySp.Add(specialAttackObj);
                        enemyUnitBtn.Add(unit.GetComponent<Unit>().unitKey, specialAttackObj);
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
                    //Debug.Log($"multi player mode unit.specialkey: {unit.specialkey}");

                    // Anthea 2021-04-22 need to change
                    SpecialAttackType specialAttackType = (SpecialAttackType)Enum.Parse(typeof(SpecialAttackType), unit.specialkey.ToUpper());
                    //Debug.Log($"1 player mode specialAttackType: {specialAttackType}, SpecialAttackPrefab[specialAttackType]:");// {SpecialAttackPrefab[specialAttackType]}");
                    GameObject specialAttack = SpecialAttackPrefab[specialAttackType];
                    //Debug.Log($"1 player mode specialAttack: {specialAttack}");
                    InstantiateSpButton(unit.specialAttackType, unit.GetComponent<Unit>(), specialAttack);
                    }
                }
            }
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
        StartCoroutine(SetupSpecialButtonMidMatch(unit));
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
    public void InstantiateSpButton(SpecialAttackDict.SpecialAttackType spType, Unit unit, GameObject specialAttack)
    {
        //Debug.Log("SpButtonManager InstantiateSpButton()");

        //only spawn one button for each type of Sp
        //Debug.Log($"spawn {spType}");
        buttonCount++;
        // if(spType == SpecialAttackDict.SpecialAttackType.Shield) { Debug.Log(buttonCount); }
        // spawn the button
        CharacterImage characterImage = Arts.CharacterArtDictionary[unit.unitKey.ToString()];
        button = Instantiate(buttonPrefab, spPrefabParent);
        //Set button pos
        button.GetComponent<RectTransform>().anchoredPosition = new Vector3(FirstCardPos.anchoredPosition.x + buttonOffSet * buttonCount, FirstCardPos.anchoredPosition.y, 0);
        buttonChild = button.transform.Find("mask").gameObject;
        buttonChild.transform.GetChild(0).GetComponent<Image>().sprite = characterImage.image;
        SpecialAttackDict.ChildSpSprite.TryGetValue(spType, out sprite);
        buttonChild.transform.GetChild(1).GetComponent<Image>().sprite = sprite;
        spawnedSpButtonUnit.Add(unit.unitKey);
        buttons.Add(button.GetComponent<Button>());

        // Instantiate specialAttack

        GameObject specialAttackObj = Instantiate(specialAttack, spPrefabParent);        

        ISpecialAttack iSpecialAttack = specialAttackObj.GetComponent(typeof(ISpecialAttack)) as ISpecialAttack;
        specialAttackObj.transform.SetParent(unit.transform);
        
       // Debug.Log($"SpButtonManager InstantiateSpButton() specialAttackObj:{specialAttackObj}, iSpecialAttack:{iSpecialAttack}");


        button.GetComponent<Button>().onClick.AddListener(iSpecialAttack.OnPointerDown);
        button.GetComponent<SpCostDisplay>().SetUnit(unit);
        button.GetComponent<SpCostDisplay>().SetSpPrefab(specialAttack);
        // tell unit where is the button in the list
        unitBtn.Add(unit.unitKey, button.GetComponent<Button>());

    }

    public static Dictionary<UnitMeta.UnitKey, Button> unitBtn = new Dictionary<UnitMeta.UnitKey, Button>()
    {

    };
    public static Dictionary<UnitMeta.UnitKey, GameObject> enemyUnitBtn = new Dictionary<UnitMeta.UnitKey, GameObject>()
    {

    };
    // Update is called once per frame
    void Update()
    {

    }

}

