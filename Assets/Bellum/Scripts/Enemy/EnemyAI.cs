using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using static SpecialAttackDict;

public class EnemyAI : MonoBehaviour
{
    private bool enemyBreakWall = false;
    private bool usingCard = true;
    private bool FinishDealEnemyCard = false;
    private bool usedTatical = false;
    private bool canSpawnUnit = true;
    private bool savingCardForDefend = false;
    private bool handledDict = false;
    private int elexier = 0;
    private Card nextCard;
    private Card urgentSpawn;
    private RTSPlayer RTSplayer;
    private UnitFactory localFactory;
    private bool ISGAMEOVER = false;
    public List<Card> cards = new List<Card>();
    private List<SpCostDisplay> spCostDisplay = new List<SpCostDisplay>();
    private enum Difficulty { OneStar, TwoStar, ThreeStar,StatUp };
    private enum Position { left, right, centre };
    private Vector3 heroPos;
    private int mission;
    private int chapter;
    private float progressImageVelocity;
    [SerializeField] private bool dragCard = true;
    [SerializeField] private float cardSizeUpFactor = 0.625f;
    [SerializeField] private TacticalBehavior TB;
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

    [SerializeField] Vector3 startLeftPos;
    [SerializeField] Vector3 startRightPos;
    [SerializeField] Vector3 startCentrePos;

    [SerializeField] UnitMeta.UnitType attackType;
    private Dictionary<UnitMeta.UnitType, List<SpecialAttackType>> Sp = new Dictionary<UnitMeta.UnitType, List<SpecialAttackType>>()
    { };
    private Dictionary<int, List<SpecialAttackType>> Chapter = new Dictionary<int, List<SpecialAttackType>>()
    { };
    [SerializeField] int statUpFactor;
    private Dictionary<string, int> Mission = new Dictionary<string, int>()
    {
        {"1",1},
        {"2",2},
        {"3",3},
        {"4",4}
    };
    [SerializeField] List<SpecialAttackType> SpList1;
    [SerializeField] List<SpecialAttackType> SpList2;
    float test;
    private void Start()
    {
        if (((RTSNetworkManager)NetworkManager.singleton).Players.Count != 1) { return; }
        RTSplayer = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        CardDealer.FinishDealEnemyCard += StartSpawnnEnemy;
        TotalEleixier.UpdateEnemyElexier += OnUpdateElexier;
        GameOverHandler.ClientOnGameOver += HandleGameOver;
        Unit.ClientOnUnitSpawned += UrgentDefend;
        Unit.ClientOnUnitDespawned += Rage;
        InvokeRepeating("HandleSpawnEnemyBackUp", 10, 10);
    }
    private void OnDestroy()
    {
        CardDealer.FinishDealEnemyCard -= StartSpawnnEnemy;
        TotalEleixier.UpdateEnemyElexier -= OnUpdateElexier;
        GameOverHandler.ClientOnGameOver -= HandleGameOver;
        Unit.ClientOnUnitSpawned -= UrgentDefend;
        Unit.ClientOnUnitDespawned -= Rage;
        UnitWeapon.GateOpened -= GateOpen;
        UnitProjectile.GateOpened -= GateOpen;
        if (GameObject.FindGameObjectWithTag("King1"))
        {
            GameObject.FindGameObjectWithTag("King1").GetComponent<Health>().ClientOnHealthUpdated -= OnHealthUpdated;
        }
    }
    private void StartSpawnnEnemy()
    {
        FinishDealEnemyCard = true;
        StartCoroutine(HandleSpawnnEnemy());
    }
    private IEnumerator HandleDictionary(List<UnitMeta.UnitType> factor)
    {
        if (handledDict == false)
        {
            handledDict = true;
            StratergyPostion.Add(unitType[0], position[0]);
            StratergyPostion.Add(unitType[1], position[1]);
            StratergyPostion.Add(unitType[2], position[2]);
            StratergyPostion.Add(unitType[3], position[3]);
            StratergyPostion.Add(unitType[4], position[4]);

            factor.Add(UnitMeta.UnitType.CAVALRY);
            Stratergy.Add(0, unitTypesList);
            Stratergy.Add(1, unitTypesList2);
            Stratergy.Add(2, unitTypesList3);

            Sp.Add(unitType[0], SpList1);
            Sp.Add(unitType[1], SpList1);
            Sp.Add(unitType[2], SpList1);
            Sp.Add(unitType[3], SpList2);
            Sp.Add(unitType[4], SpList1);

            GameObject.FindGameObjectWithTag("King1").GetComponent<Health>().ClientOnHealthUpdated += OnHealthUpdated;
            UnitWeapon.GateOpened += GateOpen;
            UnitProjectile.GateOpened += GateOpen;
        }
        
      /*  Mission.Add(1, Difficulty.OneStar);
        Mission.Add(2, Difficulty.TwoStar);
        Mission.Add(3, Difficulty.ThreeStar);
        Mission.Add(4, Difficulty.StatUp);*/
        yield return null;

    }
    
    private IEnumerator HandleSpawnnEnemy()
    {
        List<UnitMeta.UnitType> lists = HandleMission();
        yield return HandleDictionary(lists);
        //yield return new WaitForSeconds(2f);
        while (!ISGAMEOVER)
        {
            yield return new WaitForSeconds(2.5f);
            if (localFactory == null) { yield return SetLocalFactory(); }

            // Debug.Log($"HandleSpawnnEnemy {cards.Count}");
            if (enemyBreakWall == false)
            {
                SpawnSpesificUnit(UnitMeta.UnitType.FOOTMAN, Vector3.zero);
            }
            if (canSpawnUnit == true)
            {//Debug.Log("HandleSpawnnEnemy");
                 yield return SelectCard(true);
                 yield return new WaitForSeconds(2f);
                  yield return SpawnEnemy();
              
                //if (timer <= 0) { localFactory.GetComponent<Player>().dragCardMerge(); }
            }
           
                usingCard = true;
                //yield return SelectWallPos(); 
        }
        //yield return new WaitForSeconds(1f);

        //Debug.Log("End game");
        yield return null;
    }
    private void HandleSpawnEnemyBackUp()
    {
        if(usingCard == false)
        {
            //Debug.Log("glichted");
            StopCoroutine(HandleSpawnnEnemy());
            StartCoroutine(HandleSpawnnEnemy());
        }
        usingCard = false;
    }
    private void Rage(Unit unit)
    {
       // Debug.Log($"tag == {unit.tag} type = {unit.unitType} use T {usedTatical}");
        //Need to change Tag;
        if (unit.CompareTag("Die1") && unit.unitType == UnitMeta.UnitType.HERO  && usedTatical == false)
        {
            if (chapter >= 4 && mission >= 3)
            {
                usedTatical = true;
                TB.taticalAttack(TacticalBehavior.TaticalAttack.CAVALRYCHARGES, RTSplayer.GetEnemyID());
            }
            else if(chapter >= 4 && mission == 2)
            {
                usedTatical = true;
                TB.taticalAttack(TacticalBehavior.TaticalAttack.ABSOLUTEDEFENSE, RTSplayer.GetEnemyID());
            }
            else if(chapter >= 4 && mission == 1)
            { 
                usedTatical = true;
                TB.taticalAttack(TacticalBehavior.TaticalAttack.ARROWRAIN, RTSplayer.GetEnemyID());
            }
        }
    }
    private void UrgentDefend(Unit unit)
    {
        StartCoroutine(Defend(unit));
    }
    private void OnHealthUpdated(int currentHealth, int maxHealth, int lastDamageDeal)
    {
        if (currentHealth <= maxHealth / 2 && chapter == 3 && usedTatical == false)
        {
            usedTatical = true;
            TB.taticalAttack(TacticalBehavior.TaticalAttack.SPINATTACK, RTSplayer.GetEnemyID());
        }
    }
    private IEnumerator Defend(Unit unit)
    {
        while (unit.CompareTag("Unit"))
        {
            yield return new WaitForSeconds(0.5f);
            if(unit == null) { break; }
        }
        // Debug.Log($"unit.unitType{unit.unitType} tag == {unit.tag}");
        // Debug.Log(savingCardForDefend);
        if (savingCardForDefend == false)
        {
            if (unit !=null && unit.unitType == UnitMeta.UnitType.FOOTMAN && unit.CompareTag("Sneaky0"))
            {
                savingCardForDefend = true;
                canSpawnUnit = false;
                int i = 3;
                yield return SelectCard(true);
                //Debug.Log($"unit == {unit.name} next card {nextCard}");
                while (unit.transform.position.z < halfLine.position.z || elexier < nextCard.GetUnitElexier())
                {
                    yield return new WaitForSeconds(1f);
                    i--;
                    //  Debug.Log($"i == {i}");

                    if (i == 0)
                    {
                        // Debug.Log("break");
                        canSpawnUnit = true;
                        savingCardForDefend = false;
                        break;
                    }
                }
                if (i > 0)
                {
                    if (localFactory == null) { yield return SetLocalFactory(); }
                    int type = (int)nextCard.cardFace.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length;
                    SpawnUnit(unit.transform.position, nextCard, (UnitMeta.UnitType)type);
                    canSpawnUnit = true;
                    savingCardForDefend = false;
                }
            }
        }
        if (unit.unitType == UnitMeta.UnitType.CAVALRY && unit.CompareTag("Player0") && chapter >= 2)
        {
            canSpawnUnit = false;
            while (elexier < wall.GetUnitElexier())
            {
                yield return new WaitForSeconds(1f);
            }
            Vector3 vector3 = GameObject.FindGameObjectWithTag("King1").transform.position;
            PlaceWall(new Vector3(vector3.x, vector3.y, vector3.z - 5));
            canSpawnUnit = true;
        }
    }
    public void GateOpen(string playerID)
    {
        Debug.Log($"player ID is {playerID}");
        if(playerID == "1")
        {
            enemyBreakWall = true;
        }
    }
    private void SpawnSpesificUnit(UnitMeta.UnitType unitType ,Vector3 pos)
    {
        for(int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            int type = (int)card.cardFace.numbers % System.Enum.GetNames(typeof(UnitMeta.UnitType)).Length;
            if ((UnitMeta.UnitType)type == unitType)
            {
                if( elexier < card.GetUnitElexier())
                {
                    if (pos == Vector3.zero)
                    {
                        SpawnUnit(GameObject.FindGameObjectWithTag("King1").transform.position, card, (UnitMeta.UnitType)type);
                    }
                    else
                    {
                        SpawnUnit(pos, card, (UnitMeta.UnitType)type);
                    }
                }

            }
        }
    }
    private List<UnitMeta.UnitType> HandleMission()
    {
        string missions;
        string chapters;
        chapters = StaticClass.Chapter;
        missions = StaticClass.Mission;
       
        Mission.TryGetValue(missions, out int _mission);
        Mission.TryGetValue(chapters, out int _chapter);
        chapter = _chapter;
        mission = _mission;
        switch (mission)
        {
            case 1:
                statUpFactor = 1;
                return unitTypesList;
            case 2:
                statUpFactor = 1;
                return unitTypesList2;
            case 3:
                statUpFactor = 1;
                return unitTypesList3;
            default:
                if (localFactory == null) { SetLocalFactory(); }
                GameObject[] units = GameObject.FindGameObjectsWithTag("Player" + 1);
                GameObject king = GameObject.FindGameObjectWithTag("King" + 1);
                List<GameObject> armies = new List<GameObject>();
                armies = units.ToList();
                if (king != null)
                    armies.Add(king);
                foreach (GameObject unit in armies)
                {
                    //Debug.Log($"AI Spawn Emeny {unit.name} {RTSplayer.GetTeamEnemyColor()}");
                    CardStats cardStats = unit.GetComponent<CardStats>();
                   // cardStats.attack *= statUpFactor;
                   // cardStats.health *= statUpFactor;
                   // cardStats.defense *= statUpFactor;
                    unit.GetComponent<UnitPowerUp>().PowerUp(1, unit.name, unit.GetComponent<Unit>().GetSpawnPointIndex(), cardStats.star, cardStats.cardLevel, cardStats.health * statUpFactor,
                        cardStats.attack * statUpFactor, cardStats.repeatAttackDelay, cardStats.speed, cardStats.defense * statUpFactor, cardStats.special, cardStats.specialkey,
                        cardStats.passivekey, RTSplayer.GetTeamEnemyColor());
                }
                if (chapter == 4)
                {
                    TB.taticalAttack(TacticalBehavior.TaticalAttack.CAVALRYCHARGES, RTSplayer.GetEnemyID());
                }
                return unitTypesList3;
        }
    }
    public void SetCards(Card card)
    {
       // Debug.Log($"Enemy card list add {card.cardFace.numbers} ");
        //Debug.Log($" is enemy {card.enemyCard}");
        //if (card.GetComponentInParent<Player>()) { Debug.Log($"belong to {card.GetComponentInParent<Player>().name}"); }
        
        this.cards.Add(card);
        StartCoroutine(SameCard());
    }
    private IEnumerator SameCard()
    {
        if(dragCard == true && FinishDealEnemyCard == true)
        {
            //Debug.Log("SameCard");
            List<Card> Archers = new List<Card>();
            List<Card> Cavalrys = new List<Card>();
            List<Card> Footmans = new List<Card>();
            List<Card> Magics = new List<Card>();
            List<Card> Tanks = new List<Card>();
            List<List<Card>> numOfEachCard = new List<List<Card>>();
            numOfEachCard.Add(Archers);
            numOfEachCard.Add(Cavalrys);
            numOfEachCard.Add(Footmans);
            numOfEachCard.Add(Magics);
            numOfEachCard.Add(Tanks);
            foreach (Card card in cards) 
            {
                switch (card.cardFace.numbers)
                {
                    case Card_Numbers.ARCHER:
                        Archers.Add(card);
                        //Debug.Log("Add Archers");
                        break;
                    case Card_Numbers.CAVALRY:
                        Cavalrys.Add(card);
                        //Debug.Log("Add Cavalrys");
                        break;
                    case Card_Numbers.FOOTMAN:
                        Footmans.Add(card);
                        //Debug.Log("Add Footmans");
                        break;
                    case Card_Numbers.MAGIC:
                        Magics.Add(card);
                        //Debug.Log("Add Magics");
                        break;
                    case Card_Numbers.TANK:
                        Tanks.Add(card);
                        //Debug.Log("Add Tanks");
                        break;
                }
            }
            foreach(List<Card> cards in numOfEachCard)
            {
                if(cards.Count >= 2)
                {
                    //Debug.Log($"{cards[0].cardFace.numbers} has over 2 card");
                    yield return SameStar(cards);
                }
            }
        }
        yield return enemyPlayer.mergeCard();
        yield return null;
    }
    private IEnumerator SameStar(List<Card> cards)
    {
       // int i = 0;
        
            Card beforeNewCard = cards[0];
            Card card = cards[1];
            //Debug.Log($"{beforeNewCard.cardFace.numbers} is {beforeNewCard.cardFace.star} == {card.cardFace.numbers} is {card.cardFace.star}");
            //if (beforeNewCard == card) { Debug.Log("Card"); }
            if (beforeNewCard.cardFace.star == card.cardFace.star)
            {
                yield return DragCard(this.cards.IndexOf(beforeNewCard), this.cards.IndexOf(card));
            }
        
        yield return null;
    }
    private IEnumerator DragCard(int beforeNewCard, int card )
    {
       
        Vector2 original = cards[beforeNewCard].GetComponent<RectTransform>().anchoredPosition;
        //Card finishedCard = card == cards.Count - 1 ? cards[card - 1] : cards[card + 1];
        Card finishedCard = cards[card];
        //Vector2 finishPos = finishedCard.GetComponentInParent<CardSlot>().GetComponentInParent<RectTransform>().anchoredPosition;
        //Debug.Log($"DragCard {original} {finishPos}");
        float timer = 1f;

       float x = cards[beforeNewCard].transform.position.x;
        float y = cards[beforeNewCard].transform.position.y;
        float PosX = finishedCard.transform.position.x;
        //Debug.Log($"Drag card {enemyPlayer.cardSlotlist.IndexOf(cards[beforeNewCard].GetComponentInParent<CardSlot>())} < {enemyPlayer.cardSlotlist.IndexOf(finishedCard.GetComponentInParent<CardSlot>())}");
        if (enemyPlayer.cardSlotlist.IndexOf(cards[beforeNewCard].GetComponentInParent<CardSlot>()) < enemyPlayer.cardSlotlist.IndexOf(finishedCard.GetComponentInParent<CardSlot>()))
        {
            
            while (enemyPlayer.cardSlotlist.IndexOf(cards[beforeNewCard].GetComponentInParent<CardSlot>()) <= enemyPlayer.cardSlotlist.IndexOf(finishedCard.GetComponentInParent<CardSlot>()))
            {
                //Debug.Log("right");
                timer -= Time.deltaTime;
                //Debug.Log($"pos = {cards[beforeNewCard].GetComponent<RectTransform>().anchoredPosition} time left = {timer}");
                //Debug.Log($"card slot index = {enemyPlayer.cardSlotlist.IndexOf(cards[beforeNewCard].GetComponentInParent<CardSlot>())} going to {enemyPlayer.cardSlotlist.IndexOf(finishedCard.GetComponentInParent<CardSlot>())}");
                //trying to use crad slot recttransform for the ending.
                x = Mathf.SmoothDamp(cards[beforeNewCard].transform.position.x,
                    PosX, ref progressImageVelocity, 1f);
                cards[beforeNewCard].transform.position = new Vector3(x, y, 0);
                StartCoroutine(cards[beforeNewCard].GetComponent<DragCard>().ShiftCard());
                if (timer <= 0)
                {
                    cards[beforeNewCard].GetComponent<DragCard>().OnEndDrag(null);
                    break;
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
        else if(enemyPlayer.cardSlotlist.IndexOf(cards[beforeNewCard].GetComponentInParent<CardSlot>()) > enemyPlayer.cardSlotlist.IndexOf(finishedCard.GetComponentInParent<CardSlot>()))
        {
            while(enemyPlayer.cardSlotlist.IndexOf(cards[beforeNewCard].GetComponentInParent<CardSlot>()) >= enemyPlayer.cardSlotlist.IndexOf(finishedCard.GetComponentInParent<CardSlot>()))
            {
                //Debug.Log("left");
                timer -= Time.deltaTime;
                //Debug.Log($"pos = {cards[beforeNewCard].GetComponent<RectTransform>().anchoredPosition} time left = {timer}");
                //Debug.Log($"card slot index = {enemyPlayer.cardSlotlist.IndexOf(cards[beforeNewCard].GetComponentInParent<CardSlot>())} going to {enemyPlayer.cardSlotlist.IndexOf(finishedCard.GetComponentInParent<CardSlot>())}");
                x = Mathf.SmoothDamp(cards[beforeNewCard].transform.position.x,
                    PosX, ref progressImageVelocity, 1f);
                cards[beforeNewCard].transform.position = new Vector3(x, y, 0);
                StartCoroutine(cards[beforeNewCard].GetComponent<DragCard>().ShiftCard());
                if (timer <= 0)
                {
                    cards[beforeNewCard].GetComponent<DragCard>().OnEndDrag(null);
                    break;
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
        
     

        //dealManagers.GetComponent<CardDealer>().Hit(true);
        yield return null;
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
    private IEnumerator SelectCard(bool needScale)
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
        //Debug.Log($"SelectCard {_card}");
        
            nextCard = _card;
            RectTransform rect = nextCard.GetComponent<RectTransform>();
            //float x = rect.localScale.x;
            //float y = rect.localScale.y;
            //float z = rect.localScale.z;
            rect.localScale = new Vector3(cardSizeUpFactor, cardSizeUpFactor, cardSizeUpFactor);
        
       
       // Debug.Log($"Select card {nextCard.cardFace.numbers}");
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
                }
                i++;
            }
            _star++;
        }
        return false;
    }
    private IEnumerator SelectPos(Card card, UnitMeta.UnitType type)
    {
        CardFace cardFace = card.cardFace;
        if (localFactory == null) { yield return SetLocalFactory(); }
        
        Vector3 unitPos = new Vector3(0,0,0);
        List<GameObject> LeftSideUnits = new List<GameObject>();
        List<GameObject> RightSideUnits = new List<GameObject>();
        GameObject[] units = GameObject.FindGameObjectsWithTag("Player" + 0);
        GameObject king = GameObject.FindGameObjectWithTag("King" + 0);
        List<GameObject> armies = new List<GameObject>();
        armies = units.ToList();
        if (king != null)
            armies.Add(king);
        foreach (GameObject unit in armies)
        {
            // Debug.Log($"unit.transform.position {unit.transform.position} >= halfLine.position.x{halfLine.position.x}");
            if (unit.transform.position.x > halfLine.position.x)
            {
                RightSideUnits.Add(unit);
            }
            else
            {
                LeftSideUnits.Add(unit);
            }

        }
        Position position = RightSideUnits.Count >= LeftSideUnits.Count ? Position.right : Position.left;
        if (type == attackType) { position = Position.centre; }
        //Debug.Log($"Position{position}");
        switch (position)
        {
            case Position.right:
                foreach (GameObject unit in RightSideUnits)
                {
                    //Debug.Log($"unit.transform.position {unit.transform.position} >= halfLine.position.z{halfLine.position.z}");
                    if (unit.transform.position.z > halfLine.position.z)
                    {
                        unitPos = unit.transform.position;
                        break;
                    }
                }
                if (unitPos == new Vector3(0, 0, 0))
                {
                   // Debug.Log("Using startRightPos");
                    unitPos = startRightPos;
                }
                break;
            case Position.left:
                foreach (GameObject unit in LeftSideUnits)
                {
                    //Debug.Log($"unit.transform.position {unit.transform.position} >= halfLine.position.z{halfLine.position.z}");
                    if (unit.transform.position.z > halfLine.position.z)
                    {
                        unitPos = unit.transform.position;
                        break;
                    }
                }
                if (unitPos == new Vector3(0, 0, 0))
                {
                   // Debug.Log("Using startLeftPos");
                    unitPos = startLeftPos;
                }
                break;
            default:
                //Debug.Log("Using startCentrePos");
                unitPos = startCentrePos;
                break;
        }
        SpawnUnit(unitPos,card,type);
         yield return null;
    }
    private void SpawnUnit(Vector3 unitPos,Card card, UnitMeta.UnitType type)
    {
        CardFace cardFace = card.cardFace;
        if (!UnitMeta.UnitSize.TryGetValue((UnitMeta.UnitType)type, out int unitsize)) { unitsize = 1; }
        FindObjectOfType<TotalEleixier>().enemyEleixer -= card.GetUnitElexier();
        localFactory.CmdDropUnit(RTSplayer.GetEnemyID(), unitPos, StaticClass.enemyRace, (UnitMeta.UnitType)type, ((UnitMeta.UnitType)type).ToString(), unitsize, cardFace.stats.cardLevel,
            cardFace.stats.health * (int)statUpFactor, cardFace.stats.attack * (int)statUpFactor, cardFace.stats.repeatAttackDelay, cardFace.stats.speed, cardFace.stats.defense * (int)statUpFactor, cardFace.stats.special, cardFace.stats.specialkey,
            cardFace.stats.passivekey, (int)cardFace.star + 1, RTSplayer.GetTeamEnemyColor() , Quaternion.identity);
        card.enemyCard = false;
        card.ResetScale();
        //cards.Remove(card);
        enemyPlayer.moveCard(card.cardPlayerHandIndex);
        cardDealer.Hit(true);

        // }
        SpecialAttack(type);
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
                                //Debug.Log($"SpecialAttack  cost {iSpecialAttack.GetSpCost()} <= {btn.GetComponent<SpCostDisplay>().spCost / 3}");
                                if(iSpecialAttack != null && btn != null)
                                {
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
    public void RemoveCard(Card card)
    {
        cards.Remove(card);
    }
    private void Update()
    {
        //test = Mathf.SmoothDamp(test, 100, ref progressImageVelocity, 0.5f);
        //Debug.Log(test);
    }
}

