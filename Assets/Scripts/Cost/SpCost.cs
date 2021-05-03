using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpCost : NetworkBehaviour
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
        //SPImage = GameObject.FindGameObjectWithTag("SP Bar").GetComponent<Image>();
        //SPText = GameObject.FindGameObjectWithTag("SP Text").GetComponent<TextMeshProUGUI>();
        
    }
    [ClientRpc]
    public void RpcUpdateSPAmount(int cost, GameObject unit)
    {
        UpdateSPAmount(cost, unit.GetComponent<Unit>());
    }
    public void UpdateSPAmount(int cost,Unit unit)
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        Debug.Log($"unit:{unit}");
        //Debug.Log("UpdateSPAmount");
        //SPAmount += cost;
        //SPText.text = (string)SPAmount.ToString();
        //SPImage.fillAmount = (float)SPAmount / MaxSpCost;
        if (unit != null)
        {
            if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)//1 player mode
            {
                if (unit.CompareTag("Player0") || unit.CompareTag("King0"))
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
            }
            else
            {
                Debug.Log($"unit:{unit.tag}player:{player.GetPlayerID()}");

                if (unit.CompareTag("Player" + player.GetPlayerID()) || unit.CompareTag("King" + player.GetPlayerID()))
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
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
