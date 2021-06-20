using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
namespace DigitalRuby.ThunderAndLightning
{
    public class Lightning : MonoBehaviour, ISpecialAttack
    {
        [SerializeField] private GameObject LightlingPrefab;
        [SerializeField] private LayerMask layerMask = new LayerMask();
        [SerializeField] private GameObject attackPoint;
        [SerializeField] private int electicDamage = 10;
        [SerializeField] private int electicShockDamage = 10;

        private int enemyCount = 0;
        public int attackRange = 100;
        public int minAttackRange = 1000;
        public int maxAttackrange = 400;
        public int SPCost = 5;
        private float lightlingTimer;

        private bool SpawnedButton;

        private bool IsSuperAttack = false;

        private Button SPButton;
        private SpCost spCost;
        private RTSPlayer player;
        private TacticalBehavior TB;
        private GameObject searchPoint;
        private GameObject hitCollider;
        private GameObject lightling;
        private GameObject lightlingChild;
        private List<GameObject> targetList = new List<GameObject>();
        private List<GameObject> startPointList = new List<GameObject>();
        private List<GameObject> lightlingList = new List<GameObject>();



        // Start is called before the first frame update
        public void Start()
        {
               player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            //if (CompareTag("King" + player.GetEnemyID()) || CompareTag("Player" + player.GetEnemyID())) { return; }
            /* SpawnedButton = FindObjectOfType<SpButton>().InstantiateSpButton(SpecialAttackDict.SpecialAttackType.Lightling, GetComponent<Unit>());
            if (SpawnedButton) { SPButton = FindObjectOfType<SpButton>().GetButton(GetComponent<Unit>().SpBtnTicket).GetComponent<Button>(); }
            if (SPButton == null) { return; }
            SPButton.onClick.RemoveAllListeners();
            SPButton.onClick.AddListener(OnPointerDowns);*/
            spCost = FindObjectOfType<SpCost>();
            TB = GameObject.FindGameObjectWithTag("TacticalSystem").GetComponent<TacticalBehavior>();
        }

        public void OnPointerDown()
        {
            //Debug.Log(layerMask);
            targetList.Clear();
            startPointList.Clear();
            lightlingList.Clear();
            if (!SpButtonManager.unitBtn.TryGetValue(GetComponentInParent<Unit>().unitKey, out Button btn))
            {
                SpButtonManager.enemyUnitBtns.TryGetValue(GetComponentInParent<Unit>().unitKey, out var _btn);
                btn = _btn;
            }
            if (spCost.useSpCost == true)
                {
                    //if (spCost.SPAmount < SPCost) { return; }
                    if ((btn.GetComponent<SpCostDisplay>().spCost / 3) < SPCost) { return; }
                    StartCoroutine(btn.GetComponent<SpCostDisplay>().MinusSpCost(SPCost));
                    spCost.UpdateSPAmount(-SPCost, null);
                }
            

            searchPoint = gameObject.transform.parent.gameObject;
            GameObject closestTarget = null;
            bool haveTarget = true;
            var distance = float.MaxValue;
            var localDistance = 0f;

            while (haveTarget == true)
            {
                //Debug.Log("OnPointerDown1");
                startPointList.Add(searchPoint);
                bool findedTarget = false;
                //Search target in a distance
                Collider[] hitColliders = Physics.OverlapBox(searchPoint.transform.position, transform.localScale * attackRange, Quaternion.identity, layerMask);
                int i = 0;
                //Debug.Log(hitColliders.Length);
                while (i < hitColliders.Length)
                {
                    
                    distance = float.MaxValue;
                    hitCollider = hitColliders[i++].transform.gameObject;
                   // Debug.Log($"OnPointerDown2 {(hitCollider.transform.position - searchPoint.transform.position).sqrMagnitude} < {distance} {!targetList.Contains(hitCollider)}");
                    //Debug.Log($"searched target {hitCollider.name}");
                    // check If the target is cloestest to king && it is not in the same team && check if it already finded the target
                    if ((localDistance = (hitCollider.transform.position - searchPoint.transform.position).sqrMagnitude) < distance && !targetList.Contains(hitCollider))
                    {
                        int id;
                        if (transform.parent.CompareTag("Player1") || transform.parent.CompareTag("King1"))
                        {
                             id = 0;
                        }
                        else
                        {
                             id = ((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1 ? 1 : player.GetPlayerID() == 0 ? 1 : 0;
                        }
                        
                       // Debug.Log($"OnPointerDown3 {hitCollider.tag} Player + {id}");
                       
                        if (hitCollider.CompareTag("Player" + id) || hitCollider.CompareTag("King" + id))
                        {
                           // Debug.Log("OnPointerDown4");
                            //if (localDistance < maxAttackrange)
                            //{
                            if (localDistance < distance)
                                {

                                    findedTarget = true;
                                    distance = localDistance;
                                    closestTarget = hitCollider;
                                    // Move the searchPoint to the next target, so it will not search at the same point


                                }

                            //}
                        }
                    }
                }
                //Debug.Log($"{searchPoint.name} -- > {localDistance}, --> {hitCollider.name}");
                searchPoint = closestTarget;
                // if there is no more target is finded then break
                if (findedTarget == false)
                {
                    break;
                }

                targetList.Add(closestTarget);
            }
            searchPoint = this.gameObject;
            // if it doesnot find any target return
            if (closestTarget == null) { return; }
            for (int a = 0; a < targetList.ToArray().Length; a++)
            {

                Lightlings(startPointList.ToArray()[a], targetList.ToArray()[a]);
            }
            lightlingTimer = 5;
        }
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
            if (true)
            {
                //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
                Gizmos.DrawWireCube(transform.parent.transform.position, transform.localScale * attackRange);
            }
        }
        public void Lightlings(GameObject startPoint, GameObject endPoint)
        {
            
            GameObject lightlingChilds;
            if (enemyCount == 0)
            {
                lightling = Instantiate(LightlingPrefab);
                lightlingChild = lightling.transform.GetChild(0).gameObject;
                lightlingChild.transform.position = new Vector3(startPoint.transform.position.x, startPoint.transform.position.y + 5, startPoint.transform.position.z);
                lightling.GetComponent<LightningBoltPathScriptBase>().LightningPath.Add(lightlingChild);
            }
            lightlingChilds = Instantiate(lightlingChild, lightling.transform);
            lightlingChilds.transform.position = new Vector3(endPoint.transform.position.x, endPoint.transform.position.y + 5, endPoint.transform.position.z);
            lightling.GetComponent<LightningBoltPathScriptBase>().LightningPath.Add(lightlingChilds);
            lightlingList.Add(lightlingChilds);
            enemyCount++;
            endPoint.GetComponent<Health>().OnElectricShock(electicDamage, electicShockDamage);
            endPoint.transform.GetComponent<UnitAnimator>().StateControl(UnitAnimator.AnimState.GETHIT);


        }
        public void Lightlinggs(GameObject startPoint, GameObject endPoint)
        {
            Instantiate(LightlingPrefab, transform);
        }
        public void Update()
        {
            if (lightlingTimer > 0)
            {
                lightlingTimer -= Time.deltaTime;
            }
            else
            {
                foreach (GameObject light in lightlingList)
                {
                    Destroy(light);
                }
                foreach (GameObject target in targetList)
                {
                    if (target != null)
                    {
                        if (target.TryGetComponent<Health>(out Health health))
                        {
                            health.IsElectricShock = false;
                        }
                    }
                }
            }
        }
        public int GetSpCost()
        {
            return SPCost;
        }
    }
}