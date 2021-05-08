using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpCost : MonoBehaviour
{
    public bool useSpCost = true;
    [HideInInspector]public int SPAmount = 0;
    private int MaxSpCost;
    private UnitMeta.UnitKey unitKey;
    //private Image SPImage;
    //private TMP_Text SPText;
    private RTSPlayer player;
    
    // Start is called before the first frame update
    void Start()
    {
        MaxSpCost = SPAmount;
        Unit.ClientOnUnitDespawned += Handlekill;
        //SPImage = GameObject.FindGameObjectWithTag("SP Bar").GetComponent<Image>();
        //SPText = GameObject.FindGameObjectWithTag("SP Text").GetComponent<TextMeshProUGUI>();

    }
    private void OnDestroy()
    {
        Unit.ClientOnUnitDespawned -= Handlekill;
    }
    private void Handlekill(Unit unit)
    {
        //Debug.Log("Handlekill");
        UpdateSPAmount(1, unit);
    }
    //[ClientRpc]
   // public void RpcUpdateSPAmount(int cost, GameObject unit)
   // {
     //   UpdateSPAmount(cost, unit.GetComponent<Unit>());
    //}
    public void UpdateSPAmount(int cost,Unit unit)
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        //Debug.Log($"unit:{unit}");
        //Debug.Log("UpdateSPAmount");
        //SPAmount += cost;
        //SPText.text = (string)SPAmount.ToString();
        //SPImage.fillAmount = (float)SPAmount / MaxSpCost;
        if (unit != null)
        {
            if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)//1 player mode
            {
                if (unit.CompareTag("Player1") || unit.CompareTag("King1"))
                {
                    bool GettedValue = SpButtonManager.unitBtn.TryGetValue(unit.unitKey, out Button btn);
                    if (GettedValue == true)
                    {
                        if (btn.GetComponent<SpCostDisplay>().useTimer == false)
                        {
                            StartCoroutine(btn.GetComponent<SpCostDisplay>().AddSpCost());
                        }
                    }
                    else
                    {

                        foreach (Button button in SpButtonManager.buttons)
                        {
                            if (button.GetComponent<SpCostDisplay>().useTimer == false)
                            {
                                StartCoroutine(button.GetComponent<SpCostDisplay>().AddSpCost());
                            }
                        }
                    }
                }
                else
                {
                    if (SpButtonManager.enemyUnitBtn.TryGetValue(GetComponentInParent<Unit>().unitKey, out GameObject obj))
                    {
                        if (obj.GetComponent<EnemySpManager>().useTimer == false)
                        {
                            obj.GetComponent<EnemySpManager>().ChangeSPCost(-cost);
                        }
                    }
                }
            }
            else
            {
                bool GettedValue = SpButtonManager.enemyUnitBtn.TryGetValue(unit.unitKey, out GameObject obj);
                if (GettedValue == true)
                {
                    if (obj.GetComponent<EnemySpManager>().useTimer == false)
                    {
                        obj.GetComponent<EnemySpManager>().ChangeSPCost(1);
                    }
                }
                else
                {

                    foreach (GameObject gameobj in SpButtonManager.enemySp)
                    {
                        if (gameobj.GetComponent<EnemySpManager>().useTimer == false)
                        {
                            gameobj.GetComponent<EnemySpManager>().ChangeSPCost(1);
                        }
                    }
                }
            }
            }
            else
            {
                Debug.Log($"unit:{unit.tag}player:{player.GetPlayerID()}");

                if (unit.CompareTag("Player" + player.GetEnemyID()) || unit.CompareTag("King" + player.GetEnemyID()))
                {
                    bool GettedValue = SpButtonManager.unitBtn.TryGetValue(unit.unitKey, out Button btn);
                    if (GettedValue == true)
                    {
                        if (btn.GetComponent<SpCostDisplay>().useTimer == false)
                        {
                            StartCoroutine(btn.GetComponent<SpCostDisplay>().AddSpCost());
                        }
                    }
                    else
                    {
                    Debug.Log($"button{SpButtonManager.buttons.Count}");
                        foreach (Button button in SpButtonManager.buttons)
                        {
                            if (button.GetComponent<SpCostDisplay>().useTimer == false)
                            {
                                StartCoroutine(button.GetComponent<SpCostDisplay>().AddSpCost());
                            }
                        }
                    }
                }
            }
        }
    }