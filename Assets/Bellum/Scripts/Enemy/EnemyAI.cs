using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using static SpecialAttackDict;

public class EnemyAI : MonoBehaviour
{
    private int elexier = 0;
    private Card nextCard;
    private RTSPlayer RTSplayer;
    private UnitFactory localFactory;
    private bool ISGAMEOVER = false;
    private List<Card> cards = new List<Card>();
    private List<SpCostDisplay> spCostDisplay = new List<SpCostDisplay>();
    private enum Position { left, right, centre};
    private Vector3 heroPos;
    [SerializeField] private Transform halfLine;
    [SerializeField] private Card wall;
    [SerializeField] private Player player;
    [SerializeField] private Player enemyPlayer;
    [SerializeField] private CardDealer cardDealer;
     private Dictionary<UnitMeta.UnitType, Position> StratergyPostion = new Dictionary<UnitMeta.UnitType, Position>()
    { };
    [SerializeField] List<UnitMeta.UnitType> unitType;
    [SerializeField] List<Position> position;

    private Dictionary<int, List<UnitMeta.UnitType>> Stratergy = new Dictionary<int, List<UnitMeta.UnitType>>()
    { };
    [SerializeField] List<UnitMeta.UnitType> unitTypesList;
    [SerializeField] List<UnitMeta.UnitType> unitTypesList2;
    [SerializeField] List<UnitMeta.UnitType> unitTypesList3;

    [SerializeField] UnitMeta.UnitType attackType;
    private Dictionary<UnitMeta.UnitType, List<SpecialAttackType>> Sp = new Dictionary<UnitMeta.UnitType, List<SpecialAttackType>>()
    { };
    [SerializeField] List<SpecialAttackType> SpList1;
    [SerializeField] List<SpecialAttackType> SpList2;
    private void Start()
    {
        RTSplayer = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
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

        Sp.Add(unitType[0], SpList1);
        Sp.Add(unitType[1], SpList1);
        Sp.Add(unitType[2], SpList1);
        Sp.Add(unitType[3], SpList2);
        Sp.Add(unitType[4], SpList1);
        yield return null;

    }

    private IEnumerator HandleSpawnnEnemy()
    {
       yield return HandleDictionary();
        yield return new WaitForSeconds(2f);
        while (!ISGAMEOVER)
        {
            yield return new WaitForSeconds(2f);
           // Debug.Log($"HandleSpawnnEnemy {cards.Count}");
            
                //Debug.Log("HandleSpawnnEnemy");
                yield return SelectCard();
            yield return new WaitForSeconds(2f);
            yield return SpawnEnemy();
            //yield return SelectWallPos();
             
           
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
            yield return new WaitForSeconds(2f);
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
        i = 2;
        while (_card == null)
        {
            _card = GetCard(i);
            i--;
            if (i == -1) { break; }
        }
       // Debug.Log($"SelectCard {_card}");
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
            //Debug.Log($"{(int)card.cardFace.star} == {star}");
            if ((int)card.cardFace.star == star)
            {
                if(checkType(card, star))
                {
                    return card;
                }
            }
            
        }
        foreach (Card card in cards)
        {
            if ((int)card.cardFace.star == star)
            {
                if (checkWaitCard(card, star))
                {
                    return card;
                }
            }
        }
        
        return null;
    }
    private Card GetCard(int star)
    {
        foreach (Card card in cards)
        {
            if ((int)card.cardFace.star == star)
            {
               // Debug.Log("checkStar");
                return card;
            }
        }
        return null;
    }

    private bool checkType(Card card, int star)
    {
       // Debug.Log(star);
        int type = (int)card.cardFace.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length;
        Stratergy.TryGetValue(star, out var unitTypes);
       // Debug.Log($"checkType {unitTypes}");
        int i = 0;
        while (i < unitTypes.Count)
        {
            if ((UnitMeta.UnitType)type == unitTypes[i])
            {
                return true;
                break;
            }
            i++;
        }
       return false;
    }
    private bool checkWaitCard(Card card, int star)
    {
       // Debug.Log("checkWaitCard");
        int type = (int)card.cardFace.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length;
        int _star = star;
        while(_star < 3)
        {
            Stratergy.TryGetValue(star, out var unitTypes);
            int i = 0;
            while (i < unitTypes.Count)
            {
                if ((UnitMeta.UnitType)type == unitTypes[i])
                {
                    return true;
                    break;
                }
                i++;
            }
            _star++;
        }
        return false;
    }
    private IEnumerator SelectPos(Card card, UnitMeta.UnitType type)
    {
        Position position = Position.centre;
        int enemyInRight = 0;
        int enemyInLeft = 0;
        CardFace cardFace = card.cardFace;
        if(localFactory == null) { yield return SetLocalFactory(); }
        GameObject[] units = GameObject.FindGameObjectsWithTag("Player" + 0);
        GameObject king = GameObject.FindGameObjectWithTag("King" + 0);
        List<GameObject> armies = new List<GameObject>();
        armies = units.ToList();
        if (king != null)
            armies.Add(king);
        //Debug.Log($"armies.Count{armies.Count}");
        foreach(GameObject unit in armies)
        {
           // Debug.Log($"unit.transform.position {unit.transform.position} >= halfLine.position.x{halfLine.position.x}");
            if (unit.transform.position.x > halfLine.position.x)
            {
                enemyInRight++;
            }
            else
            {
                enemyInLeft++;
            }
          
        }
        
        if(type == attackType)
        {
            position = enemyInRight < enemyInLeft ? Position.right : Position.left;
        }
        else
        {
           //Debug.Log($"enemyInRight {enemyInRight} >= enemyInLeft{enemyInLeft}");
            position = enemyInRight >= enemyInLeft ? Position.right : Position.left;
        }
     
        //  if (StratergyPostion.TryGetValue(type, out var pos))
        // {
        if (!UnitMeta.UnitSize.TryGetValue((UnitMeta.UnitType)type, out int unitsize)) { unitsize = 1; }
            
            //Debug.Log(localFactory);
            Vector3 unitPos = new Vector3();
            GameObject[] Heros = GameObject.FindGameObjectsWithTag("Player" + 1);
            foreach (GameObject hero in Heros)
            {
               if (hero.GetComponent<Unit>().unitType == UnitMeta.UnitType.HERO)
               {
                  heroPos = hero.transform.position;
               }
            }
       // Debug.Log(position);
            switch (position)
            { 
                case Position.right:
                     unitPos = type == attackType ? new Vector3(heroPos.x, heroPos.y, heroPos.z-20) : new Vector3(heroPos.x, heroPos.y, heroPos.z+10);
                    break;
                case Position.left:
                unitPos = type == attackType ? new Vector3(heroPos.x-15, heroPos.y, heroPos.z -20) : new Vector3(heroPos.x-15, heroPos.y, heroPos.z + 10);
                    break;
                default:
                    GameObject[] Kings = GameObject.FindGameObjectsWithTag("King" + 1);
                    foreach (GameObject _king in Kings)
                    {
                        if (_king.GetComponent<Unit>().unitType == UnitMeta.UnitType.KING)
                        {
                            if (_king.transform.position.x < halfLine.position.x)
                            {
                                unitPos = type == attackType ? new Vector3(_king.transform.position.x, _king.transform.position.y, _king.transform.position.z-15) :
                                    new Vector3(_king.transform.position.x, _king.transform.position.y, _king.transform.position.z + 10);
                            }
                        }

                    }
                    break;
            }
        FindObjectOfType<TotalEleixier>().enemyEleixer -= card.GetUnitElexier();
                localFactory.CmdDropUnit(RTSplayer.GetEnemyID(), unitPos, StaticClass.enemyRace, (UnitMeta.UnitType)type, ((UnitMeta.UnitType)type).ToString(), unitsize, cardFace.stats.cardLevel,
                    cardFace.stats.health, cardFace.stats.attack, cardFace.stats.repeatAttackDelay, cardFace.stats.speed, cardFace.stats.defense, cardFace.stats.special, cardFace.stats.specialkey,
                    cardFace.stats.passivekey, (int)cardFace.star + 1, RTSplayer.GetTeamColor(), Quaternion.identity);
            enemyPlayer.moveCard(card.cardPlayerHandIndex);
            cardDealer.Hit(true);
            
       // }
        SpecialAttack(type);
        yield return null;
    }
    private void SpecialAttack(UnitMeta.UnitType type)
    {
        //Debug.Log("SpecialAttack");
        if(Sp.TryGetValue(type,out List<SpecialAttackType> specialAttackTypes))
        {
            //Debug.Log($"SpecialAttack Got {specialAttackTypes}");
            GameObject[] units = GameObject.FindGameObjectsWithTag("Player" + 1);
            GameObject king = GameObject.FindGameObjectWithTag("King" + 1);
            List<GameObject> armies = new List<GameObject>();
            armies = units.ToList();
            if (king != null)
                armies.Add(king);
            foreach (GameObject unit in armies)
            {
                //Debug.Log($"SpecialAttack Unit type is  {unit.GetComponent<Unit>().unitType}");
                if (unit.GetComponent<Unit>().unitType == UnitMeta.UnitType.HERO || unit.GetComponent<Unit>().unitType == UnitMeta.UnitType.KING)
                {
                    if (unit.GetComponent<CardStats>().specialkey != "")
                    {
                        SpecialAttackType specialAttackType = (SpecialAttackType)Enum.Parse(typeof(SpecialAttackType), unit.GetComponent<CardStats>().specialkey.ToUpper());
                        //Debug.Log($"SpecialAttack  type is  {specialAttackType}");
                        foreach (SpecialAttackType _specialAttackType in specialAttackTypes)
                        {
                            //Debug.Log($"SpecialAttack  type is  {specialAttackType} == {_specialAttackType}");
                            if (specialAttackType == _specialAttackType)
                            {
                                ISpecialAttack iSpecialAttack = unit.GetComponentInChildren(typeof(ISpecialAttack)) as ISpecialAttack;
                                SpButtonManager.enemyUnitBtns.TryGetValue(unit.GetComponent<Unit>().unitKey, out var btn);
                                // Debug.Log($"SpecialAttack  cost {iSpecialAttack.GetSpCost()} <= {btn.GetComponent<SpCostDisplay>().spCost / 3}");
                                if (iSpecialAttack.GetSpCost() <= btn.GetComponent<SpCostDisplay>().spCost / 3)
                                {
                                    //Debug.Log("Onpointerdown");
                                    iSpecialAttack.OnPointerDown();
                                }
                            }
                        }
                    }

                }                   
            }
        }
    }
    private IEnumerator SelectWallPos()
    {
        int enemyInRight = 0;
        int enemyInLeft = 0;
        GameObject[] units = GameObject.FindGameObjectsWithTag("Player" + 0);
        GameObject king = GameObject.FindGameObjectWithTag("King" + 0);
        List<GameObject> armies = new List<GameObject>();
        armies = units.ToList();
        if (king != null)
            armies.Add(king);
        //Debug.Log($"armies.Count{armies.Count}");
        foreach (GameObject unit in armies)
        {
            // Debug.Log($"unit.transform.position {unit.transform.position} >= halfLine.position.x{halfLine.position.x}");
            if (unit.GetComponent<Unit>().unitType == UnitMeta.UnitType.CAVALRY)
            {
                PlaceWall(unit.transform.position);
                yield return null;
            }
            /*  if (unit.transform.position.x > halfLine.position.x)
              {
                  enemyInRight++;
              }
              else
              {
                  enemyInLeft++;
              }
          }
          if(enemyInLeft < enemyInRight)
          {
            //  PlaceWall(heroPos);
          }*/
        }  yield return null;
    }
    private void PlaceWall(Vector3 pos)
    {
        Vector3 vector = new Vector3(pos.x+5, pos.y, pos.z);
        wall.DropUnit(vector);
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
                    //Debug.Log("SetLocalFactory");
                }
            }
        }
        yield return null;
    }

    private void Update()
    {
        
    }
}

