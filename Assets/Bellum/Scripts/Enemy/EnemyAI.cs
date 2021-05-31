using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SpecialAttackDict;

public class EnemyAI : MonoBehaviour
{
    private Vector3 halfLine;
    private int elexier = 0;
    private Card nextCard;
    private RTSPlayer player;
    private UnitFactory localFactory;
    private bool ISGAMEOVER = false;
    [SerializeField] private EnemyCardDealer enemyCardDealer;
    private List<Card> cards = new List<Card>();
    private List<SpCostDisplay> spCostDisplay = new List<SpCostDisplay>();
    private enum Position { left, right, centre};
    
    private Dictionary<UnitMeta.UnitType, Position> StratergyPostion = new Dictionary<UnitMeta.UnitType, Position>()
    { };
    [SerializeField] List<UnitMeta.UnitType> unitType;
    [SerializeField] List<Position> position;

    private Dictionary<int, List<UnitMeta.UnitType>> Stratergy = new Dictionary<int, List<UnitMeta.UnitType>>()
    { };
    [SerializeField] List<UnitMeta.UnitType> unitTypesList;
    [SerializeField] List<UnitMeta.UnitType> unitTypesList2;
    [SerializeField] List<UnitMeta.UnitType> unitTypesList3;

    private Dictionary<Position, Vector3> Pos = new Dictionary<Position, Vector3>()
    { };
    [SerializeField] Vector3 enemySpawnPosLeft;
    [SerializeField] Vector3 enemySpawnPosRight;
    [SerializeField] Vector3 enemySpawnPosCentre;
    [SerializeField]
    private Dictionary<UnitMeta.UnitType, List<SpecialAttackType>> Sp = new Dictionary<UnitMeta.UnitType, List<SpecialAttackType>>()
    { };

    private void Start()
    {
        GameStartDisplay.ServerGameStart += StartSpawnnEnemy;
        TotalEleixier.UpdateEnemyElexier += OnUpdateElexier;
        GameOverHandler.ClientOnGameOver += HandleGameOver;
    }
    private void OnDestroy()
    {
        GameStartDisplay.ServerGameStart -= StartSpawnnEnemy;
        TotalEleixier.UpdateEnemyElexier -= OnUpdateElexier;
        GameOverHandler.ClientOnGameOver -= HandleGameOver;
    }
    private void StartSpawnnEnemy()
    {
        StartCoroutine(HandleSpawnnEnemy());
    }
    private IEnumerator HandleDictionary()
    {
        StratergyPostion.Add(unitType[0], position[0]);
        StratergyPostion.Add(unitType[1], position[1]);
        StratergyPostion.Add(unitType[2], position[2]);
        StratergyPostion.Add(unitType[3], position[3]);
        StratergyPostion.Add(unitType[4], position[4]);

        Stratergy.Add(0, unitTypesList);
        Stratergy.Add(1, unitTypesList2);
        Stratergy.Add(2, unitTypesList3);

        Pos.Add(position[0], enemySpawnPosLeft);
        Pos.Add(position[2], enemySpawnPosRight);
        Pos.Add(position[4], enemySpawnPosCentre);
        yield return null;

    }

    private IEnumerator HandleSpawnnEnemy()
    {
       yield return HandleDictionary();
         while (!ISGAMEOVER)
        {
            yield return new WaitForSeconds(4f);
           // Debug.Log($"HandleSpawnnEnemy {cards.Count}");
            
                Debug.Log("HandleSpawnnEnemy");
                yield return SelectCard();
               // yield return SpawnEnemy();
       
             
           
        }
        yield return null;
    }
    public void SetCards(Card card)
    {
        this.cards.Add(card);
    }
    private void OnUpdateElexier(int elexier)
    {
        this.elexier = elexier;
    }
    private IEnumerator SpawnEnemy()
    {
        while(nextCard != null)
        {
            if(nextCard.GetUnitElexier() <= this.elexier)
            {
                int type = (int)nextCard.cardFace.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length;
                yield return SelectPos(nextCard, (UnitMeta.UnitType)type);
                nextCard = null;
            }
        }
        yield return null;
    }
    private IEnumerator SelectCard()
    {
        int i = 2;
        Card _card = null;
        while(_card == null)
        {
            _card = checkStar(i);
            i--;
            if(i == -1) { break; }
        }
        Debug.Log($"SelectCard {_card}");
        nextCard = _card;
        RectTransform rect = nextCard.GetComponent<RectTransform>();
        float x = rect.localScale.x;
        float y = rect.localScale.y;
        float z = rect.localScale.z;
        rect.localScale = new Vector3((float)0.75, (float)0.75,(float)0.75);
        yield return null;
    }
    private Card checkStar(int star)
    {
        foreach (Card card in cards)
        {
            //int stars = (int)card.cardFace.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length
            Debug.Log($"{(int)card.cardFace.star} == {star}");
            if ((int)card.cardFace.star == star)
            {
                if(checkType(card, star))
                {
                    return card;
                }
            }
            if ((int)card.cardFace.star == star)
            {
                return card;
            }
        }
        return null;
    }
    private bool checkType(Card card, int star)
    {
        Debug.Log(star);
        int type = (int)card.cardFace.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length;
        Stratergy.TryGetValue(star, out var unitTypes);
        Debug.Log($"checkType {unitTypes}");
        int i = 0;
        while (i < unitTypes.Count)
        {
            if ((UnitMeta.UnitType)type == unitTypes[0])
            {
                return true;
                break;
            }
            i++;
        }
       return false;
    }
    private IEnumerator SelectPos(Card card, UnitMeta.UnitType type)
    {
        CardFace cardFace = card.cardFace;
        if(localFactory == null) { yield return SetLocalFactory(); }
        GameObject[] units = GameObject.FindGameObjectsWithTag("Player" + 0);
        GameObject king = GameObject.FindGameObjectWithTag("King" + 0);
        List<GameObject> armies = new List<GameObject>();
        armies = units.ToList();
        if (king != null)
            armies.Add(king);
        foreach(GameObject unit in armies)
        {
            if(unit.transform.position.x > halfLine.x)
            {

            }
        }
        if (StratergyPostion.TryGetValue(type, out var pos))
        {
            if (!UnitMeta.UnitSize.TryGetValue((UnitMeta.UnitType)type, out int unitsize)) { unitsize = 1; }
            if (Pos.TryGetValue(pos, out Vector3 cardPos))
            {
                localFactory.CmdDropUnit(player.GetEnemyID(), cardPos, StaticClass.playerRace, (UnitMeta.UnitType)type, ((UnitMeta.UnitType)type).ToString(), unitsize, cardFace.stats.cardLevel,
                    cardFace.stats.health, cardFace.stats.attack, cardFace.stats.repeatAttackDelay, cardFace.stats.speed, cardFace.stats.defense, cardFace.stats.special, cardFace.stats.specialkey,
                    cardFace.stats.passivekey, (int)cardFace.star + 1, player.GetTeamColor(), Quaternion.identity);

            }
        }
        //SpecialAttack(type);
        yield return null;
    }
    private void SpecialAttack(UnitMeta.UnitType type)
    {
        if(Sp.TryGetValue(type,out List<SpecialAttackType> specialAttackTypes))
        {
            GameObject[] units = GameObject.FindGameObjectsWithTag("Player" + 1);
            GameObject king = GameObject.FindGameObjectWithTag("King" + 1);
            List<GameObject> armies = new List<GameObject>();
            armies = units.ToList();
            if (king != null)
                armies.Add(king);
            foreach (GameObject unit in armies)
            {
                if (unit.GetComponent<Unit>().unitType == UnitMeta.UnitType.HERO || unit.GetComponent<Unit>().unitType == UnitMeta.UnitType.KING)
                {
                    SpecialAttackType specialAttackType = (SpecialAttackType)Enum.Parse(typeof(SpecialAttackType), unit.GetComponent<CardStats>().specialkey.ToUpper());
                    foreach(SpecialAttackType _specialAttackType in specialAttackTypes)
                    {
                        if(specialAttackType == _specialAttackType)
                        {
                            ISpecialAttack iSpecialAttack = unit.GetComponentInChildren(typeof(ISpecialAttack)) as ISpecialAttack;
                            SpButtonManager.enemyUnitBtns.TryGetValue(unit.GetComponent<Unit>().unitKey, out var btn);
                            if (iSpecialAttack.GetSpCost() <= btn.GetComponent<SpCostDisplay>().spCost / 3)
                            {
                                iSpecialAttack.OnPointerDown();
                            }
                        }
                    }

                }                   
            }
        }
    }
    private void HandleGameOver(string winner)
    {
        ISGAMEOVER = true;
    }

    IEnumerator SetLocalFactory()
    {
        
        if (localFactory == null)
        {
            foreach (GameObject factroy in GameObject.FindGameObjectsWithTag("UnitFactory"))
            {
                if (factroy.GetComponent<UnitFactory>().hasAuthority)
                {
                    localFactory = factroy.GetComponent<UnitFactory>();
                }
            }
        }
        yield return null;
    }

    private void Update()
    {
        
    }
}

