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
    [SerializeField] private Transform spPrefabParent;
    public Dictionary<SpecialAttackType, GameObject> SpecialAttackPrefab = new Dictionary<SpecialAttackType, GameObject>();
   

    public int buttonOffSet;
    public RectTransform FirstCardPos;
    private GameObject button;
    private int buttonCount;
    private List<SpecialAttackDict.SpecialAttackType> spawnedButtonSpType = new List<SpecialAttackDict.SpecialAttackType>();
    public List<UnitMeta.UnitKey> spawnedSpButtonUnit = new List<UnitMeta.UnitKey>();
    private Sprite sprite;
    private RTSPlayer player;
    private GameObject buttonChild;
    private ISpecialAttack SpecialAttack;

    void Awake()
    {
        Debug.Log("SpButtonManager Awake()");
        Arts.initDictionary();

        SpecialAttackPrefab.Add(SpecialAttackType.LIGHTNING, lightningPrefab);
        SpecialAttackPrefab.Add(SpecialAttackType.STUN, stunPrefab);
        SpecialAttackPrefab.Add(SpecialAttackType.ICE, icePrefab);
        SpecialAttackPrefab.Add(SpecialAttackType.SHIELD, shieldPrefab);

        //this.Start();



    }

    private void Start()
    {
        Debug.Log("SpButtonManager void Start()");
        unitBtn.Clear();
        StartCoroutine(start());
    }

    private IEnumerator start()
    {
        Debug.Log("SpButtonManager IEnumerator Start()");
        yield return new WaitForSeconds(2);
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
                        //var gotValue = unitSp.TryGetValue(unit.GetComponent<Unit>().unitKey, out SpecialAttackDict.SpecialAttackType type);
                        //if (gotValue == true)
                        //{
                        //   unit.specialAttackTypes = type;
                        //Debug.Log("one player mode");

                        
                        SpecialAttackType specialAttackType = (SpecialAttackType)Enum.Parse(typeof(SpecialAttackType), unit.specialkey.ToUpper());
                        Debug.Log($"1 player mode specialAttackType: {specialAttackType}, SpecialAttackPrefab[specialAttackType]: {SpecialAttackPrefab[specialAttackType]}");
                        SpecialAttack = SpecialAttackPrefab[specialAttackType].GetComponent(typeof(ISpecialAttack)) as ISpecialAttack;

                        Debug.Log($"1 player mode SpecialAttack: {SpecialAttack}");
                        //SpecialAttack = unit.GetComponent(typeof(ISpecialAttack)) as ISpecialAttack;
                        InstantiateSpButton(unit.specialAttackType, unit.GetComponent<Unit>(), SpecialAttack);
                    //}

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

                        SpecialAttack = unit.GetComponent(typeof(ISpecialAttack)) as ISpecialAttack;
                        InstantiateSpButton(unit.specialAttackType, unit.GetComponent<Unit>(), SpecialAttack);

                    }
                }
            }
    }

    public void InstantiateSpButton(SpecialAttackDict.SpecialAttackType spType, Unit unit, ISpecialAttack specialAttack)
    {
        Debug.Log("SpButtonManager InstantiateSpButton()");

        //only spawn one button for each type of Sp
        Debug.Log($"spawn {spType}");
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
            switch (spType)
            {
                case SpecialAttackDict.SpecialAttackType.ICE:
                    Ice ice = unit.gameObject.AddComponent<Ice>();
                    ice.layerMask = layerMask;
                break;
                   /* break;
                case SpecialAttackDict.SpecialAttackType.Slash:
                    unit.gameObject.AddComponent<GoldenSlash>();
                    break;
                case SpecialAttackDict.SpecialAttackType.Shield:
                    unit.gameObject.AddComponent<DefendSP>();
                    break;
                case SpecialAttackDict.SpecialAttackType.Stun:
                    unit.gameObject.AddComponent<Stun>();
                    break;
                case SpecialAttackDict.SpecialAttackType.Lightling:
                    unit.gameObject.AddComponent<Lightling>();
                    break;*/
            }

        Debug.Log($"SpButtonManager InstantiateSpButton() button:{button}, specialAttack:{specialAttack}");

        button.GetComponent<Button>().onClick.AddListener(specialAttack.OnPointerDown);
            // tell unit where is the button in the list
            unitBtn.Add(unit.unitKey, button.GetComponent<Button>());

    }

        public static Dictionary<UnitMeta.UnitKey, Button> unitBtn = new Dictionary<UnitMeta.UnitKey, Button>()
        {

        };

        // Update is called once per frame
        void Update()
        {

        }

}

